﻿using System;
using System.Globalization;
using System.Web.Http;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class StaffingLevelController : ApiController
	{
		private readonly IEventPublisher _publisher;
		private readonly IScheduleForecastSkillProvider _scheduleForecastSkillProvider;
		private readonly INow _now;
		private readonly IConfigReader _configReader;
		private static readonly ILog logger = LogManager.GetLogger(typeof(StaffingLevelController));
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public StaffingLevelController(IEventPublisher publisher,
			IScheduleForecastSkillProvider scheduleForecastSkillProvider, INow now, IConfigReader configReader, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_publisher = publisher;
			_scheduleForecastSkillProvider = scheduleForecastSkillProvider;
			_now = now;
			_configReader = configReader;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		[UnitOfWork, HttpGet, Route("ForecastAndStaffingForSkill")]
		public virtual IHttpActionResult ForecastAndStaffingForSkill(DateTime date, Guid skillId)
		{
			var intervals = _scheduleForecastSkillProvider.GetBySkill(skillId, date);
			return Json(intervals);
		}

		[UnitOfWork, HttpGet, Route("ForecastAndStaffingForSkillArea")]
		public virtual IHttpActionResult ForecastAndStaffingForSkillArea(DateTime date, Guid skillAreaId)
		{
			var intervals = _scheduleForecastSkillProvider.GetBySkillArea(skillAreaId, date);
			return Json(intervals);
		}

		[UnitOfWork, HttpGet, Route("TriggerResourceCalculate")]
		public virtual IHttpActionResult TriggerResourceCalculate()
		{
			var now = _now.UtcDateTime();
			var configuredNow = _configReader.AppConfig("FakeIntradayUtcStartDateTime");
			if (configuredNow != null)
			{
				try
				{
					now = DateTime.ParseExact(configuredNow, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture).Utc();
				}
				catch
				{
					logger.Warn("The app setting 'FakeIntradayStartDateTime' is not specified correctly. Format your datetime as 'yyyy-MM-dd HH:mm' ");
				}
			}

			_publisher.Publish(new UpdateStaffingLevelReadModelEvent()
			{
				StartDateTime = now.AddHours(-24),
				EndDateTime = now.AddHours(24)
			});

			return Ok();
		}

		[UnitOfWork, HttpGet, Route("GetLastCaluclatedDateTime")]
		public virtual IHttpActionResult GetLastCaluclatedDateTime()
		{
			var buid = ((TeleoptiIdentity) _currentTeleoptiPrincipal.Current().Identity).BusinessUnit.Id.GetValueOrDefault();
			return Json(_scheduleForecastSkillProvider.GetLastCaluclatedDateTime(buid));
		}
	}
}
