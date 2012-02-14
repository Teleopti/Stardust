using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AddDayOffViewModel : AddLayerViewModel<IDayOffTemplate>
    {

        public AddDayOffViewModel(IList<IDayOffTemplate> dayOffTemplates, DateTimePeriod period)
            :base(dayOffTemplates, period, UserTexts.Resources.AddDayOff, TimeSpan.FromDays(1))
        {
            
        }
    }
}
