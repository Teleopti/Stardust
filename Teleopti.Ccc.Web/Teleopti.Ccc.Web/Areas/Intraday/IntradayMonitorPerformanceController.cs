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
	public class IntradayMonitorPerformanceController : ApiController
	{
		private readonly IIntradaySkillProvider _intradaySkillProvider;
		private readonly PerformanceViewModelCreator _performanceViewModelCreator;
		private readonly IIntradayPerformanceApplicationService _performanceApplicationService;
		private readonly IToggleManager _toggleManager;

		public IntradayMonitorPerformanceController(
			IIntradaySkillProvider intradaySkillProvider, 
			PerformanceViewModelCreator performanceViewModelCreator,
			IIntradayPerformanceApplicationService performanceApplicationService,
			IToggleManager toggleManager)
		{
			_intradaySkillProvider = intradaySkillProvider;
			_performanceViewModelCreator = performanceViewModelCreator;
			_toggleManager = toggleManager ?? throw new ArgumentNullException(nameof(toggleManager));
			_performanceApplicationService = performanceApplicationService ?? throw new ArgumentNullException(nameof(performanceApplicationService));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareaperformance/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaPerformance(Guid id)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_performanceApplicationService.GeneratePerformanceViewModel(skillIdList));
			else
				return Ok(_performanceViewModelCreator.Load_old(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareaperformance/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaPerformanceByDayOffset(Guid id, int dayOffset)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_performanceApplicationService.GeneratePerformanceViewModel(skillIdList, dayOffset));
			else
				return Ok(_performanceViewModelCreator.Load_old(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillperformance/{id}")]
		public virtual IHttpActionResult MonitorSkillPerformance(Guid id)
		{
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_performanceApplicationService.GeneratePerformanceViewModel(new[] { id }));
			else
				return Ok(_performanceViewModelCreator.Load_old(new[] { id }));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillperformance/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillPerformanceByDayOffset(Guid id, int dayOffset)
		{
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_performanceApplicationService.GeneratePerformanceViewModel(new[] { id }, dayOffset));
			else
				return Ok(_performanceViewModelCreator.Load_old(new[] { id }, dayOffset));
		}
	}
}