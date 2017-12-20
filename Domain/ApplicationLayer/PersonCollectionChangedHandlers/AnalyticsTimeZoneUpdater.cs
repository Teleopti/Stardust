using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class AnalyticsTimeZoneUpdater : IHandleEvent<PossibleTimeZoneChangeEvent>, IRunOnHangfire
	{
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		public AnalyticsTimeZoneUpdater(IAnalyticsTimeZoneRepository analyticsTimeZoneRepository, IBusinessUnitRepository businessUnitRepository)
		{
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
			_businessUnitRepository = businessUnitRepository;
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PossibleTimeZoneChangeEvent @event)
		{
			SetUtcInUse();
			SetTimeZonesTobeDeleted();
		}

		public virtual void SetUtcInUse()
		{
			var timeZonesInUse = _businessUnitRepository.LoadAllTimeZones();

			var isUtcInUse = timeZonesInUse.Any(x => x.Id == "UTC");
			_analyticsTimeZoneRepository.SetUtcInUse(isUtcInUse);
		}

		public virtual void SetTimeZonesTobeDeleted()
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
