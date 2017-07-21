﻿using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayMonitorStaffingController : ApiController
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;

		public IntradayMonitorStaffingController(ISkillAreaRepository skillAreaRepository, IStaffingViewModelCreator staffingViewModelCreator)
		{
			_skillAreaRepository = skillAreaRepository;
			_staffingViewModelCreator = staffingViewModelCreator;
		}


		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStaffing(Guid id)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_staffingViewModelCreator.Load(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{id}")]
		public virtual IHttpActionResult MonitorSkillStaffing(Guid id)
		{
			return Ok(_staffingViewModelCreator.Load(new[] { id }));
		}


		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing/{skillAreaId}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaStaffingByDayOffset(Guid skillAreaId, int dayOffset)
		{
			var skillArea = _skillAreaRepository.Get(skillAreaId);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_staffingViewModelCreator.Load(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing/{skillId}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillStaffingByDayOffset(Guid skillId, int dayOffset)
		{
			return Ok(_staffingViewModelCreator.Load(new[] { skillId }, dayOffset));
		}
	}

}