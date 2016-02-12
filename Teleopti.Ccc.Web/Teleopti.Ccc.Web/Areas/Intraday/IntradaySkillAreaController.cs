using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradaySkillAreaController : ApiController
	{
		private readonly CreateSkillArea _createSkillArea;
		private readonly FetchSkillArea _fetchSkillArea;

		public IntradaySkillAreaController(CreateSkillArea createSkillArea, FetchSkillArea fetchSkillArea)
		{
			_createSkillArea = createSkillArea;
			_fetchSkillArea = fetchSkillArea;
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea)]
		[UnitOfWork, HttpPost, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult CreateSkillArea([FromBody]SkillAreaInput input)
		{
			_createSkillArea.Create(input.Name, input.Skills);
			return Ok();
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
		[UnitOfWork, HttpGet, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult GetSkillAreas()
		{
			var skillAreas = _fetchSkillArea.GetAll();
			return Ok(skillAreas);
		}
	}

	public class SkillAreaInput
	{
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}