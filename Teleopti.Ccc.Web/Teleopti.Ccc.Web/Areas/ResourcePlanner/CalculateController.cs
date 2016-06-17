using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class CalculateController : ApiController
	{
		private readonly IEventPublisher _publisher;
		private readonly ILoggedOnUser _loggedOnUser;
		private IPersonRepository _personRepository;

		public CalculateController(IEventPublisher publisher, ILoggedOnUser loggedOnUser, IPersonRepository personRepository)
		{
			_publisher = publisher;
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
		public virtual IHttpActionResult ResourceCalculate(DateTime date, Guid skillId)
		}

		[UnitOfWork, HttpGet, Route("TriggerResourceCalculate")]
		public virtual IHttpActionResult TriggerResourceCalculate()
		{
			var period = getPublishedPeriod();
			_publisher.Publish(new UpdateResourceCalculateReadModelEvent()
			{
				InitiatorId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(),
				JobName = "Resource Calculate",
				UserName = _loggedOnUser.CurrentUser().Id.GetValueOrDefault().ToString(),
				StartDateTime = period.StartDate.Date,
				EndDateTime = period.EndDate.Date
			});
			return Ok();
		}

		private DateOnlyPeriod getPublishedPeriod()
		{
			var allPeople = _personRepository.LoadAll();
			var maxPublishedDate =
				allPeople.Where(x => x.WorkflowControlSet != null)
					.Select(y => y.WorkflowControlSet)
					.Where(w => w.SchedulePublishedToDate != null)
					.Max(u => u.SchedulePublishedToDate);
			var publishedPeriod = new DateOnlyPeriod(DateOnly.Today, new DateOnly(maxPublishedDate.Value));
			return publishedPeriod;
		}

	}
}
