using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationFromWeb: IntradayOptimizationFromWebBase
	{
		public IntradayOptimizationFromWeb(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler,
			IPlanningPeriodRepository planningPeriodRepository, IPersonRepository personRepository)
			: base(intradayOptimizationCommandHandler, planningPeriodRepository, personRepository)
		{
		}

		protected override Guid? SaveJobResultIfNeeded(IPlanningPeriod planningPeriod)
		{
			// no need to save job result
			return null;
		}
	}

	public interface IIntradayOptimizationFromWeb
	{
		void Execute(Guid planningPeriodId);
	}

	public class IntradayOptimizationJobFromWeb : IntradayOptimizationFromWebBase
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public IntradayOptimizationJobFromWeb(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler,
			IPlanningPeriodRepository planningPeriodRepository, IPersonRepository personRepository,
			IJobResultRepository jobResultRepository, ILoggedOnUser loggedOnUser)
			: base(intradayOptimizationCommandHandler, planningPeriodRepository, personRepository)
		{
			_jobResultRepository = jobResultRepository;
			_loggedOnUser = loggedOnUser;
		}

		protected override Guid? SaveJobResultIfNeeded(IPlanningPeriod planningPeriod)
		{
			var jobResult = new JobResult(JobCategory.WebSchedule, planningPeriod.Range, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			planningPeriod.JobResults.Add(jobResult);
			return jobResult.Id;
		}
	}

	public abstract class IntradayOptimizationFromWebBase: IIntradayOptimizationFromWeb
	{
		private readonly IntradayOptimizationCommandHandler _intradayOptimizationCommandHandler;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPersonRepository _personRepository;

		protected IntradayOptimizationFromWebBase(IntradayOptimizationCommandHandler intradayOptimizationCommandHandler,
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
				JobResultId = jobResultId
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