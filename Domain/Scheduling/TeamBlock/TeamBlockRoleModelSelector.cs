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
		IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly dateTime, IPerson person, ISchedulingOptions schedulingOptions, IEffectiveRestriction additionalEffectiveRestriction, bool isMaxSeatToggleEnabled);
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

		public TeamBlockRoleModelSelector(ITeamBlockRestrictionAggregator teamBlockRestrictionAggregator,
			IWorkShiftFilterService workShiftFilterService,
			ISameOpenHoursInTeamBlockSpecification sameOpenHoursInTeamBlockSpecification,
			IWorkShiftSelector workShiftSelector,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IActivityIntervalDataCreator activityIntervalDataCreator,
			IMaxSeatInformationGeneratorBasedOnIntervals maxSeatInformationGeneratorBasedOnIntervals, IMaxSeatSkillAggregator maxSeatSkillAggregator)
		{
			_teamBlockRestrictionAggregator = teamBlockRestrictionAggregator;
			_workShiftFilterService = workShiftFilterService;
			_sameOpenHoursInTeamBlockSpecification = sameOpenHoursInTeamBlockSpecification;
			_workShiftSelector = workShiftSelector;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_activityIntervalDataCreator = activityIntervalDataCreator;
			_maxSeatInformationGeneratorBasedOnIntervals = maxSeatInformationGeneratorBasedOnIntervals;
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
		}

		public IShiftProjectionCache Select(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, IPerson person, ISchedulingOptions schedulingOptions, IEffectiveRestriction additionalEffectiveRestriction, bool isMaxSeatToggleEnabled)
		{
			var effectiveRestriction = _teamBlockRestrictionAggregator.Aggregate(datePointer, person, teamBlockInfo,
				schedulingOptions);
			if (effectiveRestriction == null)
				return null;
			effectiveRestriction = effectiveRestriction.Combine(additionalEffectiveRestriction);
			var isSameOpenHoursInBlock = _sameOpenHoursInTeamBlockSpecification.IsSatisfiedBy(teamBlockInfo);
			var shifts = _workShiftFilterService.FilterForRoleModel(datePointer, teamBlockInfo, effectiveRestriction,
				schedulingOptions,
				new WorkShiftFinderResult(teamBlockInfo.TeamInfo.GroupMembers.First(), datePointer),
				isSameOpenHoursInBlock);
			if (shifts.IsNullOrEmpty())
				return null;

			var activityInternalData = _activityIntervalDataCreator.CreateFor(teamBlockInfo, datePointer,
				_schedulingResultStateHolder, true);
			var maxSeatFeatureOption = MaxSeatsFeatureOptions.DoNotConsiderMaxSeats;
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> maxSeatInfo = new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			maxSeatFeatureOption = maxSeatsFeature(teamBlockInfo, datePointer, schedulingOptions, maxSeatFeatureOption,
				ref maxSeatInfo, isMaxSeatToggleEnabled);
			var maxSeatSkills = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList() , new DateOnlyPeriod(datePointer, datePointer));
			bool hasMaxSeatSkill = maxSeatSkills.Any();

			var parameters = new PeriodValueCalculationParameters(schedulingOptions
				.WorkShiftLengthHintOption, schedulingOptions
					.UseMinimumPersons,
				schedulingOptions
					.UseMaximumPersons, maxSeatFeatureOption, hasMaxSeatSkill, maxSeatInfo, isMaxSeatToggleEnabled);

			var roleModel = _workShiftSelector.SelectShiftProjectionCache(shifts, activityInternalData,
				parameters, TimeZoneGuard.Instance.TimeZone);
			return roleModel;
		}

		private MaxSeatsFeatureOptions maxSeatsFeature(ITeamBlockInfo teamBlockInfo, DateOnly datePointer, ISchedulingOptions schedulingOptions, MaxSeatsFeatureOptions maxSeatFeatureOption, ref IDictionary<DateTime, IntervalLevelMaxSeatInfo> maxSeatInfo, bool isMaxSeatToggleEnabled)
		{
			if (isMaxSeatToggleEnabled )
			{
				maxSeatFeatureOption = schedulingOptions.UserOptionMaxSeatsFeature;
				maxSeatInfo = _maxSeatInformationGeneratorBasedOnIntervals.GetMaxSeatInfo(teamBlockInfo, datePointer,
					_schedulingResultStateHolder, TimeZoneGuard.Instance.TimeZone,true);
			}
			return maxSeatFeatureOption;
		}
	}
}