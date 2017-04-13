﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AddPersonalActivityViewModel :  AddLayerViewModel<IActivity>
    {
        public AddPersonalActivityViewModel(IEnumerable<IActivity> activities, DateTimePeriod period,TimeSpan interval)
            : base(activities, period, UserTexts.Resources.AddPersonalActivity, interval, null)
        {
           

        }
    }

   
}


