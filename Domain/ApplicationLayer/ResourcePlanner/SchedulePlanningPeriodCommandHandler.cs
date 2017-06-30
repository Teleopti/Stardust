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
	public class SchedulePlanningPeriodCommandHandler
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IFullScheduling _fullScheduling;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;

		public SchedulePlanningPeriodCommandHandler(IPlanningPeriodRepository planningPeriodRepository, IFullScheduling fullScheduling, IPlanningGroupStaffLoader planningGroupStaffLoader, ILoggedOnUser loggedOnUser, IJobResultRepository jobResultRepository, IEventPopulatingPublisher eventPopulatingPublisher)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_fullScheduling = fullScheduling;
			_planningGroupStaffLoader = planningGroupStaffLoader;
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
			return _fullScheduling.DoScheduling(schedulingData.Period, schedulingData.PersonIds);
		}

		[UnitOfWork]
		protected virtual SchedulingInformation GetInfoFromPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			if (planningPeriod.PlanningGroup != null)
			{
				return new SchedulingInformation(period,
					_planningGroupStaffLoader.LoadPersonIds(period, planningPeriod.PlanningGroup));
			}
			var people = _planningGroupStaffLoader.Load(period, null);
			return new SchedulingInformation(period, people.AllPeople.Select(x => x.Id.Value).ToList());
		}

		protected class SchedulingInformation
		{
			public IList<Guid> PersonIds { get; }
			public DateOnlyPeriod Period { get; }

			public SchedulingInformation(DateOnlyPeriod period, IList<Guid> personIds)
			{
				PersonIds = personIds;
				Period = period;
			}
		}
	}
}