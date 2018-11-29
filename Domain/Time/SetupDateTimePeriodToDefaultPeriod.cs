using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Time
{
    public class SetupDateTimePeriodToDefaultPeriod : ISetupDateTimePeriod
    {
        private readonly DateTimePeriod? _period;
        private readonly ISetupDateTimePeriod _fallback;

        public SetupDateTimePeriodToDefaultPeriod(DateTimePeriod period)
        {
            _period = period;
        }

        public SetupDateTimePeriodToDefaultPeriod(DateTimePeriod? period, ISetupDateTimePeriod fallback)
        {
            _period = period;
            _fallback = fallback;
        }

        public DateTimePeriod Period
        {
            get
            {
                if (_period.HasValue)
                    return _period.Value;
                
                return getFallbackOrDefault();
            }
        }

        private DateTimePeriod getFallbackOrDefault()
        {
            if (_fallback != null) return _fallback.Period;

            return new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
        }
    }
}