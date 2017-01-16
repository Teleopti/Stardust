using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	//[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebStaffing)]
	public class StaffingController : ApiController
	{
		private readonly IStardustSender stardustSender;
		private readonly INow now;

		public StaffingController(IStardustSender stardustSender, INow now)
		{
			this.stardustSender = stardustSender;
			this.now = now;
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime")]
		public virtual IHttpActionResult AddOvertime([FromBody]AddOverTimeModel model)
		//public virtual IHttpActionResult AddOvertime()
		{
			if(model == null || model.Skills.IsEmpty()) return BadRequest();

			stardustSender.Send(new AddOverTimeEvent
								{
									OvertimeDurationMin = TimeSpan.FromHours(1),
									OvertimeDurationMax = TimeSpan.FromHours(5),
									OvertimeType = new Guid("29F7ECE8-D340-408F-BE40-9BB900B8A4CB"),
									Skills = model.Skills
								});
			return Ok();
		}

	}
}
