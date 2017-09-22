using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	internal class ScheduleEntityLoader : ILoadAggregateFromBroker<IPersistableScheduleData>
    {
		private readonly Func<IPersistableScheduleData> _scehduleEntityFinder;

		public ScheduleEntityLoader(Func<IPersistableScheduleData> scehduleEntityFinder)
        {
            _scehduleEntityFinder = scehduleEntityFinder;
        }

		public IPersistableScheduleData LoadAggregate(Guid id)
        {
            return _scehduleEntityFinder();
        }
    }
}