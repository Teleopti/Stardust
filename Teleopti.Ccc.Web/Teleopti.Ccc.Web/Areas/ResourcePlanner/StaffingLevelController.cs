using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class StaffingLevelController : ApiController
	{
		private readonly IEventPublisher _publisher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleForecastSkillProvider _scheduleForecastSkillProvider;

		public StaffingLevelController(IEventPublisher publisher, ILoggedOnUser loggedOnUser, IPersonRepository personRepository,
			 IScheduleForecastSkillProvider scheduleForecastSkillProvider)
		{
			_publisher = publisher;
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
			_scheduleForecastSkillProvider = scheduleForecastSkillProvider;
		}

		[UnitOfWork, HttpGet, Route("ForecastAndStaffingForSkill")]
		public virtual IHttpActionResult ForecastAndStaffingForSkill(DateTime date, Guid skillId)
		{
			var intervals =  _scheduleForecastSkillProvider.GetBySkill(skillId, date);
			return Json(intervals);
		}

		[UnitOfWork, HttpGet, Route("ForecastAndStaffingForSkillArea")]
		public virtual IHttpActionResult ForecastAndStaffingForSkillArea(DateTime date, Guid skillAreaId)
		{
			var intervals = _scheduleForecastSkillProvider.GetBySkillArea(skillAreaId, date);
			return Json(intervals);
		}

		[UnitOfWork, HttpGet, Route("TriggerResourceCalculate")]
		public virtual IHttpActionResult TriggerResourceCalculate()
		{
			var period = getPublishedPeriod();
			if (period.HasValue)
			{
				_publisher.Publish(new UpdateResourceCalculateReadModelEvent()
				{
					InitiatorId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(),
					StartDateTime = period.Value.StartDate.Date,
					EndDateTime = period.Value.EndDate.Date.AddDays(1) //Add one day since it is a DateTimePeriod now
				});
			}
			return Ok();
		}

		private DateOnlyPeriod? getPublishedPeriod()
		{
			var allPeople = _personRepository.LoadAll();
			var maxPublishedDate =
				allPeople.Where(x => x.WorkflowControlSet != null)
					.Select(y => y.WorkflowControlSet)
					.Where(w => w.SchedulePublishedToDate != null)
					.Max(u => u.SchedulePublishedToDate);
			if (maxPublishedDate.HasValue)
			{
				if (maxPublishedDate.Value.Date < DateTime.Today)
					return null;
			}
			var publishedPeriod = new DateOnlyPeriod(DateOnly.Today, new DateOnly(maxPublishedDate.Value));
			return publishedPeriod;
		}

	}
}
