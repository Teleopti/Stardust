using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SkillGroup;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday,DefinedRaptorApplicationFunctionPaths.WebStaffing)]
	public class IntradaySkillAreaController : ApiController
	{
		private readonly CreateSkillGroup _createSkillGroup;
		private readonly FetchSkillGroup _fetchSkillGroup;
		private readonly DeleteSkillGroup _deleteSkillGroup;
		private readonly IAuthorization _authorization;

		public IntradaySkillAreaController(CreateSkillGroup createSkillGroup, 
			FetchSkillGroup fetchSkillGroup, 
			DeleteSkillGroup deleteSkillGroup, 
			IAuthorization authorization)
		{
			_createSkillGroup = createSkillGroup;
			_fetchSkillGroup = fetchSkillGroup;
			_deleteSkillGroup = deleteSkillGroup;
			_authorization = authorization;
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea)]
		[UnitOfWork, HttpPost, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult CreateSkillArea([FromBody]SkillAreaInput input)
		{
			if (!input.Skills.Any())
				return BadRequest("No skill selected");

			_createSkillGroup.Create(input.Name, input.Skills);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult GetSkillAreas()
		{
			return Ok(new SkillAreaInfo
			{
				HasPermissionToModifySkillArea = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea),
				SkillAreas = _fetchSkillGroup.GetAll()
			});
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea)]
		[UnitOfWork, HttpDelete, Route("api/intraday/skillarea/{id}")]
		public virtual IHttpActionResult DeleteSkillArea(Guid id)
		{
			_deleteSkillGroup.Do(id);
			return Ok();
		}
	}

	public class SkillAreaInfo
	{
		public bool HasPermissionToModifySkillArea { get; set; }
		public IEnumerable<SkillGroupViewModel> SkillAreas { get; set; }
	}

	public class SkillAreaInput
	{
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}