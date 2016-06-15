using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
    public class CalculateController : ApiController
    {
		private readonly CalculateForReadModel _calculateForReadModel;
	   private IEventPublisher _publisher;
	    private ILoggedOnUser _loggedOnUser;

	    public CalculateController(CalculateForReadModel calculateForReadModel, IEventPublisher publisher, ILoggedOnUser loggedOnUser)
	    {
		    _calculateForReadModel = calculateForReadModel;
		    _publisher = publisher;
		    _loggedOnUser = loggedOnUser;
	    }

	    [UnitOfWork, HttpGet, Route("ResourceCalculate")]
		public virtual IHttpActionResult ResourceCalculate(DateTime date)
		{
			//var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			var result = _calculateForReadModel.ResourceCalculatePeriod(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date.AddDays(1))));

			return Json(result);
		}

		[UnitOfWork, HttpGet, Route("TriggerResourceCalculate")]
		public virtual IHttpActionResult TriggerResourceCalculate()
		{
			_publisher.Publish(new UpdateResourceCalculateReadModelEvent() {
				InitiatorId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault(),
				JobName = "Resource Calculate",
				UserName = _loggedOnUser.CurrentUser().Id.GetValueOrDefault().ToString()
			});
			return Ok();
		}

	}
}
