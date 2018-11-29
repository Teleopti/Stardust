using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsDateMapper
	{
		private readonly IAnalyticsDateRepository _analyticsDateRepository;

		public AnalyticsDateMapper(IAnalyticsDateRepository analyticsDateRepository)
		{
			_analyticsDateRepository = analyticsDateRepository;
		}

		public bool MapDateId(DateOnly date, out int dateId)
		{
			var noDateIdFound = new AnalyticsDate();
			var datePair = _analyticsDateRepository.Date(date.Date) ?? noDateIdFound;
			if (datePair.DateDate == noDateIdFound.DateDate)
			{
				dateId = -1;
				return false;
			}

			dateId = datePair.DateId;
			return true;
		}
	}
}