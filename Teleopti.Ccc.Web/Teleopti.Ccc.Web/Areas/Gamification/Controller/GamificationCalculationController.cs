using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Gamification.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)]
	public class GamificationCalculationController: ApiController
	{
		private readonly CalculateBadges _calculateBadges;

		public GamificationCalculationController( CalculateBadges calculateBadges)
		{
			_calculateBadges = calculateBadges;
		}

		[HttpPost, Route("api/GamificationCalculation/Reset"), UnitOfWork]
		public virtual bool ResetBadge()
		{
			return _calculateBadges.ResetBadge();
		}
	}
}