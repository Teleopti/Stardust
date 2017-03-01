using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public abstract class IntradayOptimizationFromWebBase: IIntradayOptimizationFromWeb
	{
		private readonly IWebIntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPersonRepository _personRepository;

		protected IntradayOptimizationFromWebBase(IWebIntradayOptimizationCommandHandler intradayOptimizationCommandHandler,
			IPlanningPeriodRepository planningPeriodRepository, IPersonRepository personRepository)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_planningPeriodRepository = planningPeriodRepository;
			_personRepository = personRepository;
		}

		public void Execute(Guid planningPeriodId)
		{
			var intradayOptimizationCommand = IntradayOptimizationCommand(planningPeriodId);
			_intradayOptimizationCommandHandler.Execute(intradayOptimizationCommand);
		}

		[UnitOfWork]
		protected virtual IntradayOptimizationCommand IntradayOptimizationCommand(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			var jobResultId = SaveJobResultIfNeeded(planningPeriod);
			var intradayOptimizationCommand = new IntradayOptimizationCommand
			{
				Period = planningPeriod.Range,
				RunResolveWeeklyRestRule = true,
				JobResultId = jobResultId,
				PlanningPeriodId = planningPeriodId
			};
			if (planningPeriod.AgentGroup != null)
			{
				var people = _personRepository.FindPeopleInAgentGroup(planningPeriod.AgentGroup, planningPeriod.Range);
				intradayOptimizationCommand.AgentsToOptimize = people;
			}
			return intradayOptimizationCommand;
		}

		protected abstract Guid? SaveJobResultIfNeeded(IPlanningPeriod planningPeriod);
	}
}