using System;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
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

		public WebScheduleStardustHandler(IPlanningPeriodRepository planningPeriodRepository, IAgentGroupStaffLoader agentGroupStaffLoader, FullScheduling fullScheduling, IEventPopulatingPublisher eventPublisher)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_agentGroupStaffLoader = agentGroupStaffLoader;
			_fullScheduling = fullScheduling;
			_eventPublisher = eventPublisher;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(WebScheduleStardustEvent @event)
		{
			var planningPeriod = _planningPeriodRepository.Load(@event.PlanningPeriodId);
			var webScheduleJobResult = planningPeriod.JobResults.Single(x => x.JobCategory == JobCategory.WebSchedule);
			try
			{
				var period = planningPeriod.Range;
				var people = _agentGroupStaffLoader.Load(period, planningPeriod.AgentGroup);
				var result = _fullScheduling.DoScheduling(period, people.AllPeople.Select(x => x.Id.Value));
				webScheduleJobResult.AddDetail(new JobResultDetail(DetailLevel.Info, JsonConvert.SerializeObject(result), DateTime.UtcNow, null));

				planningPeriod.JobResults.Add(new JobResult(JobCategory.WebOptimization, webScheduleJobResult.Period, webScheduleJobResult.Owner, DateTime.UtcNow));
				_eventPublisher.Publish(new WebOptimizationStardustEvent(@event));
			}
			catch (Exception e)
			{
				webScheduleJobResult.AddDetail(new JobResultDetail(DetailLevel.Error, null, DateTime.UtcNow, e));
			}
		}
	}

}