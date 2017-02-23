using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulePlanningPeriodCommandHandler
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;
		private readonly FullScheduling _fullScheduling;
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;

		public SchedulePlanningPeriodCommandHandler(IPlanningPeriodRepository planningPeriodRepository, ILoggedOnUser loggedOnUser, IJobResultRepository jobResultRepository, IEventPopulatingPublisher eventPopulatingPublisher, FullScheduling fullScheduling, IAgentGroupStaffLoader agentGroupStaffLoader)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_loggedOnUser = loggedOnUser;
			_jobResultRepository = jobResultRepository;
			_eventPopulatingPublisher = eventPopulatingPublisher;
			_fullScheduling = fullScheduling;
			_agentGroupStaffLoader = agentGroupStaffLoader;
		}

		[UnitOfWork]
		public virtual void Execute(SchedulePlanningPeriodCommand command)
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
		}

		public SchedulingResultModel ExecuteAndReturn(SchedulePlanningPeriodCommand command)
		{
			var schedulingData = GetInfoFromPlanningPeriod(command.PlanningPeriodId);
			return _fullScheduling.DoScheduling(schedulingData.Item1, schedulingData.Item2);
		}

		[UnitOfWork]
		protected virtual Tuple<DateOnlyPeriod, IList<Guid>> GetInfoFromPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var people = _agentGroupStaffLoader.Load(period, planningPeriod.AgentGroup);
			return new Tuple<DateOnlyPeriod, IList<Guid>>(period, people.AllPeople.Select(x => x.Id.Value).ToList());
		}
	}
}