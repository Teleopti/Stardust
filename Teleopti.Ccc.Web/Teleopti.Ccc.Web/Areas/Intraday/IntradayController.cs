using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	public class IntradayController : ApiController
	{
		private readonly LatestStatisticsTimeProvider _latestStatisticsTimeProvider;

		public IntradayController(LatestStatisticsTimeProvider latestStatisticsTimeProvider)
		{
			_latestStatisticsTimeProvider = latestStatisticsTimeProvider;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skills")]
		public virtual IHttpActionResult GetAllSkills()
		{
			return Ok(_latestStatisticsTimeProvider.Get());
		}
	}
}