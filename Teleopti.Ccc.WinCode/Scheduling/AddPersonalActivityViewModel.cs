﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public class AddPersonalActivityViewModel :  AddLayerViewModel<IActivity>
    {
        public AddPersonalActivityViewModel(IList<IActivity> activities, DateTimePeriod period,TimeSpan interval)
            : base(activities, period, UserTexts.Resources.AddPersonalActivity, interval)
        {
           

        }
    }

   
}


