using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class IntradayMainModel
    {
        public DateOnlyPeriod Period { get; set; }
        public IScenario Scenario { get; set; }
        public IEnumerable<IEntity> EntityCollection { get; set; }

        public DateTimePeriod PeriodAsDateTimePeriod()
        {
            return Period.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
        }
    }
}