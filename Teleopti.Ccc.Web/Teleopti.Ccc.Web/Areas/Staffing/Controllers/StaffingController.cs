using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class StaffingController : ApiController
	{
		private readonly IStardustSender _stardustSender;
		private readonly IAddOverTime _addOverTime;

		public StaffingController(IStardustSender stardustSender, IAddOverTime addOverTime)
		{
			_stardustSender = stardustSender;
			_addOverTime = addOverTime;
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime/suggestion")]
		public virtual IHttpActionResult ShowAddOvertime([FromBody]AddOverTimeModel model)
		{
			if (model == null || model.Skills.IsEmpty()) return BadRequest();

			var result =_addOverTime.GetSuggestion(model.Skills);

			return Ok(result);
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime")]
		public virtual IHttpActionResult AddOvertime([FromBody]AddOverTimeModel model)
		{

			if (model == null || model.Skills.IsEmpty()) return BadRequest();

			_stardustSender.Send(new AddOverTimeEvent
			{
				OvertimeDurationMin = TimeSpan.FromHours(1),
				OvertimeDurationMax = TimeSpan.FromHours(5),
				Skills = model.Skills
			});
			return Ok();
		}

	}
}
