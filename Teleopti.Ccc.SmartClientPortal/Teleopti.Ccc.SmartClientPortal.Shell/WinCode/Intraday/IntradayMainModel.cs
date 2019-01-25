using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday
{
    public class IntradayMainModel
    {
        public DateOnlyPeriod Period { get; set; }
        public IScenario Scenario { get; set; }
        public IEnumerable<IEntity> EntityCollection { get; set; }

        public DateTimePeriod PeriodAsDateTimePeriod()
        {
            return Period.ToDateTimePeriod(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
        }
    }
}