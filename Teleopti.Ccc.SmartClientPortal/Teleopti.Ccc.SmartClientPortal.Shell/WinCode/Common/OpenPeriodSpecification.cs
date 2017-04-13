﻿using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.WinCode.Common
{
    public class OpenPeriodSpecification : Specification<DateOnlyPeriod>
    {
        private readonly int _maxNumberOfDays;

        public OpenPeriodSpecification(int maxNumberOfDays)
        {
            _maxNumberOfDays = maxNumberOfDays;
        }

        public override bool IsSatisfiedBy(DateOnlyPeriod obj)
        {
            return  obj.DayCount() <= _maxNumberOfDays;
        }
    }
}
