using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.Adjustment;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.AdjustAdherence)]
	public class AdjustAdherenceController : ApiController
	{
		private readonly AdjustPeriodToNeutralCommandHandler _adjustPeriodToNeutralCommandHandler;
		private readonly AdjustedPeriodsViewModelBuilder _adjustedPeriodsViewModelBuilder;

		public AdjustAdherenceController(
			AdjustPeriodToNeutralCommandHandler adjustPeriodToNeutralCommandHandler, 
			AdjustedPeriodsViewModelBuilder adjustedPeriodsViewModelBuilder)
		{
			_adjustPeriodToNeutralCommandHandler = adjustPeriodToNeutralCommandHandler;
			_adjustedPeriodsViewModelBuilder = adjustedPeriodsViewModelBuilder;
		}
		
		[UnitOfWork,HttpPost, Route("api/Adherence/AdjustPeriod")]
		public virtual IHttpActionResult AdjustPeriod([FromBody] AdjustPeriodToNeutralCommand command)
		{
			_adjustPeriodToNeutralCommandHandler.Handle(command);
			return Ok();
		}	
		
		[UnitOfWork, HttpGet, Route("api/Adherence/AdjustedPeriods")]
		public virtual IHttpActionResult Load() => Ok(_adjustedPeriodsViewModelBuilder.Build());
		
		[UnitOfWork,HttpPost, Route("api/Adherence/RemoveAdjustedPeriod")]
		public virtual IHttpActionResult RemoveAdjustedPeriod([FromBody] CancelAdjustmentToNeutralCommand command)
		{
			return Ok();
		}	
	}
}