using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
    public class CalculateController : ApiController
    {
		private readonly CalculateForReadModel _calculateForReadModel;
		public CalculateController(CalculateForReadModel calculateForReadModel)
		{
			_calculateForReadModel = calculateForReadModel;
		}

		[HttpGet, Route("ResourceCalculate")]
		public virtual IHttpActionResult ResourceCalculate(DateTime date)
		{
			//var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			var result = _calculateForReadModel.ResourceCalculatePeriod(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date.AddDays(1))));

			return Json(result);
		}

	}
}
