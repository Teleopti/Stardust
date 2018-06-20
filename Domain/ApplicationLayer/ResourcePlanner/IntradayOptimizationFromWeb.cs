using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationFromWeb
	{
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPersonRepository _personRepository;

		protected IntradayOptimizationFromWeb(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler,
			IPlanningPeriodRepository planningPeriodRepository, IPersonRepository personRepository)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_planningPeriodRepository = planningPeriodRepository;
			_personRepository = personRepository;
		}

		public void Execute(Guid planningPeriodId)
		{
			var intradayOptimizationCommand = IntradayOptimizationCommand(planningPeriodId);
			if (intradayOptimizationCommand == null)
				return;
			
			_intradayOptimizationCommandHandler.Execute(intradayOptimizationCommand);
		}

		[UnitOfWork]
		protected virtual IntradayOptimizationCommand IntradayOptimizationCommand(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			var intradayOptimizationCommand = new IntradayOptimizationCommand
			{
				Period = planningPeriod.Range,
				RunResolveWeeklyRestRule = true,
				PlanningPeriodId = planningPeriodId
			};
			if (planningPeriod.PlanningGroup != null)
			{
				var people = _personRepository.FindPeopleInPlanningGroup(planningPeriod.PlanningGroup, planningPeriod.Range);
				if (!people.Any())
					return null;
				intradayOptimizationCommand.AgentsToOptimize = people;
			}
			return intradayOptimizationCommand;
		}
	}
}