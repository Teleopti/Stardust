using System;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class ClearPlanningPeriodSchedule
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public ClearPlanningPeriodSchedule(IPlanningPeriodRepository planningPeriodRepository, IScenarioRepository scenarioRepository, IPersonRepository personRepository, IPersonAssignmentRepository personAssignmentRepository, ICurrentUnitOfWork currentUnitOfWork)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_currentUnitOfWork = currentUnitOfWork;
		}
		
		public void ClearSchedules(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null) throw new Exception("PlanningPeriod not found");
			if (planningPeriod.PlanningGroup == null) throw new Exception("PlanningGroup not found");

			var scenario = _scenarioRepository.LoadDefaultScenario();
			var people = _personRepository.FindPeopleInPlanningGroup(planningPeriod.PlanningGroup, planningPeriod.Range);
			foreach (var batch in people.Batch(200))
			{
				foreach (var personAssignment in _personAssignmentRepository.Find(batch, planningPeriod.Range, scenario, ScheduleSource.WebScheduling))
				{
					personAssignment.ClearMainActivities(false);
					personAssignment.SetDayOff(null);

					if (!personAssignment.PersonalActivities().Any())
						_personAssignmentRepository.Remove(personAssignment);
				}
				_currentUnitOfWork.Current().PersistAll();
			}
			planningPeriod.Reset();
		}
	}
}