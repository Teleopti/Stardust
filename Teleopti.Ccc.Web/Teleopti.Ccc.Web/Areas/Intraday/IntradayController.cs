using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayController : IntradayControllerBase
	{
		private readonly LatestStatisticsTimeProvider _latestStatisticsTimeProvider;

		public IntradayController(LatestStatisticsTimeProvider latestStatisticsTimeProvider,ISkillAreaRepository skillAreaRepository) : base(skillAreaRepository)
		{
			_latestStatisticsTimeProvider = latestStatisticsTimeProvider;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/lateststatisticstimeforskillarea/{id}")]
		public virtual IHttpActionResult GetLatestStatisticsTimeForSkillArea(Guid id)
		{
			var skillIdList = GetSkillsFromSkillArea(id);
			return Ok(new { latestIntervalTime = _latestStatisticsTimeProvider.Get(skillIdList) });
		}

		[UnitOfWork, HttpGet, Route("api/intraday/lateststatisticstimeforskill/{id}")]
		public virtual IHttpActionResult GetLatestStatisticsTimeForSkill(Guid id)
		{
			return Ok(_latestStatisticsTimeProvider.Get(new[] { id }));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/lateststatisticstimeforskillarea/{id}/{dayOffset}")]
		public virtual IHttpActionResult GetLatestStatisticsTimeForSkillAreaAndDate(Guid id, int dayOffset)
		{
			var skillIdList = GetSkillsFromSkillArea(id);
			return Ok(new { latestIntervalTime = _latestStatisticsTimeProvider.Get(skillIdList, dayOffset) });
		}

		[UnitOfWork, HttpGet, Route("api/intraday/lateststatisticstimeforskill/{id}/{dayOffset}")]
		public virtual IHttpActionResult GetLatestStatisticsTimeForSkillByDayOffset(Guid id, int dayOffset)
		{
			return Ok(_latestStatisticsTimeProvider.Get(new[] { id }, dayOffset));
		}
	}
}