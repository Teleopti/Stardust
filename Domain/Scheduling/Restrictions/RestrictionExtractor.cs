using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{

    public class RestrictionExtractor : IRestrictionExtractor
    {
        private readonly ISchedulingResultStateHolder _resultStateHolder;
        private readonly IList<IAvailabilityRestriction> _availList = new List<IAvailabilityRestriction>();
        private readonly IList<IRotationRestriction> _rotList = new List<IRotationRestriction>();
        private readonly IList<IStudentAvailabilityDay> _studentAvailabilityList = new List<IStudentAvailabilityDay>();
        private readonly IList<IPreferenceRestriction> _preferenceList = new List<IPreferenceRestriction>();
        

        public RestrictionExtractor(ISchedulingResultStateHolder resultStateHolder)
        {
            _resultStateHolder = resultStateHolder;
        }
        private IScheduleDictionary ScheduleDictionary {get { return _resultStateHolder.Schedules; }}

        public IEnumerable<IAvailabilityRestriction> AvailabilityList
        {
            get { return _availList; }
        }

        public IEnumerable<IRotationRestriction> RotationList
        {
            get { return _rotList; }
        }

        public IEnumerable<IStudentAvailabilityDay> StudentAvailabilityList
        {
            get { return _studentAvailabilityList; }
        }

        public IEnumerable<IPreferenceRestriction> PreferenceList
        {
            get { return _preferenceList; }
        }

        public void Extract(IScheduleDay schedulePart)
        {
            ClearLists();
            ExtractDay(schedulePart);
        }

        private void ExtractDay(IScheduleDay schedulePart)
        {
            if (schedulePart != null)
            {
                var restrictions = schedulePart.RestrictionCollection();
                restrictions.FilterBySpecification(RestrictionMustBe.Rotation).ForEach(rot => _rotList.Add((IRotationRestriction)rot));
                restrictions.FilterBySpecification(RestrictionMustBe.Availability).ForEach(rot => _availList.Add((IAvailabilityRestriction)rot));
                restrictions.FilterBySpecification(RestrictionMustBe.Preference).ForEach(rot => _preferenceList.Add((IPreferenceRestriction)rot));

                schedulePart.PersonRestrictionCollection().OfType<IStudentAvailabilityDay>().ForEach(_studentAvailabilityList.Add);
            }
        }

        private void ClearLists()
        {
            _availList.Clear();
            _rotList.Clear();
            _studentAvailabilityList.Clear();
            _preferenceList.Clear();
        }

        public void Extract(IPerson person, DateOnly dateOnly)
        {
            ClearLists();

            IScheduleDay schedulePart = GetScheduleDay(person, dateOnly);
            ExtractDay(schedulePart);
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

        public void ExtractFromGroupPerson(IGroupPerson groupPerson, DateOnly dateOnly)
        {
            InParameter.NotNull("groupPerson", groupPerson);

            ClearLists();
            foreach (var person in groupPerson.GroupMembers)
            {
                var scheduleDay = GetScheduleDay(person, dateOnly);
                ExtractDay(scheduleDay);
            }

        }

        public IEffectiveRestriction CombinedRestriction(ISchedulingOptions schedulingOptions)
        {
            var start = new StartTimeLimitation();
            var end = new EndTimeLimitation();
            var time = new WorkTimeLimitation();

            InnerRestrictionExtractor innerRestrictionExtractor = new InnerRestrictionExtractor(schedulingOptions,this);

            IEffectiveRestriction ret = new EffectiveRestriction(start, end, time, null, null, null, new List<IActivityRestriction>());
            return innerRestrictionExtractor.Extract(ret);
        }
    }

    internal class InnerRestrictionExtractor
    {
        private readonly ISchedulingOptions _schedulingOptions;
        private readonly IRestrictionExtractor _restrictionExtractor;

        public InnerRestrictionExtractor(ISchedulingOptions schedulingOptions, IRestrictionExtractor restrictionExtractor)
        {
            _schedulingOptions = schedulingOptions;
            _restrictionExtractor = restrictionExtractor;
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

        private IEffectiveRestriction ExtractStudentAvailabilities(IEffectiveRestriction effectiveRestriction)
        {
            foreach (IStudentAvailabilityDay restriction in _restrictionExtractor.StudentAvailabilityList)
            {
                if (restriction.RestrictionCollection.Count > 0)
                {
                    effectiveRestriction.IsStudentAvailabilityDay = true;
                    var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
                    studentAvailabilityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.MaxValue, null);
                    studentAvailabilityRestriction.EndTimeLimitation = new EndTimeLimitation(null, TimeSpan.Zero);
                    foreach (var restrict in restriction.RestrictionCollection)
                    {
                        if (!restrict.StartTimeLimitation.StartTime.HasValue || restrict.StartTimeLimitation.StartTime < studentAvailabilityRestriction.StartTimeLimitation.StartTime)
                            studentAvailabilityRestriction.StartTimeLimitation = restrict.StartTimeLimitation;
                        if (!restrict.EndTimeLimitation.EndTime.HasValue || restrict.EndTimeLimitation.EndTime > studentAvailabilityRestriction.EndTimeLimitation.EndTime)
                            studentAvailabilityRestriction.EndTimeLimitation = restrict.EndTimeLimitation;
                    }
                    effectiveRestriction = effectiveRestriction.Combine(new EffectiveRestriction(studentAvailabilityRestriction.StartTimeLimitation,
                                                               studentAvailabilityRestriction.EndTimeLimitation,
                                                               studentAvailabilityRestriction.WorkTimeLimitation,
                                                               null, null, null, new List<IActivityRestriction>()));
                    if (effectiveRestriction == null) return effectiveRestriction;
                }

                if (restriction.NotAvailable) effectiveRestriction.NotAvailable = true;
            }
			
			if (_restrictionExtractor.StudentAvailabilityList.IsEmpty())
				effectiveRestriction.NotAvailable = true;

            return effectiveRestriction;
        }

        private IEffectiveRestriction ExtractAvailabilities(IEffectiveRestriction effectiveRestriction)
        {
            foreach (IAvailabilityRestriction restriction in _restrictionExtractor.AvailabilityList)
            {
                if (restriction.IsRestriction())
                {
                    effectiveRestriction.IsAvailabilityDay = true;

                	var newEffectiverestriction = new EffectiveRestriction(restriction.StartTimeLimitation,
                	                                                       restriction.EndTimeLimitation,
                	                                                       restriction.WorkTimeLimitation,
                	                                                       null, null, null,
                	                                                       new List<IActivityRestriction>());
                	newEffectiverestriction.NotAvailable = restriction.NotAvailable;
					effectiveRestriction = effectiveRestriction.Combine(newEffectiverestriction);
                    if (effectiveRestriction == null) return effectiveRestriction;
                }
				//if (restriction.NotAvailable)
				//    effectiveRestriction.NotAvailable = true;
            }
            return effectiveRestriction;
        }

        private IEffectiveRestriction ExtractPreferences(IEffectiveRestriction effectiveRestriction)
        {
            foreach (var restriction in _restrictionExtractor.PreferenceList)
            {
                if (_schedulingOptions.UsePreferencesMustHaveOnly && !restriction.MustHave) continue;

                if (restriction.IsRestriction())
                {
                    effectiveRestriction.IsPreferenceDay = true;
                    IShiftCategory shiftCategory = restriction.ShiftCategory;
                    IDayOffTemplate dayOff = restriction.DayOffTemplate;
                    var absence = restriction.Absence;

                    effectiveRestriction = effectiveRestriction.Combine(new EffectiveRestriction(restriction.StartTimeLimitation,
                                                               restriction.EndTimeLimitation,
                                                               restriction.WorkTimeLimitation,
                                                               shiftCategory, dayOff, absence,
                                                               new List<IActivityRestriction>(restriction.ActivityRestrictionCollection)));
                    if (effectiveRestriction == null) return effectiveRestriction;
                }
            }
            return effectiveRestriction;
        }

        private IEffectiveRestriction ExtractRotations(IEffectiveRestriction effectiveRestriction)
        {
            foreach (IRotationRestriction restriction in _restrictionExtractor.RotationList)
            {
                if (restriction.IsRestriction())
                {
                    effectiveRestriction.IsRotationDay = true;
                    effectiveRestriction = effectiveRestriction.Combine(new EffectiveRestriction(restriction.StartTimeLimitation,
                                                               restriction.EndTimeLimitation,
                                                               restriction.WorkTimeLimitation,
                                                               restriction.ShiftCategory,
                                                               restriction.DayOffTemplate,
                                                               null,
                                                               new List<IActivityRestriction>()));
                    if (effectiveRestriction == null)
                        return effectiveRestriction;
                }
            }
            return effectiveRestriction;
        }
    }
}
