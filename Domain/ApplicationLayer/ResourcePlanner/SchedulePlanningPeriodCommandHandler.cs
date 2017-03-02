using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulePlanningPeriodCommandHandler : ISchedulePlanningPeriodCommandHandler
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly FullScheduling _fullScheduling;
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;

		public SchedulePlanningPeriodCommandHandler(IPlanningPeriodRepository planningPeriodRepository, FullScheduling fullScheduling, IAgentGroupStaffLoader agentGroupStaffLoader, ILoggedOnUser loggedOnUser, IJobResultRepository jobResultRepository, IEventPopulatingPublisher eventPopulatingPublisher)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_fullScheduling = fullScheduling;
			_agentGroupStaffLoader = agentGroupStaffLoader;
			_loggedOnUser = loggedOnUser;
			_jobResultRepository = jobResultRepository;
			_eventPopulatingPublisher = eventPopulatingPublisher;
		}

		public object Execute(SchedulePlanningPeriodCommand command)
		{
			if (command.RunAsynchronously)
				return ExecuteAsync(command);
			return ExecuteSync(command);
		}

		[UnitOfWork]
		protected virtual Guid ExecuteAsync(SchedulePlanningPeriodCommand command)
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

		protected SchedulingResultModel ExecuteSync(SchedulePlanningPeriodCommand command)
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