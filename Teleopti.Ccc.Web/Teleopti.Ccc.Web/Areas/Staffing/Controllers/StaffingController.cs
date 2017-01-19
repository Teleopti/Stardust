using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	//[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebStaffing)]
	public class StaffingController : ApiController
	{
		private readonly IStardustSender stardustSender;

		public StaffingController(IStardustSender stardustSender, INow now)
		{
			this.stardustSender = stardustSender;
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
									Skills = model.Skills
								});
			return Ok();
		}

	}
}
