using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SkillGroup
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday,
		DefinedRaptorApplicationFunctionPaths.WebStaffing)]
	public class SkillGroupController : ApiController
	{
		private readonly IAuthorization _authorization;
		private readonly CreateSkillGroup _createSkillGroup;
		private readonly DeleteSkillGroup _deleteSkillGroup;
		private readonly SkillGroupViewModelBuilder _skillGroupViewModelBuilder;
		private readonly ModifySkillGroup _modifySkillGroup;
		private readonly ISkillGroupRepository _skillGroupRepository;


		public SkillGroupController(
			CreateSkillGroup createSkillGroup,
			SkillGroupViewModelBuilder skillGroupViewModelBuilder,
			DeleteSkillGroup deleteSkillGroup,
			ModifySkillGroup modifySkillGroup,
			IAuthorization authorization,
			ISkillGroupRepository skillGroupRepository
		)
		{
			_createSkillGroup = createSkillGroup;
			_skillGroupViewModelBuilder = skillGroupViewModelBuilder;
			_deleteSkillGroup = deleteSkillGroup;
			_authorization = authorization;
			_modifySkillGroup = modifySkillGroup;
			_skillGroupRepository = skillGroupRepository;
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup)]
		[UnitOfWork]
		[HttpPost]
		[Route("api/skillgroup/create")]
		public virtual IHttpActionResult CreateSkillGroup([FromBody] SkillGroupInput input)
		{
			if (!input.Skills.Any())
				return BadRequest("No skill selected");

			_createSkillGroup.Create(input.Name, input.Skills);
			return Ok();
		}

		[UnitOfWork]
		[HttpGet]
		[Route("api/skillgroup/skillgroups")]
		public virtual IHttpActionResult GetSkillGroups()
		{
			return Ok(new SkillGroupInfo
			{
				HasPermissionToModifySkillArea =
					_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup),
				SkillAreas = _skillGroupViewModelBuilder.GetAllWithAtleastOneQueue()
			});
		}

		[UnitOfWork]
		[HttpGet]
		[Route("api/skillgroup/skillgroupsallskills")]
		public virtual IHttpActionResult GetSkillGroupsWithAllSkills()
		{
			return Ok(new SkillGroupInfo
			{
				HasPermissionToModifySkillArea =
					_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup),
				SkillAreas = _skillGroupViewModelBuilder.GetAllSkills()
			});
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup)]
		[UnitOfWork]
		[HttpDelete]
		[Route("api/skillgroup/delete/{id}")]
		public virtual IHttpActionResult DeleteSkillGroup(Guid id)
		{
			_deleteSkillGroup.Do(id);
			return Ok();
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillGroup)]
		[UnitOfWork]
		[HttpPut]
		[Route("api/skillgroup/update")]
		public virtual IHttpActionResult ModifySkillGroup([FromBody] List<SGMGroup> input)
		{
			try
			{
				foreach (var inputSkillGroup in input)
				{

					Guid id;
					if (Guid.TryParse(inputSkillGroup.Id, out id))
					{
						var skillGroup = _skillGroupRepository.Get(id);
						if (skillGroup != null)
						{
							_modifySkillGroup.Do(inputSkillGroup);
						}
					}
					else
					{
						var skillIds = inputSkillGroup.Skills.Select(x => Guid.Parse(x.Id.ToString()));
						_createSkillGroup.Create(inputSkillGroup.Name, skillIds);
					}
				}
				return Ok(input);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return InternalServerError(e);
			}
		}
	}
}