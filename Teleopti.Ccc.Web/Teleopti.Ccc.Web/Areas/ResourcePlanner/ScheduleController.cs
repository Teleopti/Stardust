using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly FullScheduling _fullScheduling;
		private readonly IActionThrottler _actionThrottler;

		public ScheduleController(FullScheduling fullScheduling, IActionThrottler actionThrottler)
		{
			_fullScheduling = fullScheduling;
			_actionThrottler = actionThrottler;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff")]
		public virtual IHttpActionResult FixedStaff([FromBody] FixedStaffSchedulingInput input)
		{
			var token = _actionThrottler.Block(ThrottledAction.Scheduling);
			try
			{
				var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
				var result = _fullScheduling.DoScheduling(period);
				result.ThrottleToken = token;
				return Ok(result);
			}
			finally
			{
				_actionThrottler.Pause(token, TimeSpan.FromMinutes(1));
			}
		}
	}
}