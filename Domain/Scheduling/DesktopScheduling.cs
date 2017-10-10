using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DesktopScheduling
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resouceResourceOptimizationHelper;
		private readonly ScheduleHourlyStaffExecutor _scheduleHourlyStaffExecutor;
		private readonly DesktopSchedulingContext _desktopSchedulingContext;

		public DesktopScheduling(SchedulingCommandHandler schedulingCommandHandler, 
			Func<ISchedulerStateHolder> schedulerStateHolder,
			IResourceCalculation resouceResourceOptimizationHelper,
			ScheduleHourlyStaffExecutor scheduleHourlyStaffExecutor,
			DesktopSchedulingContext desktopSchedulingContext)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_schedulerStateHolder = schedulerStateHolder;
			_resouceResourceOptimizationHelper = resouceResourceOptimizationHelper;
			_scheduleHourlyStaffExecutor = scheduleHourlyStaffExecutor;
			_desktopSchedulingContext = desktopSchedulingContext;
		}

		public void Execute(ISchedulingCallback schedulingCallback, SchedulingOptions schedulingOptions,
			ISchedulingProgress backgroundWorker, IEnumerable<IPerson> selectedAgents, DateOnlyPeriod selectedPeriod)
		{

			if (schedulingOptions.ScheduleEmploymentType == ScheduleEmploymentType.FixedStaff)
			{
				var command = new SchedulingCommand
				{
					AgentsToSchedule = selectedAgents.FixedStaffPeople(selectedPeriod),
					Period = selectedPeriod,
					RunWeeklyRestSolver = true
				};
				using (_desktopSchedulingContext.Set(command, _schedulerStateHolder(), schedulingOptions, schedulingCallback))
				{
					_schedulingCommandHandler.Execute(command);
				}
			}
			else
			{
				_scheduleHourlyStaffExecutor.Execute(schedulingOptions, backgroundWorker, selectedAgents, selectedPeriod);
			}

			var resCalcData = _schedulerStateHolder().SchedulingResultState.ToResourceOptimizationData(_schedulerStateHolder().ConsiderShortBreaks, false);
			_resouceResourceOptimizationHelper.ResourceCalculate(selectedPeriod, resCalcData);
		}
	}
}