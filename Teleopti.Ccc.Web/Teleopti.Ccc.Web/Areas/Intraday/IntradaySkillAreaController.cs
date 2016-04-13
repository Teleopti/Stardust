using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradaySkillAreaController : ApiController
	{
        private readonly ISkillAreaRepository _skillAreaRepository;
        private readonly CreateSkillArea _createSkillArea;
		private readonly FetchSkillArea _fetchSkillArea;
		private readonly DeleteSkillArea _deleteSkillArea;
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly MonitorSkillsProvider _monitorSkillsProvider;

		public IntradaySkillAreaController(CreateSkillArea createSkillArea, FetchSkillArea fetchSkillArea, DeleteSkillArea deleteSkillArea, IPrincipalAuthorization principalAuthorization, MonitorSkillsProvider monitorSkillsProvider, ISkillAreaRepository skillAreaRepository)
		{
			_createSkillArea = createSkillArea;
			_fetchSkillArea = fetchSkillArea;
			_deleteSkillArea = deleteSkillArea;
			_principalAuthorization = principalAuthorization;
			_monitorSkillsProvider = monitorSkillsProvider;
		    _skillAreaRepository = skillAreaRepository;
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea)]
		[UnitOfWork, HttpPost, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult CreateSkillArea([FromBody]SkillAreaInput input)
		{
			_createSkillArea.Create(input.Name, input.Skills);
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skillarea")]
		public virtual IHttpActionResult GetSkillAreas()
		{
			return Ok(new SkillAreaInfo
			{
				HasPermissionToModifySkillArea = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkillArea),
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

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillarea/{id}")]
		public virtual IHttpActionResult MonitorSkillArea(Guid Id)
		{
            var skillArea = _skillAreaRepository.Get(Id);
            var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
            return Ok(_monitorSkillsProvider.Load(skillIdList));
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