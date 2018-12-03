using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class EffectiveRestriction : IEffectiveRestriction
    {
        private readonly StartTimeLimitation _startTimeLimitation;
        private readonly EndTimeLimitation _endTimeLimitation;
        private readonly WorkTimeLimitation _workTimeLimitation;
        private IShiftCategory _shiftCategory;
        private IDayOffTemplate _dayOff;
        private IAbsence _absence;
        private readonly IDayOffTemplate _invalidDayOff;
        private readonly IShiftCategory _invalidCategory;
        private readonly IAbsence _invalidAbsence;
        private readonly IList<IActivityRestriction> _activityRestrictionCollection;
	    private static readonly DateOnly ExitDate = new DateOnly(2050, 1, 1);

	    public EffectiveRestriction(
            StartTimeLimitation startTimeLimitation,
            EndTimeLimitation endTimeLimitation,
            WorkTimeLimitation workTimeLimitation,
            IShiftCategory shiftCategory,
            IDayOffTemplate dayOff,
            IAbsence absence,
            IList<IActivityRestriction> activityRestrictionCollection
            )
        {
            InParameter.NotNull(nameof(activityRestrictionCollection), activityRestrictionCollection);

            _startTimeLimitation = startTimeLimitation;
            _endTimeLimitation = endTimeLimitation;
            _workTimeLimitation = workTimeLimitation;
            _shiftCategory = shiftCategory;
            _dayOff = dayOff;
            _absence = absence;
            _invalidDayOff = new DayOffTemplate(new Description("__invalid__"));
            _invalidCategory = new ShiftCategory("__invalid__");
            _invalidAbsence = new Absence();
            _activityRestrictionCollection = activityRestrictionCollection;
        }

	    public EffectiveRestriction()
	    {
			_startTimeLimitation = new StartTimeLimitation();
			_endTimeLimitation = new EndTimeLimitation();
			_workTimeLimitation = new WorkTimeLimitation();
		    _shiftCategory = null;
		    _dayOff = null;
		    _absence = null;
			_invalidDayOff = new DayOffTemplate(new Description("__invalid__"));
			_invalidCategory = new ShiftCategory("__invalid__");
			_invalidAbsence = new Absence();
			_activityRestrictionCollection = new List<IActivityRestriction>();
	    }

		#region Implementation of IIWorkTimeMinMaxRestriction

		public bool MayMatchWithShifts()
		{
			var available = !NotAvailable;
			var noDayOff = DayOffTemplate == null;
			var noAbsence = Absence == null;
			return available && noDayOff && noAbsence;
		}

		public bool MayMatchBlacklistedShifts()
		{
			return IsRestriction;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Match(IShiftCategory shiftCategory)
		{
			if (ShiftCategory == null)
				return true;
			return shiftCategory.Equals(ShiftCategory);
		}

		public bool Match(IWorkShiftProjection workShiftProjection)
		{
			return ValidateWorkShiftInfo(workShiftProjection);
		}

		#endregion

		public StartTimeLimitation StartTimeLimitation => _startTimeLimitation;

	    public EndTimeLimitation EndTimeLimitation => _endTimeLimitation;

	    public WorkTimeLimitation WorkTimeLimitation => _workTimeLimitation;

	    public IAbsence Absence
        {
            get { return _absence; }
            set { _absence = value; }
        }

        // This got a setter because in the option dialog when scheduling one can select a Shift Category
        public IShiftCategory ShiftCategory
        {
            get { return _shiftCategory; }
            set { _shiftCategory = value;}
        }
        // This got a setter because an availability doesn't have a DayOffTempalte but when not available
        // we need to set one
        public IDayOffTemplate DayOffTemplate
        {
            get { return _dayOff; }
            set { _dayOff = value; }
        }
		
		public bool ValidateWorkShiftInfo(IWorkShiftProjection workShiftProjection)
        {
			TimePeriod workShiftTimePeriod = workShiftProjection.TimePeriod;

            if(!StartTimeLimitation.IsValidFor(workShiftTimePeriod.StartTime))
                return false;

				if (!EndTimeLimitation.IsValidFor(workShiftTimePeriod.EndTime))
                return false;

            TimeSpan contractTime = workShiftProjection.ContractTime;
				if (!WorkTimeLimitation.IsValidFor(contractTime))
                return false;

            if (ShiftCategory != null)
            {
                if (!ShiftCategory.Id.Value.Equals(workShiftProjection.ShiftCategoryId))
                    return false;
            }

            if (DayOffTemplate != null)
            {
                return false;
            }

            if(Absence != null)
            {
                return false;
            }

            TimeZoneInfo tzInfo = TimeZoneInfo.Utc;
            if (!VisualLayerCollectionSatisfiesActivityRestriction(WorkShift.BaseDateOnly, tzInfo, workShiftProjection.Layers))
            {
                return false;
            }

            return true;
        }

        public IEffectiveRestriction Combine(IEffectiveRestriction effectiveRestriction)
        {
			if (effectiveRestriction == null)
				return null;
            IDayOffTemplate dayOff = resolveDayOff(effectiveRestriction.DayOffTemplate);
            IShiftCategory cat = resolveShiftCategory(effectiveRestriction.ShiftCategory);
            var absence = resolveAbsence(effectiveRestriction.Absence);

            foreach (var restriction in effectiveRestriction.ActivityRestrictionCollection)
            {
                _activityRestrictionCollection.Add(restriction);
            }
            
            if(dayOff != null)
            {
                if (dayOff.Equals(_invalidDayOff))
					return null;
            }

            if (cat != null)
            {
                if (cat.Equals(_invalidCategory))
					return null;
            }

            if(absence != null)
            {
                if (absence.Equals(_invalidAbsence))
					return null;
            }

            TimeSpan? start = resolveTime(_startTimeLimitation.StartTime, effectiveRestriction.StartTimeLimitation.StartTime, false);
            TimeSpan? end = resolveTime(_startTimeLimitation.EndTime, effectiveRestriction.StartTimeLimitation.EndTime, true);
            if (start.HasValue && end.HasValue)
            {
                if (start.Value > end.Value)
					return null;
            }
            var startTimeLimitation = new StartTimeLimitation(start, end);

            start = resolveTime(_endTimeLimitation.StartTime, effectiveRestriction.EndTimeLimitation.StartTime, false);
            end = resolveTime(_endTimeLimitation.EndTime, effectiveRestriction.EndTimeLimitation.EndTime, true);
            if (start.HasValue && end.HasValue)
            {
                if (start.Value > end.Value)
					return null;
            }
            var endTimeLimitation = new EndTimeLimitation(start, end);

            //If we have both and the End conflicts with the we must change them so they can work together
            if (startTimeLimitation.HasValue() && endTimeLimitation.HasValue() ) //&& endTimeLimitation.StartTime < startTimeLimitation.EndTime)
            {
                if (endTimeLimitation.EndTime < startTimeLimitation.StartTime)
					return null;

                if (startTimeLimitation.EndTime > endTimeLimitation.EndTime)
						 startTimeLimitation = new StartTimeLimitation(startTimeLimitation.StartTime, endTimeLimitation.EndTime);

                if (endTimeLimitation.StartTime < startTimeLimitation.StartTime)
						 endTimeLimitation = new EndTimeLimitation(startTimeLimitation.StartTime, endTimeLimitation.EndTime);
            }
            start = resolveTime(_workTimeLimitation.StartTime, effectiveRestriction.WorkTimeLimitation.StartTime, false);
            end = resolveTime(_workTimeLimitation.EndTime, effectiveRestriction.WorkTimeLimitation.EndTime, true);
            if (start.HasValue && end.HasValue)
            {
                if (start.Value > end.Value)
					return null;
            }
            var workTimeLimitation = new WorkTimeLimitation(start, end);

            var ret = new EffectiveRestriction(startTimeLimitation, endTimeLimitation, workTimeLimitation,
                                           cat, dayOff, absence, _activityRestrictionCollection);
        	ret.NotAvailable = NotAvailable;
            if (effectiveRestriction.IsRotationDay || IsRotationDay)
                ret.IsRotationDay = true;
			if (effectiveRestriction.IsAvailabilityDay || IsAvailabilityDay)
				ret.IsAvailabilityDay = true;
			if (effectiveRestriction.IsPreferenceDay || IsPreferenceDay)
				ret.IsPreferenceDay = true;
			if (effectiveRestriction.IsStudentAvailabilityDay || IsStudentAvailabilityDay)
				ret.IsStudentAvailabilityDay = true;
			if (effectiveRestriction.NotAvailable)
				ret.NotAvailable = true;
	        ret.CommonActivity = CommonActivity;
	        ret.CommonMainShift = CommonMainShift;

	        if (effectiveRestriction.CommonMainShift != null)
	        {
				if (CommonMainShift == null || areMainShiftsEqual(CommonMainShift, effectiveRestriction.CommonMainShift))
				{
					ret.CommonMainShift = effectiveRestriction.CommonMainShift; 
		        }
		        else
		        {
					ret = null;
		        }
	        }
			
			if (effectiveRestriction.CommonActivity != null)
	        {
				if (CommonActivity == null || CommonActivity.Equals(effectiveRestriction.CommonActivity))
		        {
			        ret.CommonActivity = effectiveRestriction.CommonActivity;
		        }
		        else
		        {
					ret = null;
		        }
	        }
			
	        return ret;

        }

		private static bool areMainShiftsEqual(IEditableShift original, IEditableShift current)
		{
			IScheduleDayEquator scheduleDayEquator = new ScheduleDayEquator(new EditableShiftMapper());
			return scheduleDayEquator.MainShiftBasicEquals(original, current);
		}

        private static TimeSpan? resolveTime(TimeSpan? thisTime, TimeSpan? otherTime, bool min)
        {
            if (!thisTime.HasValue && !otherTime.HasValue)
                return null;

            if (thisTime.HasValue && !otherTime.HasValue)
                return thisTime;

            if (!thisTime.HasValue)
                return otherTime;

            if (min)
                return otherTime < thisTime ? otherTime : thisTime;

            return otherTime > thisTime ? otherTime : thisTime;
        }

        private IAbsence resolveAbsence(IAbsence other)
        {
            if (_absence == null && other == null)
                return null;
            if (_absence == null)
                return other;

            if (other == null)
                return _absence;

            if (_absence.Equals(other))
                return _absence;

            return _invalidAbsence;   
        }

        private IShiftCategory resolveShiftCategory(IShiftCategory other)
        {
            if (_shiftCategory == null && other == null)
                return null;
            if (_shiftCategory == null)
                return other;

            if (other == null)
                return _shiftCategory; 

            if (_shiftCategory.Equals(other))
                return _shiftCategory;

            return _invalidCategory;
        }

        private IDayOffTemplate resolveDayOff(IDayOffTemplate other)
        {
            if (_dayOff == null && other == null)
                return null;
            if (_dayOff == null)
                return other;

            if (other == null)
                return _dayOff;

            if (_dayOff.Equals(other))
                return _dayOff;

            return _invalidDayOff;
        }

        public bool IsRotationDay { get; set; }

        public bool IsAvailabilityDay { get; set; }

        public bool IsPreferenceDay { get; set; }

        public bool IsStudentAvailabilityDay { get; set; }

        public bool NotAvailable { get; set; }

        
        public IList<IActivityRestriction> ActivityRestrictionCollection => _activityRestrictionCollection;

		public bool VisualLayerCollectionSatisfiesActivityRestriction(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IEnumerable<IActivityRestrictableVisualLayer> layers)
        {
			if (scheduleDayDateOnly == ExitDate)
				return false;

	        var layersLookup = layers.ToLookup(k => k.ActivityId);
            if (!layersLookup.Any())
                return false;
			
            foreach (IActivityRestriction activityRestriction in _activityRestrictionCollection)
            {
                bool result = visualLayerCollectionSatisfiesOneActivityRestriction(agentTimeZone,
																				   layersLookup, activityRestriction);
                if(!result)
                    return false;
            }

            return true;
        }
		
        public bool IsRestriction => IsAvailabilityDay || IsPreferenceDay || IsRotationDay || IsStudentAvailabilityDay;

	    private static bool visualLayerCollectionSatisfiesOneActivityRestriction(TimeZoneInfo agentTimeZone, ILookup<Guid, IActivityRestrictableVisualLayer> layerCollection, IActivityRestriction activityRestriction)
        {
            IActivityRestriction actRestriction = activityRestriction;
            
			var layers = layerCollection[activityRestriction.Activity.Id.GetValueOrDefault()];
			foreach (var layer in layers)
            {
            	TimePeriod period = layer.Period.TimePeriod(agentTimeZone);

				//starts too early
				if (actRestriction.StartTimeLimitation.StartTime.HasValue && period.StartTime < actRestriction.StartTimeLimitation.StartTime.Value)
					continue;
                //starts too late
                if (actRestriction.StartTimeLimitation.EndTime.HasValue && period.StartTime > actRestriction.StartTimeLimitation.EndTime.Value)
                    continue;
                //ends too early
                if (actRestriction.EndTimeLimitation.StartTime.HasValue && period.EndTime < actRestriction.EndTimeLimitation.StartTime.Value)
                    continue;
				//ends too late
				if (actRestriction.EndTimeLimitation.EndTime.HasValue && period.EndTime > actRestriction.EndTimeLimitation.EndTime.Value)
					continue;
                //too short
	            var spanningTime = period.SpanningTime();
	            if (actRestriction.WorkTimeLimitation.StartTime.HasValue && spanningTime < actRestriction.WorkTimeLimitation.StartTime.Value)
                    continue;
                // too long
				if (actRestriction.WorkTimeLimitation.EndTime.HasValue && spanningTime > actRestriction.WorkTimeLimitation.EndTime.Value)
                    continue;

                return true;
            }
            return false;
        }

		public override int GetHashCode()
		{
			unchecked
			{
				var result = 0;
				result = (result * 398) ^ StartTimeLimitation.GetHashCode();
				result = (result * 398) ^ EndTimeLimitation.GetHashCode();
				result = (result * 398) ^ WorkTimeLimitation.GetHashCode();
				if (ShiftCategory != null)
					result = (result * 398) ^ ShiftCategory.GetHashCode();
				if (CommonMainShift != null)
					result = (result * 398) ^ CommonMainShift.GetHashCode();
				if (CommonActivity != null)
					result = (result * 398) ^ CommonActivity.GetHashCode();
				if (DayOffTemplate != null)
					result = (result * 398) ^ DayOffTemplate.GetHashCode();
				if (Absence != null)
					result = (result * 398) ^ Absence.GetHashCode();
				foreach (IActivityRestriction activityRestriction in ActivityRestrictionCollection)
				{
					result = (result * 398) ^ activityRestriction.Activity.GetHashCode();
					result = (result * 398) ^ activityRestriction.StartTimeLimitation.GetHashCode();
					result = (result * 398) ^ activityRestriction.EndTimeLimitation.GetHashCode();
					result = (result * 398) ^ activityRestriction.WorkTimeLimitation.GetHashCode();
				}
				result = (result * 398) ^ NotAvailable.GetHashCode();
				result = (result * 398) ^ IsAvailabilityDay.GetHashCode();
				return result;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is EffectiveRestriction restriction && Equals(restriction);
		}

		public bool Equals(EffectiveRestriction restriction)
		{
			return restriction.IsAvailabilityDay == IsAvailabilityDay &&
				   restriction.IsPreferenceDay == IsPreferenceDay &&
				   restriction.IsRestriction == IsRestriction &&
				   restriction.IsRotationDay == IsRotationDay &&
				   restriction.IsStudentAvailabilityDay == IsStudentAvailabilityDay &&
				   restriction.NotAllowedForDayOffs == NotAllowedForDayOffs &&
				   restriction.NotAvailable == NotAvailable &&
				   (restriction.ShiftCategory?.Equals(ShiftCategory) ?? restriction.ShiftCategory == null && ShiftCategory == null) &&
				   (restriction.Absence?.Equals(Absence) ?? restriction.Absence == null && Absence == null) &&
				   (restriction.DayOffTemplate?.Equals(DayOffTemplate) ??
					restriction.DayOffTemplate == null && DayOffTemplate == null) &&
				   restriction._startTimeLimitation.Equals(_startTimeLimitation) &&
				   restriction._endTimeLimitation.Equals(_endTimeLimitation) &&
				   restriction._workTimeLimitation.Equals(_workTimeLimitation) &&
				   restriction._activityRestrictionCollection.Count == _activityRestrictionCollection.Count &&
				   restrictionCollectionsAreEqual(restriction._activityRestrictionCollection, _activityRestrictionCollection) &&
				   ((restriction.CommonActivity == null && CommonActivity == null) ||
					(restriction.CommonActivity != null && CommonActivity != null &&
					 restriction.CommonActivity.GetHashCode() == CommonActivity.GetHashCode())) &&
				   ((restriction.CommonMainShift == null && CommonMainShift == null) ||
					(restriction.CommonMainShift != null && CommonMainShift != null &&
					 restriction.CommonMainShift.GetHashCode() == CommonMainShift.GetHashCode()));
		}

	    private bool restrictionCollectionsAreEqual(IList<IActivityRestriction> collection1, IList<IActivityRestriction> collection2)
	    {
		    for (int i = 0; i < collection1.Count; i++)
		    {
			    if (!collection1[i].Activity.Equals(collection2[i].Activity))
				    return false;
				if (!collection1[i].StartTimeLimitation.Equals(collection2[i].StartTimeLimitation))
					return false;
				if (!collection1[i].EndTimeLimitation.Equals(collection2[i].EndTimeLimitation))
					return false;
				if (!collection1[i].WorkTimeLimitation.Equals(collection2[i].WorkTimeLimitation))
					return false;
			}
		    return true;
	    }

	    public bool NotAllowedForDayOffs { get; set; }
	    public IEditableShift CommonMainShift { get; set; }
	    public CommonActivity CommonActivity { get; set; }
    }
}
