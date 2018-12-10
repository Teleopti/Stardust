using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockDayOffOptimizer
	{
		private readonly IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private readonly TeamBlockDaysOffSameDaysOffLockSyncronizer _teamBlockDaysOffSameDaysOffLockSyncronizer;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly DayOffOptimizerStandard _dayOffOptimizeStandard;
		private readonly IOptimizerHelperHelper _optimizerHelperHelper;

		public TeamBlockDayOffOptimizer(
			IAllTeamMembersInSelectionSpecification allTeamMembersInSelectionSpecification,
			TeamBlockDaysOffSameDaysOffLockSyncronizer teamBlockDaysOffSameDaysOffLockSyncronizer,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			DayOffOptimizerStandard dayOffOptimizeStandard,
			IOptimizerHelperHelper optimizerHelperHelper)
		{
			_allTeamMembersInSelectionSpecification = allTeamMembersInSelectionSpecification;
			_teamBlockDaysOffSameDaysOffLockSyncronizer = teamBlockDaysOffSameDaysOffLockSyncronizer;
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_dayOffOptimizeStandard = dayOffOptimizeStandard;
			_optimizerHelperHelper = optimizerHelperHelper;
		}

		[TestLog]
		public virtual void OptimizeDaysOff(
			IEnumerable<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			IBlockPreferenceProvider blockPreferenceProvider,
			ITeamInfoFactory teamInfoFactory,
			ISchedulingProgress schedulingProgress)
		{
			_optimizerHelperHelper.LockDaysForDayOffOptimization(allPersonMatrixList, optimizationPreferences, selectedPeriod);
			
			var schedulerStateHolder = _schedulerStateHolder();
			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
				_scheduleDayChangeCallback,
				tagSetter);
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			
			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = teamInfoFactory.CreateTeamInfo(schedulerStateHolder.SchedulingResultState.LoadedAgents, selectedPerson, selectedPeriod, allPersonMatrixList);
				if(optimizationPreferences.Extra.UseTeamBlockOption && optimizationPreferences.Extra.UseTeamSameDaysOff)
				{
					if (!_allTeamMembersInSelectionSpecification.IsSatifyBy(teamInfo, selectedPersons))
						continue;
				}
				if (teamInfo != null )
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					if(!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(selectedPeriod, groupMember);
				}
			}

			_teamBlockDaysOffSameDaysOffLockSyncronizer.SyncLocks(selectedPeriod, optimizationPreferences, allTeamInfoListOnStartDate);

			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate.Where(x => x.GroupMembers.Any()));

			var cancelMe = false;
			while (remainingInfoList.Count > 0)
			{
				var teamInfosToRemove = _dayOffOptimizeStandard.Execute(optimizationPreferences, rollbackService,
					remainingInfoList, schedulingOptions,
					selectedPersons,
					resourceCalculateDelayer, schedulerStateHolder.SchedulingResultState, () =>
					{
						cancelMe = true;
					},
					dayOffOptimizationPreferenceProvider, blockPreferenceProvider, schedulingProgress);

				if (cancelMe || teamInfosToRemove==null)
					break;

				foreach (var teamInfo in teamInfosToRemove)
				{
					remainingInfoList.Remove(teamInfo);
				}
			}
		}
	}
}