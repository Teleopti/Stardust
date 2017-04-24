using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ShiftProjectionCacheFilter : IShiftProjectionCacheFilter
    {
	    private readonly ILongestPeriodForAssignmentCalculator _rules;
	    private readonly IPersonalShiftAndMeetingFilter _personalShiftAndMeetingFilter;
	    private readonly INotOverWritableActivitiesShiftFilter _notOverWritableActivitiesShiftFilter;
	    private readonly ICurrentTeleoptiPrincipal _currentIdentity;
	    private readonly ITimeZoneGuard _timeZoneGuard;

	    public ShiftProjectionCacheFilter(ILongestPeriodForAssignmentCalculator rules, IPersonalShiftAndMeetingFilter personalShiftAndMeetingFilter, INotOverWritableActivitiesShiftFilter notOverWritableActivitiesShiftFilter, ICurrentTeleoptiPrincipal currentIdentity, ITimeZoneGuard timeZoneGuard)
        {
        	_rules = rules;
	        _personalShiftAndMeetingFilter = personalShiftAndMeetingFilter;
	        _notOverWritableActivitiesShiftFilter = notOverWritableActivitiesShiftFilter;
	        _currentIdentity = currentIdentity;
		    _timeZoneGuard = timeZoneGuard;
        }

    	public IList<ShiftProjectionCache> FilterOnRestrictionAndNotAllowedShiftCategories(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, 
            IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IList<IShiftCategory> notAllowedCategories, IWorkShiftFinderResult finderResult)
        {
            if (restriction == null)
            {
				finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingRestrictions, 0,
                                                                         0));
                shiftList.Clear();
                return shiftList;
            }

             shiftList = FilterOnShiftCategory(restriction.ShiftCategory, shiftList, finderResult);

             shiftList = FilterOnNotAllowedShiftCategories(notAllowedCategories, shiftList, finderResult);

             shiftList = FilterOnRestrictionTimeLimits(scheduleDayDateOnly, agentTimeZone, shiftList, restriction, finderResult);

             shiftList = FilterOnActivityRestrictions(scheduleDayDateOnly, agentTimeZone, shiftList, restriction, finderResult);

             return FilterOnRestrictionMinMaxWorkTime(shiftList, restriction, finderResult);
           }

        public IList<ShiftProjectionCache> FilterOnMainShiftOptimizeActivitiesSpecification(IList<ShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification)
        {
			if (shiftList == null || mainShiftActivitiesOptimizeSpecification == null) return new List<ShiftProjectionCache>();

	        IList<ShiftProjectionCache> ret =
		        shiftList.Where(s => mainShiftActivitiesOptimizeSpecification.IsSatisfiedBy(s.TheMainShift)).ToList();

		    return ret;
        }
		
        public static IList<ShiftProjectionCache> FilterOnActivityRestrictions(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction , IWorkShiftFinderResult finderResult)
        {
            IList<IActivityRestriction> activityRestrictions = restriction.ActivityRestrictionCollection;
            if (activityRestrictions.Count == 0)
                return shiftList;

	        IList<ShiftProjectionCache> workShiftsWithActivity =
		        shiftList.Where(
			        s => restriction.VisualLayerCollectionSatisfiesActivityRestriction(scheduleDayDateOnly, agentTimeZone,
				        s.MainShiftProjection.OfType<IActivityRestrictableVisualLayer>())).ToList();

			finderResult.AddFilterResults(
                new WorkShiftFilterResult(UserTexts.Resources.FilterOnPreferenceActivity, shiftList.Count,
                    workShiftsWithActivity.Count));

            return workShiftsWithActivity;
        }

        public bool CheckRestrictions(ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, IWorkShiftFinderResult finderResult)
        {
            if (effectiveRestriction == null)
            {
				finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingRestrictions, 0,
                                                                         0));
                return false;
            }

            if(effectiveRestriction.ShiftCategory != null && schedulingOptions.ShiftCategory != null)
            {
                if(effectiveRestriction.ShiftCategory.Id != schedulingOptions.ShiftCategory.Id)
                {
                    finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.ConflictingShiftCategories, 0,
                                                                         0));
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
				finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.NoRestrictionDefined, 0, 0));
                return false;
            }

            return true;
        }

        public IList<ShiftProjectionCache> FilterOnRestrictionTimeLimits(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<ShiftProjectionCache> shiftList,
                                                                   IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
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
                    workShiftsWithinPeriod = FilterOnDateTimePeriod(workShiftsWithinPeriod, validPeriod, finderResult);
                }

                if (restriction.StartTimeLimitation.EndTime.HasValue)
                {
                    workShiftsWithinPeriod = FilterOnLatestStartTime(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.StartTimeLimitation.EndTime.Value), agentTimeZone), finderResult);
                }

                if (restriction.EndTimeLimitation.StartTime.HasValue)
                {
                    workShiftsWithinPeriod = FilterOnEarliestEndTime(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.EndTimeLimitation.StartTime.Value), agentTimeZone), finderResult);
                }
            }
            
            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnDateTimePeriod(IList<ShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult)
        {
            var cntBefore = shiftList.Count;
	        IList<ShiftProjectionCache> workShiftsWithinPeriod =
		        shiftList.Select(s => new {s, OuterPeriod = s.TheMainShift.LayerCollection.OuterPeriod()})
			        .Where(s => s.OuterPeriod.HasValue && validPeriod.Contains(s.OuterPeriod.Value))
			        .Select(s => s.s)
			        .ToList();

			var regional = _currentIdentity.Current().Regional;
	        var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
	        finderResult.AddFilterResults(
                new WorkShiftFilterResult(
                    string.Format(regional.Culture,
                                  UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
                                  validPeriod.StartDateTimeLocal(currentTimeZone), validPeriod.EndDateTimeLocal(currentTimeZone)), cntBefore,
                    workShiftsWithinPeriod.Count));

            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnLatestStartTime(IList<ShiftProjectionCache> shiftList, DateTime latestStart, IWorkShiftFinderResult finderResult)
        {
            int cntBefore = shiftList.Count;
	        IList<ShiftProjectionCache> workShiftsWithinPeriod =
		        shiftList.Select(s => new {s, Period = s.MainShiftProjection.Period()})
			        .Where(s => s.Period.HasValue && s.Period.Value.StartDateTime <= latestStart)
			        .Select(s => s.s)
			        .ToList();

			finderResult.AddFilterResults(
                new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnMinEndTimeOnRestriction, " "),
                                          cntBefore, workShiftsWithinPeriod.Count));

            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnEarliestEndTime(IList<ShiftProjectionCache> shiftList, DateTime earliestEnd, IWorkShiftFinderResult finderResult)
        {
            int cntBefore = shiftList.Count;
            IList<ShiftProjectionCache> workShiftsWithinPeriod = shiftList.Select(s => new
		        {
			        s,
			        Period = s.MainShiftProjection.Period()
		        }).Where(s => s.Period.HasValue && s.Period.Value.EndDateTime >= earliestEnd)
		        .Select(s => s.s).ToList();
            
			finderResult.AddFilterResults(
                new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnMinEndTimeOnRestriction, " "),
                                          cntBefore, workShiftsWithinPeriod.Count));

            return workShiftsWithinPeriod;
        }

        public IList<ShiftProjectionCache> FilterOnContractTime(MinMax<TimeSpan> validMinMax, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            int cntBefore = shiftList.Count;
	        IList<ShiftProjectionCache> workShiftsWithinMinMax =
		        shiftList.Where(s => validMinMax.Contains(s.WorkShiftProjectionContractTime)).ToList();

			finderResult.AddFilterResults(
				new WorkShiftFilterResult(string.Format(_currentIdentity.Current().Regional.Culture, UserTexts.Resources.FilterOnContractTimeLimitationsWithParams, validMinMax.Minimum, validMinMax.Maximum),
                                          cntBefore, workShiftsWithinMinMax.Count));

            return workShiftsWithinMinMax;

        }

        public IList<ShiftProjectionCache> FilterOnRestrictionMinMaxWorkTime(IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            if (restriction.WorkTimeLimitation.EndTime.HasValue || restriction.WorkTimeLimitation.StartTime.HasValue)
            {
	            var workShiftsWithinMinMax =
		            shiftList.Where(
				            s =>
						            restriction.WorkTimeLimitation.IsCorrespondingToWorkTimeLimitation(s.WorkShiftProjectionContractTime))
			            .ToList();

				finderResult.AddFilterResults(
				new WorkShiftFilterResult(string.Format(_currentIdentity.Current().Regional.Culture, UserTexts.Resources.FilterOnWorkTimeLimitationsWithParams, restriction.WorkTimeLimitation.StartTimeString, restriction.WorkTimeLimitation.EndTimeString),
                                          shiftList.Count, workShiftsWithinMinMax.Count));

				return workShiftsWithinMinMax;
			}

	        return shiftList;
        }

        public IList<ShiftProjectionCache> FilterOnShiftCategory(IShiftCategory category, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            if (category == null)
                return shiftList;
            int before = shiftList.Count;
            IEnumerable<ShiftProjectionCache> filtered =
                shiftList.Where(shift => shift.TheWorkShift.ShiftCategory.Equals(category));
            var ret = filtered.ToList();
			finderResult.AddFilterResults(new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOn, " ") + category.Description, before, ret.Count));
            return ret;
        }

        public IList<ShiftProjectionCache> FilterOnNotAllowedShiftCategories(IList<IShiftCategory> categories, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
            if (categories.Count == 0)
                return shiftList;
            if (shiftList.Count == 0)
                return shiftList;
            int before = shiftList.Count;
            var ret = shiftList.Where(shiftProjectionCache => !categories.Contains(shiftProjectionCache.TheWorkShift.ShiftCategory)).ToList();

			finderResult.AddFilterResults(new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOn, " ") + categories.Count + UserTexts.Resources.NotAllowedShiftCategories, before, ret.Count));
            return ret;
        }

        public IList<ShiftProjectionCache> FilterOnBusinessRules(IScheduleRange current, IList<ShiftProjectionCache> shiftList, DateOnly dateToCheck, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            DateTimePeriod? rulePeriod = _rules.PossiblePeriod(current, dateToCheck);
            if (!rulePeriod.HasValue)
            {
				finderResult.AddFilterResults(
                    new WorkShiftFilterResult(UserTexts.Resources.CannotFindAValidPeriodAccordingToTheBusinessRules,
                                              shiftList.Count, 0));
                return new List<ShiftProjectionCache>();
            }

            return FilterOnDateTimePeriod(shiftList, rulePeriod.Value, finderResult);
        }

        public IList<ShiftProjectionCache> FilterOnStartAndEndTime(DateTimePeriod startAndEndTime, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
            int cnt = shiftList.Count;

	        IList<ShiftProjectionCache> ret =
		        shiftList.Select(s => new {s, Period = s.MainShiftProjection.Period()})
			        .Where(s => s.Period.HasValue && s.Period.Value == startAndEndTime)
			        .Select(s => s.s)
			        .ToList();

            finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.AfterCheckingAgainstKeepStartAndEndTime, cnt, ret.Count));
            return ret;
        }

        

        public IList<ShiftProjectionCache> Filter(IScheduleDictionary schedules, MinMax<TimeSpan> validMinMax, IList<ShiftProjectionCache> shiftList, DateOnly dateToSchedule, IScheduleRange current, IWorkShiftFinderResult finderResult)
        {
            shiftList = FilterOnContractTime(validMinMax, shiftList, finderResult);

            shiftList = FilterOnBusinessRules(current, shiftList, dateToSchedule, finderResult);

	        shiftList = _notOverWritableActivitiesShiftFilter.Filter(schedules, dateToSchedule, current.Person, shiftList, finderResult);

			var dayPart = current.ScheduledDay(dateToSchedule);
	        shiftList = _personalShiftAndMeetingFilter.Filter(shiftList, dayPart, finderResult);

	        return shiftList;
        }

        

		public IList<ShiftProjectionCache> FilterOnBusinessRules(IEnumerable<IPerson> groupOfPersons, IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IList<ShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
			InParameter.NotNull(nameof(groupOfPersons), groupOfPersons);
			InParameter.NotNull(nameof(scheduleDictionary), scheduleDictionary);
			foreach (var person in groupOfPersons)
            {
                var range = scheduleDictionary[person];
                shiftList = FilterOnBusinessRules(range, shiftList, dateOnly, finderResult);
            }
            return shiftList;
        }
    }
}
