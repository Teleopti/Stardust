using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly FullScheduling _fullScheduling;

		public ScheduleController(FullScheduling fullScheduling)
		{
			_fullScheduling = fullScheduling;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff")]
		public virtual IHttpActionResult FixedStaff([FromBody] FixedStaffSchedulingInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			return Ok(_fullScheduling.DoScheduling(period));
		}
	}
}