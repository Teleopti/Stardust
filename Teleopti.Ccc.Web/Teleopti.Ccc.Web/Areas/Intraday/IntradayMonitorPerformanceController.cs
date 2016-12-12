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
		private readonly MonitorPerformanceProvider _monitorPerformanceProvider;

		public IntradayMonitorPerformanceController(ISkillAreaRepository skillAreaRepository,
			MonitorPerformanceProvider monitorPerformanceProvider)
		{
			_skillAreaRepository = skillAreaRepository;
			_monitorPerformanceProvider = monitorPerformanceProvider;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareaperformance/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaPerformance(Guid id)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_monitorPerformanceProvider.Load(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillperformance/{id}")]
		public virtual IHttpActionResult MonitorSkillPerformance(Guid Id)
		{
			return Ok(_monitorPerformanceProvider.Load(new Guid[] {Id}));
		}
	}
}