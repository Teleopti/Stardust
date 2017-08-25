using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday,DefinedRaptorApplicationFunctionPaths.WebStaffing)]
	public class IntradaySkillAreaController : ApiController
	{
		private readonly CreateSkillArea _createSkillArea;
		private readonly FetchSkillArea _fetchSkillArea;
		private readonly DeleteSkillArea _deleteSkillArea;
		private readonly IAuthorization _authorization;

		public IntradaySkillAreaController(CreateSkillArea createSkillArea, 
			FetchSkillArea fetchSkillArea, 
			DeleteSkillArea deleteSkillArea, 
			IAuthorization authorization)
		{
			_createSkillArea = createSkillArea;
			_fetchSkillArea = fetchSkillArea;
			_deleteSkillArea = deleteSkillArea;
			_authorization = authorization;
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea)]
		[UnitOfWork, HttpPost, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult CreateSkillArea([FromBody]SkillAreaInput input)
		{
			if (!input.Skills.Any())
				return BadRequest("No skill selected");

			_createSkillArea.Create(input.Name, input.Skills);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult GetSkillAreas()
		{
			return Ok(new SkillAreaInfo
			{
				HasPermissionToModifySkillArea = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea),
				SkillAreas = _fetchSkillArea.GetAll()
			});
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea)]
		[UnitOfWork, HttpDelete, Route("api/intraday/skillarea/{id}")]
		public virtual IHttpActionResult DeleteSkillArea(Guid id)
		{
			_deleteSkillArea.Do(id);
			return Ok();
		}
	}

	public class SkillAreaInfo
	{
		public bool HasPermissionToModifySkillArea { get; set; }
		public IEnumerable<SkillAreaViewModel> SkillAreas { get; set; }
	}

	public class SkillAreaInput
	{
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}