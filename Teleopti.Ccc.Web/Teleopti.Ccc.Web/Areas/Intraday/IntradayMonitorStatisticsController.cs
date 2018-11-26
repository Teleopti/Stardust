using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayMonitorStatisticsController : ApiController
	{
		private readonly IIntradaySkillProvider _intradaySkillProvider;
		private readonly IIntradayIncomingTrafficApplicationService _intradayIncomingTrafficApplicationService;

		public IntradayMonitorStatisticsController(
			IIntradaySkillProvider intradaySkillProvider,
			IIntradayIncomingTrafficApplicationService intradayIncomingTrafficApplicationService)
		{
			_intradaySkillProvider = intradaySkillProvider;
			_intradayIncomingTrafficApplicationService = intradayIncomingTrafficApplicationService ?? throw new ArgumentNullException(nameof(intradayIncomingTrafficApplicationService));
		}

		[UnitOfWorkAttribute, HttpGetAttribute, Route("api/intraday/monitorskillareastatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStatistics(Guid id)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillStatistics(Guid Id)
		{
			return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(new[] { Id }));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastatistics/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaStatisticsByDayOffset(Guid id, int dayOffset)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillStatisticsByDayOffset(Guid id, int dayOffset)
		{
			return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(new[] { id }, dayOffset));
		}
	}
}