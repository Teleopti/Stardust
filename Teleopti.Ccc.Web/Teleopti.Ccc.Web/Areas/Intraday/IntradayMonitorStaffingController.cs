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
	public class IntradayMonitorStaffingController : ApiController
	{
		private readonly IIntradaySkillProvider _intradaySkillProvider;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IIntradayStaffingApplicationService _intradayStaffingApplicationService;
		private readonly IToggleManager _toggleManager;

		public IntradayMonitorStaffingController(
			IIntradaySkillProvider intradaySkillProvider, 
			IStaffingViewModelCreator staffingViewModelCreator,
			IIntradayStaffingApplicationService intradayStaffingApplicationService,
			IToggleManager toggleManager)
		{
			_intradaySkillProvider = intradaySkillProvider;
			_staffingViewModelCreator = staffingViewModelCreator;
			_intradayStaffingApplicationService = intradayStaffingApplicationService ?? throw new ArgumentNullException(nameof(intradayStaffingApplicationService));
			_toggleManager = toggleManager ?? throw new ArgumentNullException(nameof(toggleManager));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStaffing(Guid id)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(skillIdList));
			else
				return Ok(_staffingViewModelCreator.Load_old(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillStaffing(Guid id)
		{
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(new[] { id }));
			else
				return Ok(_staffingViewModelCreator.Load_old(new[] { id }));
		}
		
		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing/{skillAreaId}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaStaffingByDayOffset(Guid skillAreaId, int dayOffset)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(skillAreaId);
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(skillIdList, dayOffset));
			else
				return Ok(_staffingViewModelCreator.Load_old(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{skillId}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillStaffingByDayOffset(Guid skillId, int dayOffset)
		{
			if (_toggleManager.IsEnabled(Toggles.WFM_Intraday_Refactoring_74652))
				return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(new[] { skillId }, dayOffset));
			else
				return Ok(_staffingViewModelCreator.Load_old(new[] { skillId }, dayOffset));
		}
	}
}