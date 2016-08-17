using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradayMonitorStatisticsController : ApiController
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly MonitorSkillsProvider _monitorSkillsProvider;

		public IntradayMonitorStatisticsController(ISkillAreaRepository skillAreaRepository, MonitorSkillsProvider monitorSkillsProvider)
		{
			_skillAreaRepository = skillAreaRepository;
			_monitorSkillsProvider = monitorSkillsProvider;
		}

		[UnitOfWorkAttribute, HttpGetAttribute, Route("api/intraday/monitorskillareastatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillAreaStatistics(Guid id)
		{
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_monitorSkillsProvider.Load(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}")]
		public virtual IHttpActionResult MonitorSkillStatistics(Guid Id)
		{
			return Ok(_monitorSkillsProvider.Load(new[] { Id }));
		}

	}
}