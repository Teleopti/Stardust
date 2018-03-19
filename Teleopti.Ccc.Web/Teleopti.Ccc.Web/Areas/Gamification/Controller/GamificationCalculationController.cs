using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.ApplicationLayer.Gamification;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Gamification.Models;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Gamification.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)]
	public class GamificationCalculationController: ApiController
	{
		private readonly CalculateBadges _calculateBadges;
		private readonly IRecalculateBadgeJobService _recalculateBadgeJobService;

		public GamificationCalculationController( CalculateBadges calculateBadges, IRecalculateBadgeJobService recalculateBadgeJobService)
		{
			_calculateBadges = calculateBadges;
			_recalculateBadgeJobService = recalculateBadgeJobService;
		}

		[HttpPost, Route("api/GamificationCalculation/Reset"), UnitOfWork]
		public virtual bool ResetBadge()
		{
			return _calculateBadges.ResetBadge();
		}

		[HttpPost, Route("api/Gamification/RecalculateBadges/NewRecalculateBadgeJob"), UnitOfWork]
		public virtual IHttpActionResult NewRecalculateBadgeJob([FromBody]RecalculateForm input)
		{
			_recalculateBadgeJobService.CreateJob(input.Start, input.End);
			return Ok();
		}

		[Route("api/gamification/RecalcualteBadges/GetJobList"), HttpGet, UnitOfWork]
		public virtual IList<RecalculateBadgeJobResultDetail> GetJobList()
		{
			return _recalculateBadgeJobService.GetJobsForCurrentBusinessUnit();
		}
	}
}