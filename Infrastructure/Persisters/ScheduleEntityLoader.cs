using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
    internal class ScheduleEntityLoader : ILoadAggregateById<IPersistableScheduleData>
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