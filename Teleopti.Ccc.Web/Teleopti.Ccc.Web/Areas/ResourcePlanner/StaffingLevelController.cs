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
		private readonly IScheduleForecastSkillProvider _scheduleForecastSkillProvider;
		private readonly INow _now;

		public StaffingLevelController(IEventPublisher publisher,
			IScheduleForecastSkillProvider scheduleForecastSkillProvider, INow now)
		{
			_publisher = publisher;
			_scheduleForecastSkillProvider = scheduleForecastSkillProvider;
			_now = now;
		}

		[UnitOfWork, HttpGet, Route("ForecastAndStaffingForSkill")]
		public virtual IHttpActionResult ForecastAndStaffingForSkill(DateTime date, Guid skillId)
		{
			var intervals = _scheduleForecastSkillProvider.GetBySkill(skillId, date);
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
			var now = _now.UtcDateTime();
			_publisher.Publish(new UpdateResourceCalculateReadModelEvent()
			{
				StartDateTime = now,
				EndDateTime = now.AddHours(24)
			});

			return Ok();
		}

		[UnitOfWork, HttpGet, Route("GetLastCaluclatedDateTime")]
		public virtual IHttpActionResult GetLastCaluclatedDateTime()
		{
			return Json(_scheduleForecastSkillProvider.GetLastCaluclatedDateTime());
		}
	}
}
