using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockDayOffOptimizer
	{
		private readonly IAllTeamMembersInSelectionSpecification _allTeamMembersInSelectionSpecification;
		private readonly TeamBlockDaysOffSameDaysOffLockSyncronizer _teamBlockDaysOffSameDaysOffLockSyncronizer;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IOptimizerHelperHelper _optimizerHelper;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly DayOffOptimizerStandard _dayOffOptimizeStandard;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_TeamSameDayOff_44265)]
		private readonly IDayOffOptimizerUseTeamSameDaysOff _dayOffOptimizerUseTeamSameDaysOff;

		public TeamBlockDayOffOptimizer(
			IAllTeamMembersInSelectionSpecification allTeamMembersInSelectionSpecification,
			TeamBlockDaysOffSameDaysOffLockSyncronizer teamBlockDaysOffSameDaysOffLockSyncronizer,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IOptimizerHelperHelper optimizerHelper,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			DayOffOptimizerStandard dayOffOptimizeStandard,
			IDayOffOptimizerUseTeamSameDaysOff dayOffOptimizerUseTeamSameDaysOff)
		{
			_allTeamMembersInSelectionSpecification = allTeamMembersInSelectionSpecification;
			_teamBlockDaysOffSameDaysOffLockSyncronizer = teamBlockDaysOffSameDaysOffLockSyncronizer;
			_schedulerStateHolder = schedulerStateHolder;
			_optimizerHelper = optimizerHelper;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_dayOffOptimizeStandard = dayOffOptimizeStandard;
			_dayOffOptimizerUseTeamSameDaysOff = dayOffOptimizerUseTeamSameDaysOff;
		}

		[TestLog]
		[RemoveMeWithToggle("Remove if about team + sameDO below", Toggles.ResourcePlanner_TeamSameDayOff_44265)]
		public virtual void OptimizeDaysOff(
			IList<IScheduleMatrixPro> allPersonMatrixList,
			DateOnlyPeriod selectedPeriod,
			IList<IPerson> selectedPersons,
			IOptimizationPreferences optimizationPreferences,
			SchedulingOptions schedulingOptions,
			IResourceCalculateDelayer resourceCalculateDelayer,
			IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider,
			ITeamInfoFactory teamInfoFactory,
			ISchedulingProgress schedulingProgress)
		{
			var schedulerStateHolder = _schedulerStateHolder();
			var tagSetter = new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling);
			var rollbackService = new SchedulePartModifyAndRollbackService(schedulerStateHolder.SchedulingResultState,
				_scheduleDayChangeCallback,
				tagSetter);
			schedulingOptions.DayOffTemplate = schedulerStateHolder.CommonStateHolder.DefaultDayOffTemplate;
			
			var skillsDataExtractor = _optimizerHelper.CreateTeamBlockAllSkillsDataExtractor(optimizationPreferences.Advanced, selectedPeriod, schedulerStateHolder.SchedulingResultState, allPersonMatrixList);
			var periodValueCalculatorForAllSkills = _optimizerHelper.CreatePeriodValueCalculator(optimizationPreferences.Advanced, skillsDataExtractor);

			// create a list of all teamInfos
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = teamInfoFactory.CreateTeamInfo(schedulerStateHolder.SchedulingResultState.PersonsInOrganization, selectedPerson, selectedPeriod, allPersonMatrixList);
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
				IEnumerable<ITeamInfo> teamInfosToRemove;
				if(optimizationPreferences.Extra.UseTeams && optimizationPreferences.Extra.UseTeamSameDaysOff)
				{
					teamInfosToRemove = _dayOffOptimizerUseTeamSameDaysOff.Execute(periodValueCalculatorForAllSkills, optimizationPreferences, rollbackService,
					                                            remainingInfoList, schedulingOptions, null,
					                                            resourceCalculateDelayer, schedulerStateHolder.SchedulingResultState, ()=>
					                                            {
						                                            cancelMe = true;
					                                            },
																dayOffOptimizationPreferenceProvider, schedulingProgress);
				}
				else
				{
					teamInfosToRemove = _dayOffOptimizeStandard.Execute(periodValueCalculatorForAllSkills, optimizationPreferences, rollbackService,
					                                                           remainingInfoList, schedulingOptions,
					                                                           selectedPersons,
																			   resourceCalculateDelayer, schedulerStateHolder.SchedulingResultState, () =>
																			   {
																				   cancelMe = true;
																			   },
																			   dayOffOptimizationPreferenceProvider, schedulingProgress);
				}

				if (cancelMe)
					break;

				foreach (var teamInfo in teamInfosToRemove)
				{
					remainingInfoList.Remove(teamInfo);
				}
			}
		}
	}
}