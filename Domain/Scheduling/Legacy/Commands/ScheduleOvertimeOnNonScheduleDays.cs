using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class ScheduleOvertimeOnNonScheduleDays
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly TeamBlockScheduler _teamBlockScheduler;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly ITeamBlockInfoFactory _teamBlockInfoFactory;
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private readonly IWorkShiftSelector _workShiftSelector;
		private readonly PrimaryOrAllPersonSkillForNonOvertimeProvider _personSkillsForNonOvertimeProvider;
		
		public ScheduleOvertimeOnNonScheduleDays(Func<ISchedulerStateHolder> schedulerStateHolder,
			TeamBlockScheduler teamBlockScheduler,
			MatrixListFactory matrixListFactory,
			ITeamBlockInfoFactory teamBlockInfoFactory,
			ITeamInfoFactory teamInfoFactory,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IWeeksFromScheduleDaysExtractor weeksFromScheduleDaysExtractor,
			IWorkShiftSelector workShiftSelector,
			PrimaryOrAllPersonSkillForNonOvertimeProvider personSkillsForNonOvertimeProvider)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockScheduler = teamBlockScheduler;
			_matrixListFactory = matrixListFactory;
			_teamBlockInfoFactory = teamBlockInfoFactory;
			_teamInfoFactory = teamInfoFactory;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_weeksFromScheduleDaysExtractor = weeksFromScheduleDaysExtractor;
			_workShiftSelector = workShiftSelector;
			_personSkillsForNonOvertimeProvider = personSkillsForNonOvertimeProvider;
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
			var schedulingOptions = new SchedulingOptions
			{
				FixedShiftBag = overtimePreferences.ShiftBagToUse,
				OvertimeType = overtimePreferences.OvertimeType,
				ScheduleOnDayOffs = true,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("scheduling overtime", GroupPageType.SingleAgent),
				SkipNegativeShiftValues = true,
				AllowBreakContractTime = true,
				UseAverageShiftLengths = false,
				UsePreferences = false,
				UseRotations = false,
				UseAvailability = false
			};

			var shiftNudgeDirective = createShiftNudgeDirective(scheduleDay, overtimePreferences);
			var teamInfo = _teamInfoFactory.CreateTeamInfo(stateHolder.SchedulingResultState.LoadedAgents, agent, date, _matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, new[] { scheduleDay }));
			var teamBlockInfo = _teamBlockInfoFactory.CreateTeamBlockInfo(teamInfo, date, schedulingOptions.BlockFinder());
			if (teamBlockInfo == null)
				return;
			var resCalcData = new ResourceCalculationData(stateHolder.SchedulingResultState, schedulingOptions.ConsiderShortBreaks, false);
			_teamBlockScheduler.ScheduleTeamBlockDay(Enumerable.Empty<IPersonAssignment>(), new NoSchedulingCallback(), _workShiftSelector, teamBlockInfo, date, schedulingOptions, rollbackService, resourceCalculateDelayer,
				stateHolder.SchedulingResultState.SkillDays, stateHolder.SchedulingResultState.Schedules, resCalcData,
				shiftNudgeDirective, createRules(overtimePreferences), _personSkillsForNonOvertimeProvider.SkillAggregator(overtimePreferences));
		}

		private ShiftNudgeDirective createShiftNudgeDirective(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences)
		{
			var shiftNudgeDirective = new ShiftNudgeDirective();
			var person = scheduleDay.Person;
			var workTimeLimitation = new WorkTimeLimitation();

			if (!overtimePreferences.AllowBreakMaxWorkPerWeek)
			{
				var personWeek = _weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(new[] { scheduleDay }).First();
				var currentSchedules = _schedulerStateHolder().Schedules[person];
				double currentWorkTimeForWeek = 0;
				foreach (var schedule in currentSchedules.ScheduledDayCollection(personWeek.Week))
				{
					currentWorkTimeForWeek += schedule.ProjectionService().CreateProjection().WorkTime().TotalMinutes;
				}

				var maxContractWorkTimePerWeek = 0d;
				foreach (var period in person.PersonPeriods(personWeek.Week))
				{
					var workTime = period.PersonContract.Contract.WorkTimeDirective.MaxTimePerWeek.TotalMinutes;
					if (workTime > maxContractWorkTimePerWeek) maxContractWorkTimePerWeek = workTime;
				}

				var worktimeLeftOnWeek = maxContractWorkTimePerWeek - currentWorkTimeForWeek;
				if (worktimeLeftOnWeek > 0)
				{
					worktimeLeftOnWeek = Math.Min(WorkTimeLimitation.VerifyLimit.TotalMinutes, worktimeLeftOnWeek);
					workTimeLimitation = new WorkTimeLimitation(null, TimeSpan.FromMinutes(worktimeLeftOnWeek));
					var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), workTimeLimitation, null, null, null, new List<IActivityRestriction>());
					shiftNudgeDirective = new ShiftNudgeDirective(effectiveRestriction, ShiftNudgeDirective.NudgeDirection.Left);
				}
			}

			if (overtimePreferences.AvailableAgentsOnly)
			{
				var avail = scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().First();
				if (avail != null)
				{
					var startTimeLimitation = new StartTimeLimitation(avail.StartTime, null);
					var endTimeLimitation = new EndTimeLimitation(null, avail.EndTime);
					var effectiveRestriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
																		workTimeLimitation,
																		null, null, null, new List<IActivityRestriction>());
					return new ShiftNudgeDirective(effectiveRestriction, ShiftNudgeDirective.NudgeDirection.Left);
				}
			}

			return shiftNudgeDirective;
		}

		private static INewBusinessRuleCollection createRules(IOvertimePreferences overtimePreferences)
		{
			var rules = NewBusinessRuleCollection.Minimum();
			if (!overtimePreferences.AllowBreakMaxWorkPerWeek)
			{
				rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
			}
			if (!overtimePreferences.AllowBreakWeeklyRest)
			{
				var workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
				var dayOffMaxFlexCalculator = new DayOffMaxFlexCalculator(workTimeStartEndExtractor);
				rules.Add(new MinWeeklyRestRule(new WeeksFromScheduleDaysExtractor(),
					new PersonWeekViolatingWeeklyRestSpecification(new ExtractDayOffFromGivenWeek(),
						new VerifyWeeklyRestAroundDayOffSpecification(), new EnsureWeeklyRestRule(workTimeStartEndExtractor, dayOffMaxFlexCalculator))));
			}
			return rules;
		}

		private static bool jumpOutEarly(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences, IPerson agent, DateOnly date)
		{
			if (overtimePreferences.ShiftBagToUse == null)
				return true;
			if (!scheduleDay.PersonAssignment(true).ShiftLayers.IsEmpty())
				return true;
			if (!scheduleDay.PersonAbsenceCollection().IsEmpty())
				return true;
			
			var definitionSets = agent.Period(date)?.PersonContract?.Contract.MultiplicatorDefinitionSetCollection.Where(x => x.MultiplicatorType == MultiplicatorType.Overtime);

			if (definitionSets == null)
				return true;

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