using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class RtaSkillAreaController : ApiController
	{
		private readonly SkillGroupViewModelBuilder _skillGroupViewModelBuilder;

		public RtaSkillAreaController(SkillGroupViewModelBuilder skillGroupViewModelBuilder)
		{
			_skillGroupViewModelBuilder = skillGroupViewModelBuilder;
		}

		[UnitOfWork, HttpGet, Route("api/SkillGroups")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_skillGroupViewModelBuilder.GetAll());
		}

		[UnitOfWork, HttpGet, Route("api/SkillArea/For")]
		public virtual IHttpActionResult NameFor(Guid skillAreaId)
		{
			return Ok(_skillGroupViewModelBuilder.Get(skillAreaId));
		}
	}
}