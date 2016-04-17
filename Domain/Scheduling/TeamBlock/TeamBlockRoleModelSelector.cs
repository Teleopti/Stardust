using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ITeamBlockRoleModelSelector
	{
		IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly dateTime, IPerson person, ISchedulingOptions schedulingOptions, IEffectiveRestriction additionalEffectiveRestriction);
	}

	public class TeamBlockRoleModelSelector : ITeamBlockRoleModelSelector
	{
		private readonly ITeamBlockRestrictionAggregator _teamBlockRestrictionAggregator;
		private readonly IWorkShiftFilterService _workShiftFilterService;
		private readonly ISameOpenHoursInTeamBlockSpecification _sameOpenHoursInTeamBlockSpecification;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IActivityIntervalDataCreator _activityIntervalDataCreator;
		private readonly IMaxSeatInformationGeneratorBasedOnIntervals _maxSeatInformationGeneratorBasedOnIntervals;
		private readonly IMaxSeatSkillAggregator  _maxSeatSkillAggregator;
		private readonly IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			ISameOpenHoursInTeamBlockSpecification sameOpenHoursInTeamBlockSpecification,
			IWorkShiftSelector workShiftSelector,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IActivityIntervalDataCreator activityIntervalDataCreator,
			IMaxSeatInformationGeneratorBasedOnIntervals maxSeatInformationGeneratorBasedOnIntervals, 
			IMaxSeatSkillAggregator maxSeatSkillAggregator,
			IFirstShiftInTeamBlockFinder firstShiftInTeamBlockFinder)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlockSpecification = sameOpenHoursInTeamBlockSpecification;
			_workShiftSelector = workShiftSelector;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_activityIntervalDataCreator = activityIntervalDataCreator;
			_maxSeatInformationGeneratorBasedOnIntervals = maxSeatInformationGeneratorBasedOnIntervals;
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
			_firstShiftInTeamBlockFinder = firstShiftInTeamBlockFinder;
		}

		public IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, IPerson person, ISchedulingOptions schedulingOptions, IEffectiveRestriction additionalEffectiveRestriction)
		{
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(datePointer, person, teamBlockInfo,
				schedulingOptions);
			if (effectiveRestriction == null)
				return null;

			IShiftProjectionCache foundShiftProjectionCache = _firstShiftInTeamBlockFinder.FindFirst(teamBlockInfo, person,
				datePointer, _schedulingResultStateHolder);
			if (foundShiftProjectionCache != null &&
			    !schedulingOptions.NotAllowedShiftCategories.Contains(foundShiftProjectionCache.TheMainShift.ShiftCategory))
				return foundShiftProjectionCache;
			
			effectiveRestriction = effectiveRestriction.Combine(additionalEffectiveRestriction);
			var roleModel = filterAndSelect(teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, false);
			if(roleModel == null && effectiveRestriction.IsRestriction)
				roleModel = filterAndSelect(teamBlockInfo, datePointer, schedulingOptions, effectiveRestriction, true);

			return roleModel;
		}

		private IShiftProjectionCache filterAndSelect(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, IEffectiveRestriction effectiveRestriction, bool useShiftsForRestrictions)
		{
			var isSameOpenHoursInBlock = _sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(teamBlockInfo);
			var shifts = _workShiftFilterService.FilterForRoleModel(datePointer, teamBlockInfo, effectiveRestriction,
				schedulingOptions,
				new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupMembers.First(), datePointer),
				isSameOpenHoursInBlock, useShiftsForRestrictions);
			if (shifts.IsNullOrEmpty())
				return null;

			var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockInfo, datePointer,
				_schedulingResultStateHolder, true);
			var maxSeatInfo = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, datePointer, _schedulingResultStateHolder, TimeZoneGuard.Instance.TimeZone, true);
			var maxSeatSkills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(), new DateOnlyPeriod(datePointer, datePointer));
			bool hasMaxSeatSkill = maxSeatSkills.Any();
			var parameters = new PeriodValueCalculationParameters(schedulingOptions
				.WorkShiftLengthHintOption, schedulingOptions.UseMinimumPersons,
				schedulingOptions.UseMaximumPersons, schedulingOptions.UserOptionMaxSeatsFeature, hasMaxSeatSkill, maxSeatInfo);

			var roleModel = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
				parameters, TimeZoneGuard.Instance.TimeZone);

			return roleModel;
		}
	}
}