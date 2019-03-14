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
		private readonly AdjustedPeriodsViewModelBuilder _adjustedPeriodsViewModelBuilder;
		private readonly AdjustPeriodToNeutralCommandHandler _adjustPeriodToNeutralCommandHandler;
		private readonly CancelAdjustmentToNeutralCommandHandler _cancelAdjustmentToNeutralCommandHandler;

		public AdjustAdherenceController(
			AdjustedPeriodsViewModelBuilder adjustedPeriodsViewModelBuilder,
			AdjustPeriodToNeutralCommandHandler adjustPeriodToNeutralCommandHandler,
			CancelAdjustmentToNeutralCommandHandler cancelAdjustmentToNeutralCommandHandler)
		{
			_adjustPeriodToNeutralCommandHandler = adjustPeriodToNeutralCommandHandler;
			_cancelAdjustmentToNeutralCommandHandler = cancelAdjustmentToNeutralCommandHandler;
			_adjustedPeriodsViewModelBuilder = adjustedPeriodsViewModelBuilder;
		}

		[UnitOfWork, HttpGet, Route("api/Adherence/AdjustedPeriods")]
		public virtual IHttpActionResult Load() => Ok(_adjustedPeriodsViewModelBuilder.Build());

		[UnitOfWork, HttpPost, Route("api/Adherence/AdjustPeriod")]
		public virtual IHttpActionResult AdjustPeriod([FromBody] AdjustPeriodToNeutralCommand command)
		{
			_adjustPeriodToNeutralCommandHandler.Handle(command);
			return Ok();
		}

		[UnitOfWork, HttpPost, Route("api/Adherence/CancelAdjustedPeriod")]
		public virtual IHttpActionResult CancelAdjustedPeriod([FromBody] CancelAdjustmentToNeutralCommand command)
		{
			_cancelAdjustmentToNeutralCommandHandler.Handle(command);
			return Ok();
		}
	}
}