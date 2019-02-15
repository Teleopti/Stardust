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
	public class IntradayMonitorStaffingController : ApiController
	{
		private readonly IIntradaySkillProvider _intradaySkillProvider;
		private readonly IIntradayApplicationStaffingService _intradayStaffingApplicationService;

		public IntradayMonitorStaffingController(
			IIntradaySkillProvider intradaySkillProvider,
			IIntradayApplicationStaffingService intradayStaffingApplicationService)
		{
			_intradaySkillProvider = intradaySkillProvider;
			_intradayStaffingApplicationService = intradayStaffingApplicationService ?? throw new ArgumentNullException(nameof(intradayStaffingApplicationService));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStaffing(Guid id)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(id);
			return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillStaffing(Guid id)
		{
			return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(new[] { id }));
		}
		
		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing/{skillAreaId}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaStaffingByDayOffset(Guid skillAreaId, int dayOffset)
		{
			var skillIdList = _intradaySkillProvider.GetSkillsFromSkillGroup(skillAreaId);
			return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{skillId}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillStaffingByDayOffset(Guid skillId, int dayOffset)
		{
			return Ok(_intradayStaffingApplicationService.GenerateStaffingViewModel(new[] { skillId }, dayOffset));
		}
	}
}