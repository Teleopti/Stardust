using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OrganizeCascadingSkills)]
	public class SkillRoutingPriorityController : ApiController
	{
		private readonly SkillRoutingPriorityModel _skillRoutingPriorityModel;
		private readonly SkillRoutingPriorityPersister _skillRoutingPriorityPersister;

		public SkillRoutingPriorityController(SkillRoutingPriorityModel skillRoutingPriorityModel, SkillRoutingPriorityPersister skillRoutingPriorityPersister)
		{
			_skillRoutingPriorityModel = skillRoutingPriorityModel;
			_skillRoutingPriorityPersister = skillRoutingPriorityPersister;
		}

		[Route("api/ResourcePlanner/AdminSkillRoutingActivity"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult AdminSkillRoutingActivity()
		{
			var activityList = _skillRoutingPriorityModel.SkillRoutingActivites();
			return Ok(activityList);
		}

		[Route("api/ResourcePlanner/AdminSkillRoutingPriority"), HttpGet, UnitOfWork]
		public virtual IHttpActionResult AdminSkillRoutingPriority()
		{
			var skillList = _skillRoutingPriorityModel.SkillRoutingPriorityModelRows();
			return Ok(skillList); //current
		}

		[Route("api/ResourcePlanner/AdminSkillRoutingPriorityPost"), HttpPost, UnitOfWork]
		public virtual IHttpActionResult AdminSkillRoutingPriorityPost(IEnumerable<SkillRoutingPriorityModelRow> result)
		{
			try
			{
				_skillRoutingPriorityPersister.Persist(result);
			}
			catch (Exception)
			{
				return BadRequest();
			}
			return Ok();
		}
	}
}