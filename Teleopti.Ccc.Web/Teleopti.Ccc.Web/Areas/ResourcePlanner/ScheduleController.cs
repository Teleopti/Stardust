using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IEventPopulatingPublisher _eventPopulatingPublisher;
		private readonly ILoggedOnUser _loggedOnUser;

		public ScheduleController(IPlanningPeriodRepository planningPeriodRepository, IEventPopulatingPublisher eventPopulatingPublisher, ILoggedOnUser loggedOnUser)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_eventPopulatingPublisher = eventPopulatingPublisher;
			_loggedOnUser = loggedOnUser;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[UnitOfWork, HttpPost, Route("api/ResourcePlanner/Schedule/{id}")]
		public virtual IHttpActionResult ScheduleForPlanningPeriod(Guid id)
		{
			var planningPeriod = _planningPeriodRepository.Load(id);
			var jobResult = new JobResult(JobCategory.WebSchedule, planningPeriod.Range, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			planningPeriod.JobResults.Add(jobResult);
			_eventPopulatingPublisher.Publish(new WebScheduleStardustEvent
			{
				PlanningPeriodId = id
			});
			return Ok(jobResult);
		}
	}
}