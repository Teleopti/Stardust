﻿using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayController : ApiController
	{
		private readonly LatestStatisticsTimeProvider _latestStatisticsTimeProvider;
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly FetchSkillInIntraday _fetchSkillInIntraday;

		public IntradayController(
			LatestStatisticsTimeProvider latestStatisticsTimeProvider,
			ISkillAreaRepository skillAreaRepository,
			FetchSkillInIntraday fetchSkillInIntraday)
		{
			_latestStatisticsTimeProvider = latestStatisticsTimeProvider;
			_skillAreaRepository = skillAreaRepository;
			_fetchSkillInIntraday = fetchSkillInIntraday;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/lateststatisticstimeforskillarea/{id}")]
		public virtual IHttpActionResult GetLatestStatisticsTimeForSkillArea(Guid id)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(new { latestIntervalTime = _latestStatisticsTimeProvider.Get(skillIdList) });
		}
	}
}