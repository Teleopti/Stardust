using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.Historical.AdjustAdherence;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.AdjustAdherence)]
	public class AdjustAdherenceController : ApiController
	{
		[UnitOfWork,HttpPost, Route("api/Adherence/AdjustPeriod")]
		public virtual IHttpActionResult AdjustPeriod([FromBody] AdjustAdherenceToNeutralCommand command)
		{
			//call command handler
			return Ok();
		}	
		
		[UnitOfWork, HttpGet, Route("api/Adherence/AdjustedPeriods")]
		public virtual IHttpActionResult Load()
		{
			return Ok(new[] {new AdjustedPeriod() {StartTime = "2019-01-28 12:00", EndTime = "2019-01-28 14:00"}});
		}					
	}

	public class AdjustedPeriod
	{
		public string StartTime;
		public string EndTime;
	}
}