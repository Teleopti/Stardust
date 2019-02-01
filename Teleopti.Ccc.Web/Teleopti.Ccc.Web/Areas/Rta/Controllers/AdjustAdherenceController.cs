using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.AdjustAdherence;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.AdjustAdherence)]
	public class AdjustAdherenceController : ApiController
	{
		private readonly AdjustAdherenceToNeutralCommandHandler _adjustAdherenceToNeutralCommandHandler;
		private readonly AdjustedPeriodsViewModelBuilder _adjustedPeriodsViewModelBuilder;

		public AdjustAdherenceController(
			AdjustAdherenceToNeutralCommandHandler adjustAdherenceToNeutralCommandHandler, 
			AdjustedPeriodsViewModelBuilder adjustedPeriodsViewModelBuilder)
		{
			_adjustAdherenceToNeutralCommandHandler = adjustAdherenceToNeutralCommandHandler;
			_adjustedPeriodsViewModelBuilder = adjustedPeriodsViewModelBuilder;
		}
		
		[UnitOfWork,HttpPost, Route("api/Adherence/AdjustPeriod")]
		public virtual IHttpActionResult AdjustPeriod([FromBody] AdjustAdherenceToNeutralCommand command)
		{
			_adjustAdherenceToNeutralCommandHandler.Handle(command);
			return Ok();
		}	
		
		[UnitOfWork, HttpGet, Route("api/Adherence/AdjustedPeriods")]
		public virtual IHttpActionResult Load() => Ok(_adjustedPeriodsViewModelBuilder.Build());
	}
}