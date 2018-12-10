using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DesktopScheduling
	{
		private readonly CascadingResourceCalculationContextFactory _cascadingResourceCalculationContextFactory;
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ScheduleHourlyStaffExecutor _scheduleHourlyStaffExecutor;
		private readonly DesktopContextState _desktopContextState;

		public DesktopScheduling(CascadingResourceCalculationContextFactory cascadingResourceCalculationContextFactory,
			SchedulingCommandHandler schedulingCommandHandler, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceCalculation resourceCalculation,
			ScheduleHourlyStaffExecutor scheduleHourlyStaffExecutor,
			DesktopContextState desktopContextState)
		{
			_cascadingResourceCalculationContextFactory = cascadingResourceCalculationContextFactory;
			_schedulingCommandHandler = schedulingCommandHandler;
			_schedulerStateHolder = schedulerStateHolder;
			_resourceCalculation = resourceCalculation;
			_scheduleHourlyStaffExecutor = scheduleHourlyStaffExecutor;
			_desktopContextState = desktopContextState;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod)
		{
			var stateHolder = _schedulerStateHolder().SchedulingResultState;
			if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
			{
				var command = new SchedulingCommand
				{
					AgentsToSchedule = selectedAgents.FixedStaffPeople(selectedPeriod),
					Period = selectedPeriod,
					RunDayOffOptimization = false
				};
				using (_desktopContextState.SetForScheduling(command, _schedulerStateHolder(), schedulingOptions, schedulingCallback))
				{
					_schedulingCommandHandler.Execute(command);
				}
			}
			else
			{
				_scheduleHourlyStaffExecutor.Execute(schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod);
			}

			stateHolder.Schedules.ValidateBusinessRulesOnPersons(selectedAgents.FixedStaffPeople(selectedPeriod), NewBusinessRuleCollection.AllForScheduling(_schedulerStateHolder().SchedulingResultState));

			using (_cascadingResourceCalculationContextFactory.Create(stateHolder, false, selectedPeriod))
			{
				_resourceCalculation.ResourceCalculate(selectedPeriod, new ResourceCalculationData(stateHolder, false, false));
			}
		}
	}
}