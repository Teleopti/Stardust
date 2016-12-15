using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class RtaSkillAreaController : ApiController
	{
		private readonly FetchSkillArea _fetchSkillArea;

		public RtaSkillAreaController(FetchSkillArea fetchSkillArea)
		{
			_fetchSkillArea = fetchSkillArea;
		}

		[UnitOfWork, HttpGet, Route("api/SkillAreas")]
		public virtual IHttpActionResult GetSkillAreas()
		{
			return Ok(new SkillAreaInfo
			{
				SkillAreas = _fetchSkillArea.GetAll()
			});
		}

		[UnitOfWork, HttpGet, Route("api/SkillArea/For")]
		public virtual IHttpActionResult NameFor(Guid skillAreaId)
		{
			return Ok(_fetchSkillArea.Get(skillAreaId));
		}
	}

	public class SkillAreaInfo
	{
		public IEnumerable<SkillAreaViewModel> SkillAreas { get; set; }
	}

	public class SkillAreaInput
	{
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}