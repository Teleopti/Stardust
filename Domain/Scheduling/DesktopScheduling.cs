using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[RemoveMeWithToggle("Merge with base class", Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public class DesktopSchedulingNew : DesktopScheduling
	{
		private readonly ResourceCalculateWithNewContext _resourceCalculateWithNewContext;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public DesktopSchedulingNew(ResourceCalculateWithNewContext resourceCalculateWithNewContext, SchedulingCommandHandler schedulingCommandHandler, Func<ISchedulerStateHolder> schedulerStateHolder, IResourceCalculation resouceResourceOptimizationHelper, ScheduleHourlyStaffExecutor scheduleHourlyStaffExecutor, DesktopSchedulingContext desktopSchedulingContext) : base(schedulingCommandHandler, schedulerStateHolder, resouceResourceOptimizationHelper, scheduleHourlyStaffExecutor, desktopSchedulingContext)
		{
			_resourceCalculateWithNewContext = resourceCalculateWithNewContext;
			_schedulerStateHolder = schedulerStateHolder;
		}

		protected override void PostScheduling(DateOnlyPeriod selectedPeriod)
		{
			_resourceCalculateWithNewContext.ResourceCalculate(selectedPeriod, new ResourceCalculationData(_schedulerStateHolder().SchedulingResultState, false, false));
		}
	}
	
	public class DesktopScheduling
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IResourceCalculation _resouceResourceOptimizationHelper;
		private readonly ScheduleHourlyStaffExecutor _scheduleHourlyStaffExecutor;
		private readonly DesktopSchedulingContext _desktopSchedulingContext;

		[RemoveMeWithToggle("remove unused params", Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
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

			PostScheduling(selectedPeriod);
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
		protected virtual void PostScheduling(DateOnlyPeriod selectedPeriod)
		{
			var resCalcData = _schedulerStateHolder().SchedulingResultState
				.ToResourceOptimizationData(_schedulerStateHolder().ConsiderShortBreaks, false);
			_resouceResourceOptimizationHelper.ResourceCalculate(selectedPeriod, resCalcData);
		}
	}
}