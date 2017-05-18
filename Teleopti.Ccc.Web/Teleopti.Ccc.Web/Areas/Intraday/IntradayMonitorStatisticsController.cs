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

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastatistics/{id}/{dateUtc}")]
		public virtual IHttpActionResult MonitorSkillAreaStatisticsByDate(Guid id, DateTime dateUtc)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_incomingTrafficViewModelCreator.Load(skillIdList, dateUtc));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}/{dateUtc}")]
		public virtual IHttpActionResult MonitorSkillStatisticsByDate(Guid id, DateTime dateUtc)
		{
			return Ok(_incomingTrafficViewModelCreator.Load(new[] { id }, dateUtc));
		}
	}
}