using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulePlanningPeriodTaskCommandHandler: ISchedulePlanningPeriodCommandHandler
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;

		public SchedulePlanningPeriodTaskCommandHandler(IPlanningPeriodRepository planningPeriodRepository, ILoggedOnUser loggedOnUser, IJobResultRepository jobResultRepository, IEventPopulatingPublisher eventPopulatingPublisher)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_loggedOnUser = loggedOnUser;
			_jobResultRepository = jobResultRepository;
			_eventPopulatingPublisher = eventPopulatingPublisher;
		}

		[UnitOfWork]
		public virtual object Execute(SchedulePlanningPeriodCommand command)
		{
			var planningPeriod = _planningPeriodRepository.Load(command.PlanningPeriodId);
			var jobResult = new JobResult(JobCategory.WebSchedule, planningPeriod.Range, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			planningPeriod.JobResults.Add(jobResult);
			_eventPopulatingPublisher.Publish(new WebScheduleStardustEvent
			{
				PlanningPeriodId = command.PlanningPeriodId,
				JobResultId = jobResult.Id.Value
			});
			return jobResult.Id.Value;
		}
	}
}