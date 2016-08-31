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
	public class IntradayMonitorStaffingController : ApiController
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly ForecastedStaffingViewModelCreator _forecastedStaffingViewModelCreator;

		public IntradayMonitorStaffingController(ISkillAreaRepository skillAreaRepository, ForecastedStaffingViewModelCreator forecastedStaffingViewModelCreator)
		{
			_skillAreaRepository = skillAreaRepository;
			_forecastedStaffingViewModelCreator = forecastedStaffingViewModelCreator;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStaffing(Guid Id)
		{
			var skillArea = _skillAreaRepository.Get(Id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_forecastedStaffingViewModelCreator.Load(skillIdList));
		}
		
		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillStaffing(Guid Id)
		{
			return Ok(_forecastedStaffingViewModelCreator.Load(new[] { Id }));
		}
	}
}