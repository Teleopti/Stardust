using System;
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
		private readonly INow _now;
		private readonly IConfigReader _configReader;
		private static readonly ILog logger = LogManager.GetLogger(typeof(StaffingLevelController));
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public StaffingLevelController(IEventPublisher publisher, INow now, IConfigReader configReader,
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
			IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_publisher = publisher;
			_now = now;
			_configReader = configReader;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
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
				EndDateTime = now.AddHours(24),
				RequestedFromWeb = true
			});

			return Ok();
		}

		[UnitOfWork, HttpGet, Route("GetLastCaluclatedDateTime")]
		public virtual IHttpActionResult GetLastCaluclatedDateTime()
		{
			var buid = ((TeleoptiIdentity) _currentTeleoptiPrincipal.Current().Identity).BusinessUnit.Id.GetValueOrDefault();
			return Json(getLastCaluclatedDateTime(buid));
		}

		private DateTime getLastCaluclatedDateTime(Guid buId)
		{
			var lastCalculated = _scheduleForecastSkillReadModelRepository.GetLastCalculatedTime();
			return lastCalculated.ContainsKey(buId) ? lastCalculated[buId] : DateTime.MinValue;
		}
	}
}
