using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class StaffingController : ApiController
	{
		private readonly IAddOverTime _addOverTime;

		public StaffingController(IAddOverTime addOverTime)
		{
			_addOverTime = addOverTime;
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime/suggestion")]
		public virtual IHttpActionResult ShowAddOvertime([FromBody]OverTimeSuggestionModel model)
		{
			if (model == null || model.SkillIds.IsEmpty()) return BadRequest();

			var result =_addOverTime.GetSuggestion(model);
			
			return Ok(result);
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime")]
		public virtual IHttpActionResult AddOvertime([FromBody]IList<OverTimeModel> models)
		{

			if (models == null || models.IsEmpty()) return BadRequest();

			_addOverTime.Apply(models);

			return Ok();
		}

		//Remove stardust leftovers

	}
}
