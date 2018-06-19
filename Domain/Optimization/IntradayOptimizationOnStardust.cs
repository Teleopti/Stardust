using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationOnStardust
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public IntradayOptimizationOnStardust(IEventPublisher eventPublisher, IJobResultRepository jobResultRepository, ILoggedOnUser loggedOnUser)
		{
			_eventPublisher = eventPublisher;
			_jobResultRepository = jobResultRepository;
			_loggedOnUser = loggedOnUser;
		}
	
		[UnitOfWork]
		public virtual void Execute(Guid planningPeriodId)
		{
			var jobResult = new JobResult(JobCategory.WebIntradayOptimiztion, new DateOnlyPeriod(), _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			_eventPublisher.Publish(new IntradayOptimizationOnStardustWasOrdered
			{
				PlanningPeriodId = planningPeriodId,
				JobResultId = jobResult.Id.Value
			});
		}
	}
}