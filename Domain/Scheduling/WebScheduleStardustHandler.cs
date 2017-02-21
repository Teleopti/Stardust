using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class WebScheduleStardustHandler : IHandleEvent<WebScheduleStardustEvent>, IRunOnStardust
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;
		private readonly FullScheduling _fullScheduling;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IJobResultRepository _jobResultRepository;

		public WebScheduleStardustHandler(IPlanningPeriodRepository planningPeriodRepository, IAgentGroupStaffLoader agentGroupStaffLoader, FullScheduling fullScheduling, IEventPopulatingPublisher eventPublisher, IJobResultRepository jobResultRepository)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_agentGroupStaffLoader = agentGroupStaffLoader;
			_fullScheduling = fullScheduling;
			_eventPublisher = eventPublisher;
			_jobResultRepository = jobResultRepository;
		}

		[AsSystem]
		public virtual void Handle(WebScheduleStardustEvent @event)
		{
			try
			{
				var tuple = GetScheduleInfo(@event.PlanningPeriodId);
				var result = _fullScheduling.DoScheduling(tuple.Item1, tuple.Item2);
				var jobResultId = SaveJobResult(@event, result);
				_eventPublisher.Publish(new WebOptimizationStardustEvent(@event)
				{
					JobResultId = jobResultId
				});
			}
			catch (Exception e)
			{
				SaveExceptionDetailToJobResult(@event, DetailLevel.Error, null, e);
			}
		}

		[UnitOfWork]
		protected virtual Tuple<DateOnlyPeriod, IList<Guid>> GetScheduleInfo(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var people = _agentGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.AgentGroup);
			var peopleIds = people.AllPeople.Select(x => x.Id.Value).ToList();
			return new Tuple<DateOnlyPeriod, IList<Guid>>(period, peopleIds);
		}

		[UnitOfWork]
		protected virtual Guid SaveJobResult(WebScheduleStardustEvent @event, SchedulingResultModel result)
		{
			var planningPeriod = _planningPeriodRepository.Load(@event.PlanningPeriodId);
			var webScheduleJobResult = planningPeriod.JobResults.Single(x => x.Id.Value == @event.JobResultId);
			webScheduleJobResult.AddDetail(new JobResultDetail(DetailLevel.Info, JsonConvert.SerializeObject(result), DateTime.UtcNow, null));
			var jobResult = new JobResult(JobCategory.WebOptimization, webScheduleJobResult.Period, webScheduleJobResult.Owner, DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			planningPeriod.JobResults.Add(jobResult);
			return jobResult.Id.Value;
		}

		[UnitOfWork]
		protected virtual void SaveExceptionDetailToJobResult(WebScheduleStardustEvent @event, DetailLevel level, string message, Exception exception)
		{
			var planningPeriod = _planningPeriodRepository.Load(@event.PlanningPeriodId);
			var webScheduleJobResult = planningPeriod.JobResults.Single(x => x.Id.Value == @event.JobResultId);
			webScheduleJobResult.AddDetail(new JobResultDetail(level, message, DateTime.UtcNow, exception));
		}
	}
}