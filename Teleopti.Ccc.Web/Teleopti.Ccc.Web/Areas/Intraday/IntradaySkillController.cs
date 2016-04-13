using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradaySkillController : ApiController
	{
		private readonly FetchSkillInIntraday _fetchSkillInIntraday;
		private readonly MonitorSkillsProvider _monitorSkillsProvider;

		public IntradaySkillController(FetchSkillInIntraday fetchSkillInIntraday, MonitorSkillsProvider monitorSkillsProvider)
		{
			_fetchSkillInIntraday = fetchSkillInIntraday;
            _monitorSkillsProvider = monitorSkillsProvider;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skills")]
		public virtual IHttpActionResult GetAllSkills()
		{
			return Ok(_fetchSkillInIntraday.GetAll().Select(x => new {x.Id, x.Name}).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskill/{id}")]
		public virtual IHttpActionResult MonitorSkill(Guid Id)
		{
			return Ok(_monitorSkillsProvider.Load(new[] { Id }));
		}
	}
}