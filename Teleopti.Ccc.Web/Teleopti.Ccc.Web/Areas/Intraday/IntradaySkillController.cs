using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradaySkillController : ApiController
	{
		private readonly FetchSkillInIntraday _fetchSkillInIntraday;
		private readonly IIntradayMonitorDataLoader _intradayMonitorDataLoader;

		public IntradaySkillController(FetchSkillInIntraday fetchSkillInIntraday, IIntradayMonitorDataLoader intradayMonitorDataLoader)
		{
			_fetchSkillInIntraday = fetchSkillInIntraday;
			_intradayMonitorDataLoader = intradayMonitorDataLoader;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skills")]
		public virtual IHttpActionResult GetAllSkills()
		{
			return Ok(_fetchSkillInIntraday.GetAll().Select(x => new {x.Id, x.Name}).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskill/{id}")]
		public virtual IHttpActionResult MonitorSkill(Guid Id)
		{
			return Ok(_intradayMonitorDataLoader.Load(new[] { Id }, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone, DateOnly.Today));
		}
	}
}