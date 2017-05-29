using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class Scheduling : IScheduling
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IScheduleDayChangeCallback> _scheduleDayChangeCallback;
		private readonly AdvanceDaysOffSchedulingService _advanceDaysOffSchedulingService;
		private readonly MatrixListFactory _matrixListFactory;
		private readonly IWeeklyRestSolverCommand _weeklyRestSolverCommand;
		private readonly IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IUserTimeZone _userTimeZone;
		private readonly TeamBlockScheduleSelected _teamBlockScheduleSelected;
		private readonly TeamInfoFactoryFactory _teamInfoFactoryFactory;
		private readonly RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;

		public Scheduling(Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IScheduleDayChangeCallback> scheduleDayChangeCallback,
			AdvanceDaysOffSchedulingService advanceDaysOffSchedulingService,
			MatrixListFactory matrixListFactory,
			IWeeklyRestSolverCommand weeklyRestSolverCommand,
			IGroupPersonBuilderWrapper groupPersonBuilderWrapper,
			IResourceCalculation resourceCalculation,
			IUserTimeZone userTimeZone,
			TeamBlockScheduleSelected teamBlockScheduleSelected,
			TeamInfoFactoryFactory teamInfoFactoryFactory,
			RuleSetBagsOfGroupOfPeopleCanHaveShortBreak ruleSetBagsOfGroupOfPeopleCanHaveShortBreak)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			_advanceDaysOffSchedulingService = advanceDaysOffSchedulingService;
			_matrixListFactory = matrixListFactory;
			_weeklyRestSolverCommand = weeklyRestSolverCommand;
			_groupPersonBuilderWrapper = groupPersonBuilderWrapper;
			_resourceCalculation = resourceCalculation;
			_userTimeZone = userTimeZone;
			_teamBlockScheduleSelected = teamBlockScheduleSelected;
			_teamInfoFactoryFactory = teamInfoFactoryFactory;
			_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak = ruleSetBagsOfGroupOfPeopleCanHaveShortBreak;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod, IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ResourceCalculateFrequency,
				_ruleSetBagsOfGroupOfPeopleCanHaveShortBreak.CanHaveShortBreak(selectedAgents, selectedPeriod), _schedulerStateHolder().SchedulingResultState, _userTimeZone);
			ISchedulePartModifyAndRollbackService rollbackService =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState,
					_scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

			var schedulePartModifyAndRollbackServiceForContractDaysOff =
				new SchedulePartModifyAndRollbackService(_schedulerStateHolder().SchedulingResultState, _scheduleDayChangeCallback(),
					new ScheduleTagSetter(schedulingOptions.TagToUseOnScheduling));

			var teamInfoFactory = _teamInfoFactoryFactory.Create(_schedulerStateHolder().AllPermittedPersons, _schedulerStateHolder().Schedules, schedulingOptions.GroupOnGroupPageForTeamBlockPer);

			//can we use fewer also with team? what happens if AgentA and AgentB have different scheduleperiods? (does it even matter?)
			var matrixes = schedulingOptions.UseTeam ? 
				_matrixListFactory.CreateMatrixListAllForLoadedPeriod(_schedulerStateHolder().Schedules, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization, selectedPeriod) : 
				_matrixListFactory.CreateMatrixListForSelection(_schedulerStateHolder().Schedules, selectedAgents, selectedPeriod);

			_advanceDaysOffSchedulingService.Execute(schedulingCallback, matrixes, selectedAgents,
				schedulePartModifyAndRollbackServiceForContractDaysOff, schedulingOptions,
				_groupPersonBuilderWrapper, selectedPeriod);

			_teamBlockScheduleSelected.ScheduleSelected(schedulingCallback, matrixes, selectedPeriod,
				selectedAgents, rollbackService, resourceCalculateDelayer,
				_schedulerStateHolder().SchedulingResultState, schedulingOptions, teamInfoFactory);

			//TODO: get rid of _backgroundWorker here...
			_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedAgents, rollbackService, resourceCalculateDelayer,
				selectedPeriod, matrixes, backgroundWorker, dayOffOptimizationPreferenceProvider);
		}
	}
}