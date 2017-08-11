using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		private readonly INightRestWhiteSpotSolverServiceFactory _nightRestWhiteSpotSolverServiceFactory;

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
			INightRestWhiteSpotSolverServiceFactory nightRestWhiteSpotSolverServiceFactory)
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
			_nightRestWhiteSpotSolverServiceFactory = nightRestWhiteSpotSolverServiceFactory;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions, ISchedulingProgress backgroundWorker,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod)
		{
			var resourceCalculateDelayer = new ResourceCalculateDelayer(_resourceCalculation, schedulingOptions.ResourceCalculateFrequency,
				schedulingOptions.ConsiderShortBreaks, _schedulerStateHolder().SchedulingResultState, _userTimeZone);
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

			ScheduleSelected(schedulingCallback, schedulingOptions, selectedAgents, selectedPeriod, matrixes, rollbackService, resourceCalculateDelayer, teamInfoFactory);

			if (schedulingCallback.IsCancelled)
				return;

			if (schedulingOptions.IsClassic())
			{
				var nightRestWhiteSpotSolverService = _nightRestWhiteSpotSolverServiceFactory.Create(schedulingOptions.ConsiderShortBreaks);

				foreach (var scheduleMatrixPro in matrixes)
				{
					nightRestWhiteSpotSolverService.Resolve(scheduleMatrixPro, schedulingOptions, rollbackService);
				}
			}

			//TODO: get rid of _backgroundWorker here...
			_weeklyRestSolverCommand.Execute(schedulingOptions, null, selectedAgents, rollbackService, resourceCalculateDelayer,
				selectedPeriod, matrixes, backgroundWorker, null);
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_SchedulingFewerResourceCalculations_45429)]
		protected virtual void ScheduleSelected(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleMatrixPro> matrixes,
			ISchedulePartModifyAndRollbackService rollbackService, ResourceCalculateDelayer resourceCalculateDelayer,
			ITeamInfoFactory teamInfoFactory)
		{
			_teamBlockScheduleSelected.ScheduleSelected(schedulingCallback, matrixes, selectedPeriod,
				selectedAgents, rollbackService, resourceCalculateDelayer,
				_schedulerStateHolder().SchedulingResultState, schedulingOptions, teamInfoFactory);
		}
	}

	[RemoveMeWithToggle("Move this impl into Scheduling", Toggles.ResourcePlanner_SchedulingFewerResourceCalculations_45429)]
	public class SchedulingFewerResourceCalculations : Scheduling
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly TeamBlockScheduleSelected _teamBlockScheduleSelected;

		public SchedulingFewerResourceCalculations(Func<ISchedulerStateHolder> schedulerStateHolder, Func<IScheduleDayChangeCallback> scheduleDayChangeCallback, AdvanceDaysOffSchedulingService advanceDaysOffSchedulingService, MatrixListFactory matrixListFactory, IWeeklyRestSolverCommand weeklyRestSolverCommand, IGroupPersonBuilderWrapper groupPersonBuilderWrapper, IResourceCalculation resourceCalculation, IUserTimeZone userTimeZone, TeamBlockScheduleSelected teamBlockScheduleSelected, TeamInfoFactoryFactory teamInfoFactoryFactory, INightRestWhiteSpotSolverServiceFactory nightRestWhiteSpotSolverServiceFactory) : base(schedulerStateHolder, scheduleDayChangeCallback, advanceDaysOffSchedulingService, matrixListFactory, weeklyRestSolverCommand, groupPersonBuilderWrapper, resourceCalculation, userTimeZone, teamBlockScheduleSelected, teamInfoFactoryFactory, nightRestWhiteSpotSolverServiceFactory)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_teamBlockScheduleSelected = teamBlockScheduleSelected;
		}

		protected override void ScheduleSelected(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod, IEnumerable<IScheduleMatrixPro> matrixes,
			ISchedulePartModifyAndRollbackService rollbackService, ResourceCalculateDelayer resourceCalculateDelayer,
			ITeamInfoFactory teamInfoFactory)
		{
			_teamBlockScheduleSelected.ScheduleSelected(schedulingCallback, matrixes, selectedPeriod,
				selectedAgents, rollbackService, new MightResourceCalculateBeforeFindingShift(resourceCalculateDelayer),
				_schedulerStateHolder().SchedulingResultState, schedulingOptions, teamInfoFactory);
		}
	}
}