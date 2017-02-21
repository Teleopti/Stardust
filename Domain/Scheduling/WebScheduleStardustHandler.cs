using System;
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
	public class WebScheduleStardustHandler :
		IHandleEvent<WebScheduleStardustEvent>,
		IRunOnStardust
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

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(WebScheduleStardustEvent @event)
		{
			var planningPeriod = _planningPeriodRepository.Load(@event.PlanningPeriodId);
			var webScheduleJobResult = planningPeriod.JobResults.Single(x => x.Id.Value == @event.JobResultId);
			try
			{
				var period = planningPeriod.Range;
				var people = _agentGroupStaffLoader.Load(period, planningPeriod.AgentGroup);
				var result = _fullScheduling.DoScheduling(period, people.AllPeople.Select(x => x.Id.Value));
				webScheduleJobResult.AddDetail(new JobResultDetail(DetailLevel.Info, JsonConvert.SerializeObject(result), DateTime.UtcNow, null));

				var jobResult = new JobResult(JobCategory.WebOptimization, webScheduleJobResult.Period, webScheduleJobResult.Owner, DateTime.UtcNow);
				_jobResultRepository.Add(jobResult);
				planningPeriod.JobResults.Add(jobResult);
				_eventPublisher.Publish(new WebOptimizationStardustEvent(@event)
				{
					JobResultId = jobResult.Id.Value
				});
			}
			catch (Exception e)
			{
				webScheduleJobResult.AddDetail(new JobResultDetail(DetailLevel.Error, null, DateTime.UtcNow, e));
			}
		}
	}

}