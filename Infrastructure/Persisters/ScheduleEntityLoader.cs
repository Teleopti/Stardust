using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	internal class ScheduleEntityLoader : ILoadAggregateFromBroker<INonversionedPersistableScheduleData>
    {
		private readonly Func<INonversionedPersistableScheduleData> _scehduleEntityFinder;

		public ScheduleEntityLoader(Func<INonversionedPersistableScheduleData> scehduleEntityFinder)
        {
            _scehduleEntityFinder = scehduleEntityFinder;
        }

		public INonversionedPersistableScheduleData LoadAggregate(Guid id)
        {
            return _scehduleEntityFinder();
        }
    }
}