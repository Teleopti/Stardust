using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class ClearPlanningPeriodSchedulingCommandHandler
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;

		public ClearPlanningPeriodSchedulingCommandHandler(IPlanningPeriodRepository planningPeriodRepository, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IPersonAssignmentRepository personAssignmentRepository)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_personAssignmentRepository = personAssignmentRepository;
		}

		[UnitOfWork]
		public virtual void ClearSchedules(ClearPlanningPeriodSchedulingCommand command)
		{
			var planningPeriod = _planningPeriodRepository.Get(command.PlanningPeriodId);
			if (planningPeriod == null) throw new Exception("Blargh");
			if (planningPeriod.AgentGroup == null) throw new Exception("Blargh");

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var people = _personRepository.FindPeopleInAgentGroup(planningPeriod.AgentGroup, planningPeriod.Range);
			
			foreach (var personAssignment in _personAssignmentRepository.Find(people, planningPeriod.Range, scenario))
			{
				if (personAssignment.PersonalActivities().Any())
				{
					personAssignment.ClearMainActivities();
				}
				else
				{
					_personAssignmentRepository.Remove(personAssignment);
				}
			}
			planningPeriod.Reset();
		}
	}

	public class ClearPlanningPeriodSchedulingCommand
	{
		public Guid PlanningPeriodId { get; set; }
	}
}