using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationFromWeb
	{
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPersonRepository _personRepository;

		public IntradayOptimizationFromWeb(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler, IPlanningPeriodRepository planningPeriodRepository, IPersonRepository personRepository)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_planningPeriodRepository = planningPeriodRepository;
			_personRepository = personRepository;
		}

		public virtual void Execute(Guid planningPeriodId)
		{
			var planningPeriod = LoadPlanningPeriod(planningPeriodId);
			IntradayOptimizationCommand intradayOptimizationCommand;
			if (planningPeriod.AgentGroup == null)
			{
				intradayOptimizationCommand = new IntradayOptimizationCommand
				{
					Period = planningPeriod.Range,
					RunResolveWeeklyRestRule = true
				};
			}
			else
			{
				var people = LoadPeople(planningPeriodId);
				intradayOptimizationCommand = new IntradayOptimizationCommand
				{
					Period = planningPeriod.Range,
					RunResolveWeeklyRestRule = true,
					AgentsToOptimize = people
				};
			}
			_intradayOptimizationCommandHandler.Execute(intradayOptimizationCommand);
		}

		[UnitOfWork]
		protected virtual IPlanningPeriod LoadPlanningPeriod(Guid planningPeriodId)
		{
			return _planningPeriodRepository.Get(planningPeriodId);
		}

		[UnitOfWork]
		protected virtual IList<IPerson> LoadPeople(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			return _personRepository.FindPeopleInAgentGroup(planningPeriod.AgentGroup, planningPeriod.Range);
		}
	}
}