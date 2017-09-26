using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SkillGroup
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday,DefinedRaptorApplicationFunctionPaths.WebStaffing)]
	public class SkillGroupController : ApiController
	{
		private readonly CreateSkillGroup _createSkillGroup;
		private readonly FetchSkillGroup _fetchSkillGroup;
		private readonly DeleteSkillGroup _deleteSkillGroup;
		private readonly IAuthorization _authorization;

		public SkillGroupController(CreateSkillGroup createSkillGroup, 
			FetchSkillGroup fetchSkillGroup, 
			DeleteSkillGroup deleteSkillGroup, 
			IAuthorization authorization)
		{
			_createSkillGroup = createSkillGroup;
			_fetchSkillGroup = fetchSkillGroup;
			_deleteSkillGroup = deleteSkillGroup;
			_authorization = authorization;
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup)]
		[UnitOfWork, HttpPost, Route("api/skillgroup/create")]
		public virtual IHttpActionResult CreateSkillGroup([FromBody]SkillGroupInput input)
		{
			if (!input.Skills.Any())
				return BadRequest("No skill selected");

			_createSkillGroup.Create(input.Name, input.Skills);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/skillgroup/skillgroups")]
		public virtual IHttpActionResult GetSkillGroups()
		{
			return Ok(new SkillGroupInfo
			{
				HasPermissionToModifySkillArea = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup),
				SkillAreas = _fetchSkillGroup.GetAll()
			});
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup)]
		[UnitOfWork, HttpDelete, Route("api/skillgroup/delete/{id}")]
		public virtual IHttpActionResult DeleteSkillGroup(Guid id)
		{
			_deleteSkillGroup.Do(id);
			return Ok();
		}
	}
}