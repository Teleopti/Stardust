using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationFromWeb
	{
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		protected IntradayOptimizationFromWeb(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler,
			IPlanningPeriodRepository planningPeriodRepository, IPersonRepository personRepository, IJobResultRepository jobResultRepository, ILoggedOnUser loggedOnUser)
		{
			_intradayOptimizationCommandHandler = intradayOptimizationCommandHandler;
			_planningPeriodRepository = planningPeriodRepository;
			_personRepository = personRepository;
			_jobResultRepository = jobResultRepository;
			_loggedOnUser = loggedOnUser;
		}

		public void Execute(Guid planningPeriodId, bool runAsynchronously)
		{
			var intradayOptimizationCommand = IntradayOptimizationCommand(planningPeriodId, runAsynchronously);
			if (intradayOptimizationCommand == null) return;
			_intradayOptimizationCommandHandler.Execute(intradayOptimizationCommand);
		}

		[UnitOfWork]
		protected virtual IntradayOptimizationCommand IntradayOptimizationCommand(Guid planningPeriodId, bool runAsynchronously)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			var intradayOptimizationCommand = new IntradayOptimizationCommand
			{
				Period = planningPeriod.Range,
				RunResolveWeeklyRestRule = true,
				PlanningPeriodId = planningPeriodId,
				RunAsynchronously = runAsynchronously
			};
			if (planningPeriod.PlanningGroup != null)
			{
				var people = _personRepository.FindPeopleInPlanningGroup(planningPeriod.PlanningGroup, planningPeriod.Range);
				if (!people.Any()) return null;
				intradayOptimizationCommand.AgentsToOptimize = people;
			}
			if (runAsynchronously)
			{
				var jobResultId = saveJobResult(planningPeriod);
				intradayOptimizationCommand.JobResultId = jobResultId;
			}
			return intradayOptimizationCommand;
		}

		private Guid? saveJobResult(IPlanningPeriod planningPeriod)
		{
			var jobResult = new JobResult(JobCategory.WebIntradayOptimiztion, planningPeriod.Range, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			planningPeriod.JobResults.Add(jobResult);
			return jobResult.Id;
		}
	}
}