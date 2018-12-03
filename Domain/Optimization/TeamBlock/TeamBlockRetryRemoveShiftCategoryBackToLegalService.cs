using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockRetryRemoveShiftCategoryBackToLegalService
	{
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly TeamBlockClearer _teamBlockClearer;
		private readonly ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private readonly ShiftCategoryLimitCounter _shiftCategoryLimitCounter;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly RemoveScheduleDayProsBasedOnShiftCategoryLimitation _removeScheduleDayProsBasedOnShiftCategoryLimitation;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IUserTimeZone _userTimeZone;

		public TeamBlockRetryRemoveShiftCategoryBackToLegalService(TeamBlockScheduler teamBlockScheduler, 
			ITeamInfoFactory teamInfoFactory, 
			ITeamBlockInfoFactory teamBlockInfoFactory,
			TeamBlockClearer teamBlockClearer, 
			ISafeRollbackAndResourceCalculation safeRollbackAndResourceCalculation, 
			ShiftCategoryLimitCounter shiftCategoryLimitCounter, 
			IWorkShiftSelector workShiftSelector, 
			IGroupPersonSkillAggregator groupPersonSkillAggregator,
			RemoveScheduleDayProsBasedOnShiftCategoryLimitation removeScheduleDayProsBasedOnShiftCategoryLimitation,
			IScheduleDayChangeCallback scheduleDayChangeCallback,
			IResourceCalculation resourceCalculation,
			IUserTimeZone userTimeZone)
		{
			_teamBlockScheduler = teamBlockScheduler;
			_teamInfoFactory = teamInfoFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamBlockClearer = teamBlockClearer;
			_safeRollbackAndResourceCalculation = safeRollbackAndResourceCalculation;
			_shiftCategoryLimitCounter = shiftCategoryLimitCounter;
			_workShiftSelector = workShiftSelector;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_removeScheduleDayProsBasedOnShiftCategoryLimitation = removeScheduleDayProsBasedOnShiftCategoryLimitation;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_resourceCalculation = resourceCalculation;
			_userTimeZone = userTimeZone;
		}

		[TestLog]
		public virtual void Execute(SchedulingOptions schedulingOptions, 
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IEnumerable<IScheduleMatrixPro> scheduleMatrixListPros,
			ISchedulingProgress backgroundWorker)
		{
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ConsiderShortBreaks, schedulingResultStateHolder, _userTimeZone);
			backgroundWorker.ReportProgress(0, new TeleoptiProgressChangeMessage(Resources.TryingToResolveShiftCategoryLimitationsDotDotDot));
			foreach (var matrix in scheduleMatrixListPros)
			{
				var shiftNudgeDirective = new ShiftNudgeDirective();

				foreach (var limitation in matrix.SchedulePeriod.ShiftCategoryLimitationCollection())
				{
					var rollbackService = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

					var unsuccessfulDays = new HashSet<DateOnly>();
					executePerShiftCategoryLimitation(schedulingOptions, matrix, schedulingResultStateHolder,
						rollbackService, resourceCalculateDelayer, scheduleMatrixListPros, shiftNudgeDirective, limitation, unsuccessfulDays);

					unsuccessfulDays.ForEach(x => matrix.UnlockPeriod(x.ToDateOnlyPeriod()));
					_removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, matrix, limitation, rollbackService);
				}
			}

			//maybe not necessary when we put schedules "correct" above
			var rollbackServiceTemp = new SchedulePartModifyAndRollbackService(schedulingResultStateHolder, _scheduleDayChangeCallback, new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));
			foreach (var scheduleMatrixPro in scheduleMatrixListPros)
			{
				foreach (var limitation in scheduleMatrixPro.SchedulePeriod.ShiftCategoryLimitationCollection())
				{
					_removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, scheduleMatrixPro, limitation, rollbackServiceTemp);
				}
			}
			//
		}

		private void executePerShiftCategoryLimitation(SchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro,
			ISchedulingResultStateHolder schedulingResultStateHolder, ISchedulePartModifyAndRollbackService rollbackService,
			IResourceCalculateDelayer resourceCalculateDelayer, IEnumerable<IScheduleMatrixPro> allScheduleMatrixPros,
			ShiftNudgeDirective shiftNudgeDirective, IShiftCategoryLimitation limitation, HashSet<DateOnly> lockedDays)
		{
			var removedScheduleDayPros = _removeScheduleDayProsBasedOnShiftCategoryLimitation.Execute(schedulingOptions, scheduleMatrixPro, limitation, rollbackService);

			foreach (var removedScheduleDayPro in removedScheduleDayPros)
			{
				var dateOnly = removedScheduleDayPro.Day;
				ITeamInfo teamInfo;

				if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.HourlyStaff)
				{
					var group = new Group(new List<IPerson> { removedScheduleDayPro.DaySchedulePart().Person }, string.Empty);
					var scheduleMatrixProsList = new List<IList<IScheduleMatrixPro>> { new List<IScheduleMatrixPro> { scheduleMatrixPro } };
					teamInfo = new TeamInfo(group, scheduleMatrixProsList);
				}
				else
				{
					teamInfo = _teamInfoFactory.CreateTeamInfo(schedulingResultStateHolder.LoadedAgents, scheduleMatrixPro.Person, dateOnly.ToDateOnlyPeriod(), allScheduleMatrixPros);
				}

				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, dateOnly, schedulingOptions.BlockFinder());

				if (teamBlockInfo == null)
					continue;

				schedulingOptions.NotAllowedShiftCategories.Clear();

				foreach (var matrixPro in teamBlockInfo.MatrixesForGroupAndBlock())
				{
					foreach (var shiftCategoryLimitation in matrixPro.SchedulePeriod.ShiftCategoryLimitationCollection())
					{
						if (_shiftCategoryLimitCounter.HaveMaxOfShiftCategory(shiftCategoryLimitation, teamInfo, dateOnly))
						{
							schedulingOptions.NotAllowedShiftCategories.Add(shiftCategoryLimitation.ShiftCategory);
						}
					}
				}
				var resCalcData = new ResourceCalculationData(schedulingResultStateHolder, schedulingOptions.ConsiderShortBreaks, false);

				if (_teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.SkillDays, schedulingResultStateHolder.Schedules, resCalcData, shiftNudgeDirective,
					NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
					continue;

				_teamBlockClearer.ClearTeamBlock(schedulingOptions, rollbackService, teamBlockInfo);
				if (_teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, dateOnly, schedulingOptions,
					rollbackService, resourceCalculateDelayer, schedulingResultStateHolder.SkillDays, schedulingResultStateHolder.Schedules, resCalcData, shiftNudgeDirective,
					NewBusinessRuleCollection.AllForScheduling(schedulingResultStateHolder), _groupPersonSkillAggregator))
					continue;

				_safeRollbackAndResourceCalculation.Execute(rollbackService, schedulingOptions);

				scheduleMatrixPro.LockDay(removedScheduleDayPro.Day);
				lockedDays.Add(removedScheduleDayPro.Day);

				executePerShiftCategoryLimitation(schedulingOptions, scheduleMatrixPro, schedulingResultStateHolder, rollbackService,
					resourceCalculateDelayer, allScheduleMatrixPros, shiftNudgeDirective, limitation, lockedDays);
			}
		}
	}
}