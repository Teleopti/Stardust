using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationOnStardust
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public IntradayOptimizationOnStardust(IEventPublisher eventPublisher, IJobResultRepository jobResultRepository, ILoggedOnUser loggedOnUser, IPlanningPeriodRepository planningPeriodRepository)
		{
			_eventPublisher = eventPublisher;
			_jobResultRepository = jobResultRepository;
			_loggedOnUser = loggedOnUser;
			_planningPeriodRepository = planningPeriodRepository;
		}
	
		[UnitOfWork]
		public virtual void Execute(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			var jobResult = new JobResult(JobCategory.WebIntradayOptimiztion, planningPeriod.Range, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			_eventPublisher.Publish(new IntradayOptimizationOnStardustWasOrdered
			{
				PlanningPeriodId = planningPeriodId,
				JobResultId = jobResult.Id.Value
			});
		}
	}
}