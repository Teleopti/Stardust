﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Time
{
    public  class SelectedSchedulesAreEmpty : Specification<IList<IScheduleDay>>
    {
        public override bool IsSatisfiedBy(IList<IScheduleDay> obj)
        {
            return (obj == null || obj.Count == 0 ||
                    (obj[0].PersistableScheduleDataCollection().IsEmpty() && obj.Count == 1));

        }
    }
}