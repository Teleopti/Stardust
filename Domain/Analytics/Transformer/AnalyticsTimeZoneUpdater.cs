using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Analytics.Transformer
{
	public class AnalyticsTimeZoneUpdater
	{
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		public AnalyticsTimeZoneUpdater(IAnalyticsTimeZoneRepository analyticsTimeZoneRepository, IBusinessUnitRepository businessUnitRepository)
		{
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
			_businessUnitRepository = businessUnitRepository;
		}

		public void SetUtcInUse()
		{
			var timeZonesInUse = _businessUnitRepository.LoadAllTimeZones();

			var isUtcInUse = timeZonesInUse.Any(x => x.Id == "UTC");
			_analyticsTimeZoneRepository.SetUtcInUse(isUtcInUse);
		}
		
		public void SetTimeZonesTobeDeleted()
		{
			var timeZonesInUse = _businessUnitRepository.LoadAllTimeZones().Select(x => x.Id).ToList();
			var analyticsTimeZones = _analyticsTimeZoneRepository.GetAll().Select(x => x.TimeZoneCode);

			var timeZonesNotInUse = analyticsTimeZones.Except(timeZonesInUse);
			foreach (var timeZone in timeZonesNotInUse.Where(x => x != "UTC" ))
			{
				_analyticsTimeZoneRepository.SetToBeDeleted(timeZone, true);
			}
			foreach (var timeZoneInUse in timeZonesInUse)
			{
				_analyticsTimeZoneRepository.SetToBeDeleted(timeZoneInUse, false);
			}
		}
	}
}
