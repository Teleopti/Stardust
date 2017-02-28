using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AddDayOffViewModel : AddLayerViewModel<IDayOffTemplate>
    {

        public AddDayOffViewModel(IEnumerable<IDayOffTemplate> dayOffTemplates, DateTimePeriod period)
            :base(dayOffTemplates, period, UserTexts.Resources.AddDayOff, TimeSpan.FromDays(1), null)
        {
            
        }
    }
}
