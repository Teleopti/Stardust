using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradaySkillAreaController : ApiController
	{
		private readonly CreateSkillArea _createSkillArea;

		public IntradaySkillAreaController(CreateSkillArea createSkillArea)
		{
			_createSkillArea = createSkillArea;
		}

		[UnitOfWork, HttpPost, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult CreateSkillArea([FromBody]SkillAreaInput input)
		{
			_createSkillArea.Create(input.Name, input.Skills);
			return Ok();
		}
	}

	public class SkillAreaInput
	{
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}