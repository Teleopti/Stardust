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
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;

		public IntradaySkillController(FetchSkillInIntraday fetchSkillInIntraday, MonitorSkillsProvider monitorSkillsProvider, ForecastedStaffingProvider forecastedStaffingProvider)
		{
			_fetchSkillInIntraday = fetchSkillInIntraday;
			_monitorSkillsProvider = monitorSkillsProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skills")]
		public virtual IHttpActionResult GetAllSkills()
		{
			return Ok(_fetchSkillInIntraday.GetAll().Select(x => new { x.Id, x.Name }).ToArray());
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillStatistics(Guid Id)
		{
			return Ok(_monitorSkillsProvider.Load(new[] { Id }));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillStaffing(Guid Id)
		{
			return Ok(_forecastedStaffingProvider.Load(new[] { Id }));
		}
	}
}