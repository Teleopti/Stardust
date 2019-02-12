using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ShiftProjectionCacheFilter
    {
	    private readonly ILongestPeriodForAssignmentCalculator _rules;
	    private readonly IPersonalShiftAndMeetingFilter _personalShiftAndMeetingFilter;
	    private readonly INotOverWritableActivitiesShiftFilter _notOverWritableActivitiesShiftFilter;

	    public ShiftProjectionCacheFilter(ILongestPeriodForAssignmentCalculator rules, IPersonalShiftAndMeetingFilter personalShiftAndMeetingFilter, INotOverWritableActivitiesShiftFilter notOverWritableActivitiesShiftFilter)
        {
        	_rules = rules;
	        _personalShiftAndMeetingFilter = personalShiftAndMeetingFilter;
	        _notOverWritableActivitiesShiftFilter = notOverWritableActivitiesShiftFilter;
        }

    	public IList<ShiftProjectionCache> FilterOnRestrictionAndNotAllowedShiftCategories(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, 
            IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction, HashSet<IShiftCategory> notAllowedCategories)
        {
            if (restriction == null)
            {
                shiftList.Clear();
                return shiftList;
            }

             shiftList = FilterOnShiftCategory(restriction.ShiftCategory, shiftList);

             shiftList = FilterOnNotAllowedShiftCategories(notAllowedCategories, shiftList);

             shiftList = FilterOnRestrictionTimeLimits(scheduleDayDateOnly, agentTimeZone, shiftList, restriction);

             shiftList = FilterOnActivityRestrictions(scheduleDayDateOnly, agentTimeZone, shiftList, restriction);

             return FilterOnRestrictionMinMaxWorkTime(shiftList, restriction);
           }

        public IList<ShiftProjectionCache> FilterOnMainShiftOptimizeActivitiesSpecification(IList<ShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification)
        {
			if (shiftList == null || mainShiftActivitiesOptimizeSpecification == null) return new List<ShiftProjectionCache>();

	        IList<ShiftProjectionCache> ret =
		        shiftList.Where(s => mainShiftActivitiesOptimizeSpecification.IsSatisfiedBy(s.TheMainShift)).ToArray();

		    return ret;
        }
		
        public static IList<ShiftProjectionCache> FilterOnActivityRestrictions(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction)
        {
            IList<IActivityRestriction> activityRestrictions = restriction.ActivityRestrictionCollection;
            if (activityRestrictions.Count == 0)
                return shiftList;

	        IList<ShiftProjectionCache> workShiftsWithActivity =
		        shiftList.Where(
			        s => restriction.VisualLayerCollectionSatisfiesActivityRestriction(scheduleDayDateOnly, agentTimeZone,
				        s.MainShiftProjection().OfType<IActivityRestrictableVisualLayer>())).ToArray();

            return workShiftsWithActivity;
        }

        public bool CheckRestrictions(SchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction)
        {
            if (effectiveRestriction == null)
            {
                return false;
            }

            if(effectiveRestriction.ShiftCategory != null && schedulingOptions.ShiftCategory != null)
            {
                if(effectiveRestriction.ShiftCategory.Id != schedulingOptions.ShiftCategory.Id)
                {
                    return false;
                }
            }
            bool haveRestrictions = true;
            
            if (schedulingOptions.RotationDaysOnly && !effectiveRestriction.IsRotationDay)
            {
                haveRestrictions = false;
            }
            if (schedulingOptions.AvailabilityDaysOnly && !effectiveRestriction.IsAvailabilityDay)
            {
                haveRestrictions = false;
            }
            if (schedulingOptions.PreferencesDaysOnly && !effectiveRestriction.IsPreferenceDay)
            {
                haveRestrictions = false;
            }

			if (schedulingOptions.UsePreferencesMustHaveOnly && !effectiveRestriction.IsPreferenceDay)
			{
				haveRestrictions = false;
			}

            if (schedulingOptions.UseStudentAvailability && !effectiveRestriction.IsStudentAvailabilityDay)
            {
                haveRestrictions = false;
            }
        
            if (!haveRestrictions)
            {
                return false;
            }

            return true;
        }

        public IList<ShiftProjectionCache> FilterOnRestrictionTimeLimits(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<ShiftProjectionCache> shiftList,
                                                                   IEffectiveRestriction restriction)
        {
            if (shiftList.Count == 0)
                return shiftList;
            IList<ShiftProjectionCache> workShiftsWithinPeriod = shiftList;
  
            if (restriction.StartTimeLimitation.HasValue() || restriction.EndTimeLimitation.HasValue())
            {
                TimeSpan startStart = TimeSpan.Zero;
                if (restriction.StartTimeLimitation.StartTime.HasValue)
                    startStart = restriction.StartTimeLimitation.StartTime.Value;

                TimeSpan endEnd = restriction.EndTimeLimitation.EndTime.GetValueOrDefault(new TimeSpan(1, 23, 59, 59));

                if (restriction.StartTimeLimitation.StartTime.HasValue || restriction.EndTimeLimitation.EndTime.HasValue)
                {
                    var validPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(scheduleDayDateOnly.Date.Add(startStart), scheduleDayDateOnly.Date.Add(endEnd), agentTimeZone);
                    workShiftsWithinPeriod = FilterOnDateTimePeriod(workShiftsWithinPeriod, validPeriod);
                }

                if (restriction.StartTimeLimitation.EndTime.HasValue)
                {
                    workShiftsWithinPeriod = FilterOnLatestStartTime(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.StartTimeLimitation.EndTime.Value), agentTimeZone));
                }

                if (restriction.EndTimeLimitation.StartTime.HasValue)
                {
                    workShiftsWithinPeriod = FilterOnEarliestEndTime(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.EndTimeLimitation.StartTime.Value), agentTimeZone));
                }
            }
            
            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnDateTimePeriod(IList<ShiftProjectionCache> shiftList, DateTimePeriod validPeriod)
        {
	        IList<ShiftProjectionCache> workShiftsWithinPeriod =
		        shiftList.Select(s => new {s, OuterPeriod = s.TheMainShift.LayerCollection.OuterPeriod()})
			        .Where(s => s.OuterPeriod.HasValue && validPeriod.Contains(s.OuterPeriod.Value))
			        .Select(s => s.s)
			        .ToArray();

            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnLatestStartTime(IList<ShiftProjectionCache> shiftList, DateTime latestStart)
        {
	        IList<ShiftProjectionCache> workShiftsWithinPeriod =
		        shiftList.Select(s => new {s, Period = s.MainShiftProjection().Period()})
			        .Where(s => s.Period.HasValue && s.Period.Value.StartDateTime <= latestStart)
			        .Select(s => s.s)
			        .ToArray();

            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnEarliestEndTime(IList<ShiftProjectionCache> shiftList, DateTime earliestEnd)
        {
            IList<ShiftProjectionCache> workShiftsWithinPeriod = shiftList.Select(s => new
		        {
			        s,
			        Period = s.MainShiftProjection().Period()
		        }).Where(s => s.Period.HasValue && s.Period.Value.EndDateTime >= earliestEnd)
		        .Select(s => s.s).ToArray();

            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnContractTime(MinMax<TimeSpan> validMinMax, IList<ShiftProjectionCache> shiftList)
        {
            if (shiftList.Count == 0)
                return shiftList;

	        IList<ShiftProjectionCache> workShiftsWithinMinMax =
		        shiftList.Where(s => validMinMax.Contains(s.WorkShiftProjectionContractTime())).ToArray();

            return workShiftsWithinMinMax;

        }

        public IList<ShiftProjectionCache> FilterOnRestrictionMinMaxWorkTime(IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction)
        {
            if (shiftList.Count == 0)
                return shiftList;
            if (restriction.WorkTimeLimitation.EndTime.HasValue || restriction.WorkTimeLimitation.StartTime.HasValue)
			{
				var workShiftsWithinMinMax =
					shiftList.Where(
							s =>
								restriction.WorkTimeLimitation.IsCorrespondingToWorkTimeLimitation(s.WorkShiftProjectionContractTime()))
						.ToArray();

				return workShiftsWithinMinMax;
			}

	        return shiftList;
        }

        public IList<ShiftProjectionCache> FilterOnShiftCategory(IShiftCategory category, IList<ShiftProjectionCache> shiftList)
        {
            if (shiftList.Count == 0)
                return shiftList;
            if (category == null)
                return shiftList;
            IEnumerable<ShiftProjectionCache> filtered =
                shiftList.Where(shift => shift.TheWorkShift.ShiftCategory.Equals(category));
            var ret = filtered.ToArray();
            return ret;
        }

        public IList<ShiftProjectionCache> FilterOnNotAllowedShiftCategories(HashSet<IShiftCategory> categories, IList<ShiftProjectionCache> shiftList)
        {
            if (categories.Count == 0)
                return shiftList;
            if (shiftList.Count == 0)
                return shiftList;
            var ret = shiftList.Where(shiftProjectionCache => !categories.Contains(shiftProjectionCache.TheWorkShift.ShiftCategory)).ToArray();

            return ret;
        }

        public IList<ShiftProjectionCache> FilterOnBusinessRules(IScheduleRange current, IList<ShiftProjectionCache> shiftList, DateOnly dateToCheck, bool scheduleOnDayOffs)
        {
            if (shiftList.Count == 0)
                return shiftList;
            DateTimePeriod? rulePeriod = _rules.PossiblePeriod(current, dateToCheck, scheduleOnDayOffs);
            if (!rulePeriod.HasValue)
            {
                return new List<ShiftProjectionCache>();
            }

            return FilterOnDateTimePeriod(shiftList, rulePeriod.Value);
        }

        public IList<ShiftProjectionCache> Filter(IScheduleDictionary schedules, MinMax<TimeSpan> validMinMax, IList<ShiftProjectionCache> shiftList, DateOnly dateToSchedule, IScheduleRange current, bool scheduleOnDayOffs)
        {
            shiftList = FilterOnContractTime(validMinMax, shiftList);

            shiftList = FilterOnBusinessRules(current, shiftList, dateToSchedule, scheduleOnDayOffs);

	        shiftList = _notOverWritableActivitiesShiftFilter.Filter(schedules, dateToSchedule, current.Person, shiftList);

			var dayPart = current.ScheduledDay(dateToSchedule);
	        shiftList = _personalShiftAndMeetingFilter.Filter(shiftList, dayPart);

	        return shiftList;
        }
    }
}
