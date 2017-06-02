using System;
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
	public class IntradayMonitorPerformanceController : ApiController
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly PerformanceViewModelCreator _performanceViewModelCreator;

		public IntradayMonitorPerformanceController(ISkillAreaRepository skillAreaRepository,
			PerformanceViewModelCreator performanceViewModelCreator)
		{
			_skillAreaRepository = skillAreaRepository;
			_performanceViewModelCreator = performanceViewModelCreator;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareaperformance/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaPerformance(Guid id)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_performanceViewModelCreator.Load(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareaperformance/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaPerformanceByDayOffset(Guid id, int dayOffset)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_performanceViewModelCreator.Load(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillperformance/{id}")]
		public virtual IHttpActionResult MonitorSkillPerformance(Guid Id)
		{
			return Ok(_performanceViewModelCreator.Load(new Guid[] {Id}));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillperformance/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillPerformanceByDayOffset(Guid id, int dayOffset)
		{
			return Ok(_performanceViewModelCreator.Load(new[] { id }, dayOffset));
		}
	}
}