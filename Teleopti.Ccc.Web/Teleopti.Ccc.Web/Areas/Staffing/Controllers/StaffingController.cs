using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class StaffingController : ApiController
    {
	    private readonly IStardustSender stardustSender;

	    public StaffingController(IStardustSender stardustSender)
	    {
		    this.stardustSender = stardustSender;
	    }

	    [UnitOfWork, HttpGet, Route("api/staffing/overtime")]
		//public virtual IHttpActionResult AddOvertime([FromBody]AddOverTimeModel model)
		public virtual IHttpActionResult AddOvertime()
	    {
		    stardustSender.Send(new AddOverTimeEvent {Period = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)), Skills = null});
			return Ok();
		}

	}
}
