﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
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
	    private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

	    public ShiftProjectionCacheFilter(ILongestPeriodForAssignmentCalculator rules, IPersonalShiftAndMeetingFilter personalShiftAndMeetingFilter, INotOverWritableActivitiesShiftFilter notOverWritableActivitiesShiftFilter, ICurrentTeleoptiPrincipal currentIdentity, Func<ISchedulerStateHolder> schedulerStateHolder)
        {
        	_rules = rules;
	        _personalShiftAndMeetingFilter = personalShiftAndMeetingFilter;
	        _notOverWritableActivitiesShiftFilter = notOverWritableActivitiesShiftFilter;
	        _currentIdentity = currentIdentity;
	        _schedulerStateHolder = schedulerStateHolder;
        }

    	public IList<IShiftProjectionCache> FilterOnRestrictionAndNotAllowedShiftCategories(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, 
            IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IList<IShiftCategory> notAllowedCategories, IWorkShiftFinderResult finderResult)
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

        public IList<IShiftProjectionCache> FilterOnMainShiftOptimizeActivitiesSpecification(IList<IShiftProjectionCache> shiftList, ISpecification<IEditableShift> mainShiftActivitiesOptimizeSpecification)
        {

            IList<IShiftProjectionCache> ret = new List<IShiftProjectionCache>();
		    if (shiftList != null)
		        foreach (var shiftProjectionCache in shiftList)
		        {
                    if (mainShiftActivitiesOptimizeSpecification != null && mainShiftActivitiesOptimizeSpecification.IsSatisfiedBy(shiftProjectionCache.TheMainShift))
		                ret.Add(shiftProjectionCache);
		        }

		    return ret;
        }


        public static IList<IShiftProjectionCache> FilterOnActivityRestrictions(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction , IWorkShiftFinderResult finderResult)
        {
            IList<IActivityRestriction> activityRestrictions = restriction.ActivityRestrictionCollection;
            if (activityRestrictions.Count == 0)
                return shiftList;

            IList<IShiftProjectionCache> workShiftsWithActivity = new List<IShiftProjectionCache>();

            foreach (var projectionCache in shiftList)
            {
                if (restriction.VisualLayerCollectionSatisfiesActivityRestriction(scheduleDayDateOnly, agentTimeZone,
                                                                                  projectionCache.MainShiftProjection.OfType<IActivityRestrictableVisualLayer>()))
                {
                    workShiftsWithActivity.Add(projectionCache); 
                }
            }

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

        public IList<IShiftProjectionCache> FilterOnRestrictionTimeLimits(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList,
                                                                   IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            IList<IShiftProjectionCache> workShiftsWithinPeriod = shiftList;
  
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

        public IList<IShiftProjectionCache> FilterOnDateTimePeriod(IList<IShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult)
        {
            var cntBefore = shiftList.Count;
            IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
            foreach (IShiftProjectionCache proj in shiftList)
            {
                var mainShiftPeriod = proj.TheMainShift.LayerCollection.OuterPeriod();
                if (mainShiftPeriod.HasValue)
                {
                    if (validPeriod.Contains(mainShiftPeriod.Value))
                    {
                        workShiftsWithinPeriod.Add(proj);
                    }
                }
            }
	        var regional = _currentIdentity.Current().Regional;
	        var currentTimeZone = _schedulerStateHolder().TimeZoneInfo;
	        finderResult.AddFilterResults(
                new WorkShiftFilterResult(
                    string.Format(regional.Culture,
                                  UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
                                  validPeriod.StartDateTimeLocal(currentTimeZone), validPeriod.EndDateTimeLocal(currentTimeZone)), cntBefore,
                    workShiftsWithinPeriod.Count));

            return workShiftsWithinPeriod;
        }

        public IList<IShiftProjectionCache> FilterOnLatestStartTime(IList<IShiftProjectionCache> shiftList, DateTime latestStart, IWorkShiftFinderResult finderResult)
        {
            int cntBefore = shiftList.Count;
            IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
            foreach (IShiftProjectionCache proj in shiftList)
            {
                if (!proj.MainShiftProjection.Period().HasValue) continue;
                DateTimePeriod virtualPeriod = proj.MainShiftProjection.Period().Value;
                if (virtualPeriod.StartDateTime <= latestStart)
                {
                    workShiftsWithinPeriod.Add(proj);
                }
            }
			finderResult.AddFilterResults(
                new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnMinEndTimeOnRestriction, " "),
                                          cntBefore, workShiftsWithinPeriod.Count));

            return workShiftsWithinPeriod;
        }

        public IList<IShiftProjectionCache> FilterOnEarliestEndTime(IList<IShiftProjectionCache> shiftList, DateTime earliestEnd, IWorkShiftFinderResult finderResult)
        {
            int cntBefore = shiftList.Count;
            IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
            for (int workShiftCounter = 0; workShiftCounter < shiftList.Count; workShiftCounter++)
            {
                IShiftProjectionCache proj = shiftList[workShiftCounter];

                if (!proj.MainShiftProjection.Period().HasValue) continue;
                DateTimePeriod virtualPeriod = proj.MainShiftProjection.Period().Value;
                if (virtualPeriod.EndDateTime >= earliestEnd)
                {
                    workShiftsWithinPeriod.Add(proj);
                }
            }
			finderResult.AddFilterResults(
                new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOnMinEndTimeOnRestriction, " "),
                                          cntBefore, workShiftsWithinPeriod.Count));

            return workShiftsWithinPeriod;
        }

        public IList<IShiftProjectionCache> FilterOnContractTime(MinMax<TimeSpan> validMinMax, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            int cntBefore = shiftList.Count;
            IList<IShiftProjectionCache> workShiftsWithinMinMax = new List<IShiftProjectionCache>();

            foreach (IShiftProjectionCache proj in shiftList)
            {
                if (validMinMax.Contains(proj.WorkShiftProjectionContractTime))
                    workShiftsWithinMinMax.Add(proj);
            }
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(string.Format(_currentIdentity.Current().Regional.Culture, UserTexts.Resources.FilterOnContractTimeLimitationsWithParams, validMinMax.Minimum, validMinMax.Maximum),
                                          cntBefore, workShiftsWithinMinMax.Count));

            return workShiftsWithinMinMax;

        }

        public IList<IShiftProjectionCache> FilterOnRestrictionMinMaxWorkTime(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            IList<IShiftProjectionCache> workShiftsWithinMinMax = new List<IShiftProjectionCache>();
            if (restriction.WorkTimeLimitation.EndTime.HasValue || restriction.WorkTimeLimitation.StartTime.HasValue)
            {
                foreach (var proj in shiftList)
                {
                    TimeSpan contractTime = proj.WorkShiftProjectionContractTime;
                    if (restriction.WorkTimeLimitation.IsCorrespondingToWorkTimeLimitation(contractTime))
                        workShiftsWithinMinMax.Add(proj);
                }
				finderResult.AddFilterResults(
				new WorkShiftFilterResult(string.Format(_currentIdentity.Current().Regional.Culture, UserTexts.Resources.FilterOnWorkTimeLimitationsWithParams, restriction.WorkTimeLimitation.StartTimeString, restriction.WorkTimeLimitation.EndTimeString),
                                          shiftList.Count, workShiftsWithinMinMax.Count));
            }
            else
            {
                return shiftList;
            }
            return workShiftsWithinMinMax;
        }

        public IList<IShiftProjectionCache> FilterOnShiftCategory(IShiftCategory category, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            if (category == null)
                return shiftList;
            int before = shiftList.Count;
            IEnumerable<IShiftProjectionCache> filtered =
                shiftList.Where(shift => shift.TheWorkShift.ShiftCategory.Equals(category));
            var ret = filtered.ToList();
			finderResult.AddFilterResults(new WorkShiftFilterResult(string.Concat(UserTexts.Resources.FilterOn, " ") + category.Description, before, ret.Count));
            return ret;
        }

        public IList<IShiftProjectionCache> FilterOnNotAllowedShiftCategories(IList<IShiftCategory> categories, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
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

        public IList<IShiftProjectionCache> FilterOnBusinessRules(IScheduleRange current, IList<IShiftProjectionCache> shiftList, DateOnly dateToCheck, IWorkShiftFinderResult finderResult)
        {
            if (shiftList.Count == 0)
                return shiftList;
            DateTimePeriod? rulePeriod = _rules.PossiblePeriod(current, dateToCheck);
            if (!rulePeriod.HasValue)
            {
				finderResult.AddFilterResults(
                    new WorkShiftFilterResult(UserTexts.Resources.CannotFindAValidPeriodAccordingToTheBusinessRules,
                                              shiftList.Count, 0));
                return new List<IShiftProjectionCache>();
            }

            return FilterOnDateTimePeriod(shiftList, rulePeriod.Value, finderResult);
        }

        public IList<IShiftProjectionCache> FilterOnStartAndEndTime(DateTimePeriod startAndEndTime, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
            IList<IShiftProjectionCache> ret = new List<IShiftProjectionCache>();
            int cnt = shiftList.Count;
            foreach (IShiftProjectionCache shiftProjectionCache in shiftList)
            {
                if (!shiftProjectionCache.MainShiftProjection.Period().HasValue) continue;
                if (shiftProjectionCache.MainShiftProjection.Period().Value == startAndEndTime)
                    ret.Add(shiftProjectionCache);
            }

            finderResult.AddFilterResults(new WorkShiftFilterResult(UserTexts.Resources.AfterCheckingAgainstKeepStartAndEndTime, cnt, ret.Count));
            return ret;
        }

        

        public IList<IShiftProjectionCache> Filter(MinMax<TimeSpan> validMinMax, IList<IShiftProjectionCache> shiftList, DateOnly dateToSchedule, IScheduleRange current, IWorkShiftFinderResult finderResult)
        {
            shiftList = FilterOnContractTime(validMinMax, shiftList, finderResult);

            shiftList = FilterOnBusinessRules(current, shiftList, dateToSchedule, finderResult);

	        shiftList = _notOverWritableActivitiesShiftFilter.Filter(dateToSchedule, current.Person, shiftList,
		        finderResult);

			var dayPart = current.ScheduledDay(dateToSchedule);
	        shiftList = _personalShiftAndMeetingFilter.Filter(shiftList, dayPart, finderResult);

	        return shiftList;
        }

        

		public IList<IShiftProjectionCache> FilterOnBusinessRules(IEnumerable<IPerson> groupOfPersons, IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
        {
			InParameter.NotNull("groupOfPersons", groupOfPersons);
			InParameter.NotNull("scheduleDictionary", scheduleDictionary);
			foreach (var person in groupOfPersons)
            {
                var range = scheduleDictionary[person];
                shiftList = FilterOnBusinessRules(range, shiftList, dateOnly, finderResult);
            }
            return shiftList;
        }
    }
}
