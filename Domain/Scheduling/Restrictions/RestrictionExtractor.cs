using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class RestrictionExtractor : RestrictionExtractorWithoutStateHolder, IRestrictionExtractor
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;

		public RestrictionExtractor(ISchedulingResultStateHolder resultStateHolder)
        {
            _resultStateHolder = resultStateHolder;
        }

        private IScheduleDictionary ScheduleDictionary {get { return _resultStateHolder.Schedules; }}

		public void Extract(IPerson person, DateOnly dateOnly)
		{
			var scheduleDay = GetScheduleDay(person, dateOnly);
			Extract(scheduleDay);
		}

		private IScheduleDay GetScheduleDay(IPerson person, DateOnly dateOnly)
		{
			IScheduleDay schedulePart = null;
			IScheduleRange scheduleRange = ScheduleDictionary[person];
			if (scheduleRange != null)
			{
				schedulePart = scheduleRange.ScheduledDay(dateOnly);
			}
			return schedulePart;
		}
	}

    public class RestrictionExtractorWithoutStateHolder : IRestrictionExtractorWithoutStateHolder
    {
        private readonly IList<IAvailabilityRestriction> _availabilityRestrictions = new List<IAvailabilityRestriction>();
        private readonly IList<IRotationRestriction> _rotationRestrictions = new List<IRotationRestriction>();
        private readonly IList<IStudentAvailabilityDay> _studentAvailabilityDays = new List<IStudentAvailabilityDay>();
        private readonly IList<IPreferenceRestriction> _preferenceRestrictions = new List<IPreferenceRestriction>();

        public IEnumerable<IAvailabilityRestriction> AvailabilityList
        {
            get { return _availabilityRestrictions; }
        }

        public IEnumerable<IRotationRestriction> RotationList
        {
            get { return _rotationRestrictions; }
        }

        public IEnumerable<IStudentAvailabilityDay> StudentAvailabilityList
        {
            get { return _studentAvailabilityDays; }
        }

        public IEnumerable<IPreferenceRestriction> PreferenceList
        {
            get { return _preferenceRestrictions; }
        }

        public void Extract(IScheduleDay schedulePart)
        {
            ClearLists();
            ExtractDay(schedulePart);
        }

    	protected void ExtractDay(IScheduleDay scheduleDay)
        {
        	if (scheduleDay == null) return;

        	var restrictions = scheduleDay.RestrictionCollection();
        	var operation = new RestrictionRetrievalOperation();
        	operation.GetRotationRestrictions(restrictions).ForEach(_rotationRestrictions.Add);
        	operation.GetAvailabilityRestrictions(restrictions).ForEach(_availabilityRestrictions.Add);
        	operation.GetPreferenceRestrictions(restrictions).ForEach(_preferenceRestrictions.Add);
        	operation.GetStudentAvailabilityDays(scheduleDay).ForEach(_studentAvailabilityDays.Add);
        }

    	protected void ClearLists()
        {
            _availabilityRestrictions.Clear();
            _rotationRestrictions.Clear();
            _studentAvailabilityDays.Clear();
            _preferenceRestrictions.Clear();
        }

        public IEffectiveRestriction CombinedRestriction(ISchedulingOptions schedulingOptions)
        {
            var start = new StartTimeLimitation();
            var end = new EndTimeLimitation();
            var time = new WorkTimeLimitation();

            var extractor = new InnerRestrictionExtractor(schedulingOptions,this, new RestrictionCombiner());

            IEffectiveRestriction initial = new EffectiveRestriction(start, end, time, null, null, null, new List<IActivityRestriction>());
            return extractor.Extract(initial);
        }
    }

    internal class InnerRestrictionExtractor
    {
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IRestrictionExtractorWithoutStateHolder _restrictionExtractor;
    	private readonly IRestrictionCombiner _restrictionCombiner;

    	public InnerRestrictionExtractor(ISchedulingOptions schedulingOptions, IRestrictionExtractorWithoutStateHolder restrictionExtractor, IRestrictionCombiner restrictionCombiner)
        {
            _schedulingOptions = schedulingOptions;
            _restrictionExtractor = restrictionExtractor;
        	_restrictionCombiner = restrictionCombiner;
        }

        public IEffectiveRestriction Extract(IEffectiveRestriction effectiveRestriction)
        {
            if (_schedulingOptions.UseRotations)
            {
                effectiveRestriction = ExtractRotations(effectiveRestriction);
                if (effectiveRestriction == null) return effectiveRestriction;
            }

            if (_schedulingOptions.UsePreferences)
            {
                effectiveRestriction = ExtractPreferences(effectiveRestriction);
                if (effectiveRestriction == null) return effectiveRestriction;
            }
			// if it IsLimitedWorkday at this point we shall not add a dayoff 
			if (isLimitedWorkday(effectiveRestriction))
				effectiveRestriction.NotAllowedForDayOffs = true;

            if (_schedulingOptions.UseAvailability)
            {
                effectiveRestriction = ExtractAvailabilities(effectiveRestriction);
                if (effectiveRestriction == null) return effectiveRestriction;
            }

            if (_schedulingOptions.UseStudentAvailability)
            {
                effectiveRestriction = ExtractStudentAvailabilities(effectiveRestriction);
                if (effectiveRestriction == null) return effectiveRestriction;
            }

            return effectiveRestriction;
        }

		private static bool isLimitedWorkday(IEffectiveRestriction effectiveRestriction)
        {            
            if (effectiveRestriction.ShiftCategory != null)
                return true;
            if (effectiveRestriction.ActivityRestrictionCollection.Count > 0)
                return true;

			return (effectiveRestriction.StartTimeLimitation.HasValue() || effectiveRestriction.EndTimeLimitation.HasValue() || effectiveRestriction.WorkTimeLimitation.HasValue());  
        }

        private IEffectiveRestriction ExtractStudentAvailabilities(IEffectiveRestriction effectiveRestriction)
        {
        	return _restrictionCombiner.CombineStudentAvailabilityDays(_restrictionExtractor.StudentAvailabilityList, effectiveRestriction);
        }

        private IEffectiveRestriction ExtractAvailabilities(IEffectiveRestriction effectiveRestriction)
        {
			return _restrictionCombiner.CombineAvailabilityRestrictions(_restrictionExtractor.AvailabilityList, effectiveRestriction);
        }

        private IEffectiveRestriction ExtractPreferences(IEffectiveRestriction effectiveRestriction)
        {
			return _restrictionCombiner.CombinePreferenceRestrictions(_restrictionExtractor.PreferenceList, effectiveRestriction, _schedulingOptions.UsePreferencesMustHaveOnly);
        }

        private IEffectiveRestriction ExtractRotations(IEffectiveRestriction effectiveRestriction)
        {
        	return _restrictionCombiner.CombineRotationRestrictions(_restrictionExtractor.RotationList, effectiveRestriction);
        }
    }
}
