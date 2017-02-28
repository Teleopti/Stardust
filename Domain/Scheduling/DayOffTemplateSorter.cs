﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class DayOffTemplateSorter : IComparer<IDayOffTemplate>
    {
        public int Compare(IDayOffTemplate x, IDayOffTemplate y)
        {
            if(x == null)
                throw new ArgumentNullException("x");

            if(y == null)
                throw new ArgumentNullException("y");

            return string.Compare(x.Description.Name, y.Description.Name, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
