using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class PlanningPeriodController : ApiController
	{
		private readonly IRepository<IPlanningPeriod> _planningPeriodRepository;
		private readonly INow _now;

		public PlanningPeriodController(IRepository<IPlanningPeriod> planningPeriodRepository, INow now)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_now = now;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetPlanningPeriod()
		{
			var foundPlanningPeriods = _planningPeriodRepository.LoadAll();

			var result = foundPlanningPeriods.FirstOrDefault();
			if (planningPeriodNotFound(result))
			{
				result = new PlanningPeriod(_now);
				_planningPeriodRepository.Add(result);
			}
			
			return
				Ok(new PlanningPeriodModel
				{
					StartDate = result.Range.StartDate.Date,
					EndDate = result.Range.EndDate.Date,
					Id = result.Id.GetValueOrDefault()
				});
		}

		private static bool planningPeriodNotFound(IPlanningPeriod result)
		{
			return result == null;
		}
	}
}