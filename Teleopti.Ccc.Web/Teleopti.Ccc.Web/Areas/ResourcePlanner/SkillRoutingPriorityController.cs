using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OrganizeCascadingSkills)]
	public class SkillRoutingPriorityController : ApiController
	{
		private readonly SkillRoutingPriorityModel _skillRoutingPriorityModel;

		public SkillRoutingPriorityController(SkillRoutingPriorityModel skillRoutingPriorityModel)
		{
			_skillRoutingPriorityModel = skillRoutingPriorityModel;
		}

		[Route("api/ResourcePlanner/AdminSkillRoutingActivity"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult AdminSkillRoutingActivity()
		{
			var resultList = _skillRoutingPriorityModel.SkillRoutingActivites();
			return Ok(resultList);
		}

		[Route("api/ResourcePlanner/AdminSkillRoutingPriority"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult AdminSkillRoutingPriority()
		{
			var skillList = _skillRoutingPriorityModel.SkillRoutingPriorityModelRows();
			return Ok(skillList); //current
		}


	}
}