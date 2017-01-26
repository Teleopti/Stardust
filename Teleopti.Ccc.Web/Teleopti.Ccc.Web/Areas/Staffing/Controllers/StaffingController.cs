using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class StaffingController : ApiController
	{
		private readonly IStardustSender _stardustSender;

		public StaffingController(IStardustSender stardustSender)
		{
			_stardustSender = stardustSender;
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime")]
		public virtual IHttpActionResult AddOvertime([FromBody]AddOverTimeModel model)
		{

			if(model == null || model.Skills.IsEmpty()) return BadRequest();

			_stardustSender.Send(new AddOverTimeEvent
								{
									OvertimeDurationMin = TimeSpan.FromHours(1),
									OvertimeDurationMax = TimeSpan.FromHours(5),
									Skills = model.Skills
								});
			return Ok();
		}

		[UnitOfWork, HttpPost, Route("api/staffing/staffing")]
		public virtual IHttpActionResult GetStaffingData()
		{

			return Ok();
		}

	}
}
