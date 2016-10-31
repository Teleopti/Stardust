using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class AnalyticsPersonPeriodDateFixer : AnalyticsPersonPeriodDateFixerBase
	{
		public AnalyticsPersonPeriodDateFixer(IAnalyticsDateRepository analyticsDateRepository, IAnalyticsIntervalRepository analyticsIntervalRepository) : 
			base(analyticsDateRepository, analyticsIntervalRepository)
		{
		}
	}
}