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
		//private readonly ISkillRoutingPriorityModel _skillRoutingPriorityModel;

		//SkillRoutingPriorityController(ISkillRoutingPriorityModel skillRoutingPriorityModel)
		//{
		//	_skillRoutingPriorityModel = skillRoutingPriorityModel;
		//}

		[Route("api/ResourcePlanner/AdminSkillRoutingPriority"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult AdminSkillRoutingPriority()
		{
			var skillList = new List<int>(); //_skillRoutingPriorityModel.SkillRoutingPriorityModelRows();
			return Ok(skillList); //current
		}


	}
}