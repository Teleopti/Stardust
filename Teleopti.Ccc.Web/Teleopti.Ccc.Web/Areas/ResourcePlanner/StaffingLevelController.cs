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
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public StaffingLevelController(IEventPublisher publisher, ILoggedOnUser loggedOnUser, IPersonRepository personRepository,
			IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_publisher = publisher;
			_loggedOnUser = loggedOnUser;
			_personRepository = personRepository;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		[UnitOfWork, HttpGet, Route("ForecastAndStaffingForSkill")]
		public virtual IHttpActionResult ForecastAndStaffingForSkill(DateTime date, Guid skillId)
		{
			var timezoneOffset = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone().BaseUtcOffset;
			var intervals = _scheduleForecastSkillReadModelRepository.GetBySkill(skillId, date.Subtract(timezoneOffset),
				date.Add(TimeSpan.FromDays(1)).Subtract(timezoneOffset).AddSeconds(-1));
			foreach (var skillStaffingInterval in intervals)
			{
				skillStaffingInterval.StartDateTime = skillStaffingInterval.StartDateTime.Add(timezoneOffset);
				skillStaffingInterval.EndDateTime = skillStaffingInterval.EndDateTime.Add(timezoneOffset);
			}
			return Json(intervals);
		}

		[UnitOfWork, HttpGet, Route("ForecastAndStaffingForSkillArea")]
		public virtual IHttpActionResult ForecastAndStaffingForSkillArea(DateTime date, Guid skillAreaId)
		{
			var timezoneOffset = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone().BaseUtcOffset;
			var intervals = _scheduleForecastSkillReadModelRepository.GetBySkillArea(skillAreaId, date.Subtract(timezoneOffset),
				date.Add(TimeSpan.FromDays(1)).Subtract(timezoneOffset).AddSeconds(-1));
			foreach (var skillStaffingInterval in intervals)
			{
				skillStaffingInterval.StartDateTime  = skillStaffingInterval.StartDateTime.Add(timezoneOffset);
				skillStaffingInterval.EndDateTime = skillStaffingInterval.EndDateTime.Add(timezoneOffset);
			}
			return Json(intervals);
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
