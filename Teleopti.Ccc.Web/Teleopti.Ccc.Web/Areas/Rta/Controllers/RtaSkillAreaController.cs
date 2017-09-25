using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SkillGroup;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class RtaSkillAreaController : ApiController
	{
		private readonly FetchSkillGroup _fetchSkillGroup;

		public RtaSkillAreaController(FetchSkillGroup fetchSkillGroup)
		{
			_fetchSkillGroup = fetchSkillGroup;
		}

		[UnitOfWork, HttpGet, Route("api/SkillAreas")]
		public virtual IHttpActionResult GetSkillAreas()
		{
			return Ok(new SkillAreaInfo
			{
				SkillAreas = _fetchSkillGroup.GetAll()
			});
		}

		[UnitOfWork, HttpGet, Route("api/SkillArea/For")]
		public virtual IHttpActionResult NameFor(Guid skillAreaId)
		{
			return Ok(_fetchSkillGroup.Get(skillAreaId));
		}
	}

	public class SkillAreaInfo
	{
		public IEnumerable<SkillGroupViewModel> SkillAreas { get; set; }
	}

	public class SkillAreaInput
	{
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}