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
	public class IntradayMonitorStatisticsController : ApiController
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly IncomingTrafficViewModelCreator _incomingTrafficViewModelCreator;

		public IntradayMonitorStatisticsController(ISkillAreaRepository skillAreaRepository, IncomingTrafficViewModelCreator incomingTrafficViewModelCreator)
		{
			_skillAreaRepository = skillAreaRepository;
			_incomingTrafficViewModelCreator = incomingTrafficViewModelCreator;
		}

		[UnitOfWorkAttribute, HttpGetAttribute, Route("api/intraday/monitorskillareastatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStatistics(Guid id)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_incomingTrafficViewModelCreator.Load(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillStatistics(Guid Id)
		{
			return Ok(_incomingTrafficViewModelCreator.Load(new[] { Id }));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastatistics/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillAreaStatisticsByDayOffset(Guid id, int dayOffset)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_incomingTrafficViewModelCreator.Load(skillIdList, dayOffset));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}/{dayOffset}")]
		public virtual IHttpActionResult MonitorSkillStatisticsByDayOffset(Guid id, int dayOffset)
		{
			return Ok(_incomingTrafficViewModelCreator.Load(new[] { id }, dayOffset));
		}
	}
}