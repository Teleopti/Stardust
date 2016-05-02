using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleOvertimeOnNonScheduleDays
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ITeamBlockScheduler _teamBlockScheduler;
		private readonly IMatrixListFactory _matrixListFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;

		public ScheduleOvertimeOnNonScheduleDays(Func<ISchedulerStateHolder> schedulerStateHolder,
			ITeamBlockScheduler teamBlockScheduler,
			IMatrixListFactory matrixListFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamInfoFactory teamInfoFactory,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockScheduler = teamBlockScheduler;
			_matrixListFactory = matrixListFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamInfoFactory = teamInfoFactory;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
		}

		public void SchedulePersonOnDay(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var agent = scheduleDay.Person;
			if (jumpOutEarly(scheduleDay, overtimePreferences, agent, date))
				return;

			var stateHolder = _schedulerStateHolder();
			_groupPersonBuilderWrapper.SetSingleAgentTeam();
			var scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
			var rollbackService = new SchedulePartModifyAndRollbackService(stateHolder.SchedulingResultState, new DoNothingScheduleDayChangeCallBack(), scheduleTagSetter);
			using (OvertimeSchedulingScope.Set(scheduleDay, overtimePreferences))
			{
				var schedulingOptions = new SchedulingOptions
				{
					FixedShiftBag = overtimePreferences.ShiftBagToUse,
					OvertimeType = overtimePreferences.OvertimeType,
					ScheduleOnDayOffs = true
				};
				var matrixes = _matrixListFactory.CreateMatrixListForSelection(new[] { scheduleDay });
				var teamInfo = _teamInfoFactory.CreateTeamInfo(agent, date, matrixes);
				var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, date, schedulingOptions.BlockFinder(), true);
				var rules = NewBusinessRuleCollection.AllForScheduling(stateHolder.SchedulingResultState);
				if (!overtimePreferences.AllowBreakMaxWorkPerWeek)
				{
					rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
				}
				if (!overtimePreferences.AllowBreakWeeklyRest)
				{
					var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
					var dayOffMaxFlexCalculator = new DayOffMaxFlexCalculator(workTimeStartEndExtractor);
					var ensureWeeklyRestRule = new EnsureWeeklyRestRule(workTimeStartEndExtractor, dayOffMaxFlexCalculator);
					rules.Add(new MinWeeklyRestRule(new WeeksFromScheduleDaysExtractor(), new PersonWeekViolatingWeeklyRestSpecification(new ExtractDayOffFromGivenWeek(), new VerifyWeeklyRestAroundDayOffSpecification(), ensureWeeklyRestRule)));
				}
				_teamBlockScheduler.ScheduleTeamBlockDay(teamBlockInfo, date, schedulingOptions, rollbackService, resourceCalculateDelayer, stateHolder.SchedulingResultState, new ShiftNudgeDirective(), rules);
			}
		}

		private static bool jumpOutEarly(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences, IPerson agent, DateOnly date)
		{
			if (!scheduleDay.PersonAssignment(true).ShiftLayers.IsEmpty())
				return true;

			if (!scheduleDay.PersonAbsenceCollection().IsEmpty())
				return true;
			if (overtimePreferences.ShiftBagToUse == null)
				return true;

			var definitionSets = agent.Period(date)
				.PersonContract.Contract.MultiplicatorDefinitionSetCollection.Where(
					x => x.MultiplicatorType == MultiplicatorType.Overtime);
			if (!definitionSets.Contains(overtimePreferences.OvertimeType))
				return true;

			if (overtimePreferences.AvailableAgentsOnly)
			{
				if (!scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().
					Any(x => x.Period.Contains(scheduleDay.PersonAssignment(true).Period)))
				{
					return true;
				}
			}
			return false;
		}
	}
}