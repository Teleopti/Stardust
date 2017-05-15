using System;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
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

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillareastatistics/{id}/{jsDateLocal}")]
		public virtual IHttpActionResult MonitorSkillAreaStatisticsByDate(Guid id, string jsDateLocal)
		{
			//Not implemented!
			var datetime = fromJsonDate(jsDateLocal);
			var utcDate = datetime.ToUniversalTime();
			var skillArea = _skillAreaRepository.Get(id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_incomingTrafficViewModelCreator.Load(skillIdList));
		}

		[UnitOfWork, HttpGet, Route("api/intraday/monitorskillstatistics/{id}/{jsDateLocal}")]
		public virtual IHttpActionResult MonitorSkillStatisticsByDate(Guid id, string jsDateLocal)
		{
			var datetime = fromJsonDate(jsDateLocal);
			var utcDate = datetime.ToUniversalTime();
			return Ok(_incomingTrafficViewModelCreator.Load(new[] { id }, utcDate));
		}


		private DateTime fromJsonDate(string jsDate)
		{
			var epoch = new DateTime(1970,1,1,0,0,0, DateTimeKind.Utc);
			long d;
			if(long.TryParse(jsDate, out d))
			{
				return epoch.AddMilliseconds(d);
			}
			return DateTime.MinValue;
		}
	}
}