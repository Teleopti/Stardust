using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationCommandHandler : IDayOffOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public DayOffOptimizationCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}
		
		public void Execute(DayOffOptimizationCommand command, ISchedulingProgress schedulingProgress,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var evt = new DayOffOptimizationWasOrdered
			{
				StartDate = command.Period.StartDate,
				EndDate = command.Period.EndDate,
				Agents = command.AgentsToOptimize,
				RunWeeklyRestSolver = command.RunWeeklyRestSolver,
				PlanningPeriodId = command.PlanningPeriodId,
				CommandId = command.CommandId
			};
			_eventPublisher.Publish(evt);
		}
	}
}