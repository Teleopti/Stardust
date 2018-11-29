using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class TeamBlockScheduleSelected
	{
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
		private readonly ValidatedTeamBlockInfoExtractor _validatedTeamBlockExtractor;
		private readonly TeamMatrixChecker _teamMatrixChecker;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly BlockPreferencesMapper _blockPreferencesMapper;

		public TeamBlockScheduleSelected(TeamBlockScheduler teamBlockScheduler,
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation,
			IWorkShiftMinMaxCalculator workShiftMinMaxCalculator,
			ValidatedTeamBlockInfoExtractor validatedTeamBlockExtractor,
			TeamMatrixChecker teamMatrixChecker,
			IWorkShiftSelector workShiftSelector,
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			BlockPreferencesMapper blockPreferencesMapper)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_workShiftMinMaxCalculator = workShiftMinMaxCalculator;
			_validatedTeamBlockExtractor = validatedTeamBlockExtractor;
			_teamMatrixChecker = teamMatrixChecker;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_blockPreferencesMapper = blockPreferencesMapper;
		}

		public void ScheduleSelected(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
			IEnumerable<IPerson> selectedPersons,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			SchedulingOptions schedulingOption,
			ITeamInfoFactory teamInfoFactory,
			IBlockPreferenceProvider blockPreferenceProvider)
		{
			var dateOnlySkipList = new List<DateOnly>();

			var allTeamInfoListOnStartDate = getAllTeamInfoList(schedulingResultStateHolder, allPersonMatrixList, selectedPeriod, selectedPersons, teamInfoFactory);
			var checkedTeams = _teamMatrixChecker.CheckTeamList(allTeamInfoListOnStartDate, selectedPeriod);
			foreach (var datePointer in selectedPeriod.DayCollection())
			{
				if (schedulingCallback.IsCancelled) break;

				if (dateOnlySkipList.Contains(datePointer))
					continue;

				runSchedulingForAllTeamInfoOnStartDate(schedulingCallback, allPersonMatrixList, selectedPersons, selectedPeriod,
					schedulePartModifyAndRollbackService,
					checkedTeams, datePointer, dateOnlySkipList,
					resourceCalculateDelayer, schedulingResultStateHolder, schedulingOption,blockPreferenceProvider);
			}
		}

		private void runSchedulingForAllTeamInfoOnStartDate(ISchedulingCallback schedulingCallback, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, IEnumerable<IPerson> selectedPersons, DateOnlyPeriod selectedPeriod,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			IEnumerable<ITeamInfo> allTeamInfoListOnStartDate, DateOnly datePointer, List<DateOnly> dateOnlySkipList,
			IResourceCalculateDelayer resourceCalculateDelayer, ISchedulingResultStateHolder schedulingResultStateHolder, SchedulingOptions schedulingOption, IBlockPreferenceProvider blockPreferenceProvider)
		{
			var resCalcData = new ResourceCalculationData(schedulingResultStateHolder, schedulingOption.ConsiderShortBreaks, false);
			foreach (var teamInfo in allTeamInfoListOnStartDate.GetRandom(allTeamInfoListOnStartDate.Count(), true))
			{
				var blockPreferences = blockPreferenceProvider.ForAgents(teamInfo.GroupMembers, selectedPeriod.StartDate).ToArray();
				_blockPreferencesMapper.UpdateSchedulingOptionsFromExtraPreferences(schedulingOption, blockPreferences);

				var teamBlockInfo = _validatedTeamBlockExtractor.GetTeamBlockInfo(teamInfo, datePointer, allPersonMatrixList, schedulingOption, selectedPeriod);
				if (teamBlockInfo == null) continue;

				schedulePartModifyAndRollbackService.ClearModificationCollection();
				if (_teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), schedulingCallback, _workShiftSelector, teamBlockInfo, datePointer, schedulingOption,
					schedulePartModifyAndRollbackService,
					resourceCalculateDelayer, schedulingResultStateHolder.SkillDays, schedulingResultStateHolder.Schedules, resCalcData, new ShiftNudgeDirective(), NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
				{
					verifyScheduledTeamBlock(schedulingCallback, selectedPersons, schedulePartModifyAndRollbackService, datePointer,
						dateOnlySkipList, teamBlockInfo, schedulingOption);
				}
				else
				{
					schedulingCallback.Scheduled(new SchedulingCallbackInfo(null, false));
				}
				if (schedulingCallback.IsCancelled) break;
			}
		}

		private void verifyScheduledTeamBlock(ISchedulingCallback schedulingCallback, IEnumerable<IPerson> selectedPersons,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
			DateOnly datePointer, List<DateOnly> dateOnlySkipList, ITeamBlockInfo teamBlockInfo, 
			SchedulingOptions schedulingOption)
		{
			var dayCollection = teamBlockInfo.BlockInfo.BlockPeriod.DayCollection();
			foreach (var matrix in teamBlockInfo.TeamInfo.MatrixesForGroupAndDate(datePointer))
			{
				if (schedulingCallback.IsCancelled) break;

				if (!selectedPersons.Contains(matrix.Person)) continue;
				_workShiftMinMaxCalculator.ResetCache();
				if (!_workShiftMinMaxCalculator.IsPeriodInLegalState(matrix, schedulingOption))
				{
					_safeRollbackAndResourceCalculation.Execute(schedulePartModifyAndRollbackService, schedulingOption);
					dateOnlySkipList.AddRange(dayCollection);
					break;
				}
			}
		}

		private HashSet<ITeamInfo> getAllTeamInfoList(ISchedulingResultStateHolder schedulingResultStateHolder, IEnumerable<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod, IEnumerable<IPerson> selectedPersons, ITeamInfoFactory teamInfoFactory)
		{
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				var teamInfo = teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.LoadedAgents, selectedPerson, selectedPeriod, allPersonMatrixList);
				if (teamInfo != null)
					allTeamInfoListOnStartDate.Add(teamInfo);
			}

			foreach (var teamInfo in allTeamInfoListOnStartDate)
			{
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					if (!selectedPersons.Contains(groupMember))
						teamInfo.LockMember(selectedPeriod, groupMember);
				}
			}

			return allTeamInfoListOnStartDate;
		}
	}
}