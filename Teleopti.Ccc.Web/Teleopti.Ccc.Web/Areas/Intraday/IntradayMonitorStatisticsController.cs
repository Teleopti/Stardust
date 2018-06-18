using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayMonitorStatisticsController : ApiController
	{
		private readonly IIntradaySkillProvider _intradaySkillProvider;
		private readonly IncomingTrafficViewModelCreator _incomingTrafficViewModelCreator;
		private readonly IIntradayIncomingTrafficApplicationService _intradayIncomingTrafficApplicationService;
		private readonly IToggleManager _toggleManager;

		public IntradayMonitorStatisticsController(
			IIntradaySkillProvider intradaySkillProvider,
			IncomingTrafficViewModelCreator incomingTrafficViewModelCreator,
			IIntradayIncomingTrafficApplicationService intradayIncomingTrafficApplicationService,
			IToggleManager toggleManager)
		{
			_intradaySkillProvider = intradaySkillProvider;
			_incomingTrafficViewModelCreator = incomingTrafficViewModelCreator;
			_intradayIncomingTrafficApplicationService = intradayIncomingTrafficApplicationService ?? throw new ArgumentNullException(nameof(intradayIncomingTrafficApplicationService));
			_toggleManager = toggleManager ?? throw new ArgumentNullException(nameof(toggleManager));
		}

		[UnitOfWorkAttribute, HttpGetAttribute, Route("api/intraday/monitorskillareastatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStatistics(Guid id)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(skillIdList));
			else
				return Ok(_incomingTrafficViewModelCreator.Load_old(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillStatistics(Guid Id)
		{
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(new[] { Id }));
			else
				return Ok(_incomingTrafficViewModelCreator.Load_old(new[] { Id }));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastatistics/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaStatisticsByDayOffset(Guid id, int dayOffset)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(skillIdList, dayOffset));
			else
				return Ok(_incomingTrafficViewModelCreator.Load_old(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillStatisticsByDayOffset(Guid id, int dayOffset)
		{
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(new[] { id }, dayOffset));
			else
				return Ok(_incomingTrafficViewModelCreator.Load_old(new[] { id }, dayOffset));
		}
	}
}