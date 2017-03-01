using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
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
			var jobResult = new JobResult(JobCategory.WebIntradayOptimiztion, planningPeriod.Range, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			planningPeriod.JobResults.Add(jobResult);
			return jobResult.Id;
		}
	}
}