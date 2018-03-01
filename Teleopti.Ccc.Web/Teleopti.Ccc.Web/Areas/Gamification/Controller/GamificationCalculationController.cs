using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Gamification.Controller
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)]
	public class GamificationCalculationController: ApiController
	{
		private readonly CalculateBadges _calculateBadges;
		private readonly IPerformBadgeCalculation _performBadgeCalculation;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public GamificationCalculationController( CalculateBadges calculateBadges, IPerformBadgeCalculation performBadgeCalculation, ICurrentBusinessUnit currentBusinessUnit)
		{
			_calculateBadges = calculateBadges;
			_performBadgeCalculation = performBadgeCalculation;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[HttpPost, Route("api/GamificationCalculation/Reset"), UnitOfWork]
		public virtual bool ResetBadge()
		{
			return _calculateBadges.ResetBadge();
		}

		[HttpPost, Route("api/GamificationCalculation/RecalculateBadges"), UnitOfWork]
		public virtual void RecalculateBadges(DateOnlyPeriod period)
		{
			_calculateBadges.RemoveAgentBadges(period);
			foreach (var date in period.DayCollection())
			{
				_performBadgeCalculation.Calculate(_currentBusinessUnit.Current().Id.GetValueOrDefault(), date.Date);
			}
		}
	}
}