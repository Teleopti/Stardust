using System;
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

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastaffing")]
		public virtual IHttpActionResult MonitorSkillAreaStaffingByDate(Guid SkillAreaId, DateTime DateTime, bool UseShrinkage)
		{
			var skillArea = _skillAreaRepository.Get(SkillAreaId);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_staffingViewModelCreator.Load(skillIdList, new DateOnly(DateTime), UseShrinkage));
			//return Ok(_staffingViewModelCreator.Load(skillIdList));
		}
		
		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstaffing")]
		public virtual IHttpActionResult MonitorSkillStaffingByDate(Guid SkillId, DateTime DateTime, bool UseShrinkage)
		{
			return Ok(_staffingViewModelCreator.Load(new[] {SkillId }, new DateOnly(DateTime), UseShrinkage));
			//return Ok(_staffingViewModelCreator.Load(new[] { SkillId }));
		}
	}

}