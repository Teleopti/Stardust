using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class SchedulePeriodDayOffLegalStateValidator : ISchedulePeriodDayOffLegalStateValidator
    {
        private readonly IList<IDayOffLegalStateValidator> _validatorList;
        private readonly MinMax<int> _periodIndexRange;

        public SchedulePeriodDayOffLegalStateValidator(
            IList<IDayOffLegalStateValidator> validatorList, 
            MinMax<int> periodIndexRange)
        {
            _validatorList = validatorList;
            _periodIndexRange = periodIndexRange;
        }

        public bool Validate(BitArray array)
        {
            int minimumIndex = _periodIndexRange.Minimum;
            int maximumIndex = _periodIndexRange.Maximum;

            bool oneDayOffFound = false;

            for (int i = minimumIndex; i <= maximumIndex; i++)
            {
                if(array[i])
                {
                    oneDayOffFound = true;
                    if (!ValidateValidatorList(array, i))
                        return false;
                }
            }
            return oneDayOffFound;
        }

        private bool ValidateValidatorList(BitArray array, int dayIndex)
        {
            foreach (IDayOffLegalStateValidator validator in _validatorList)
            {
                if (!validator.IsValid(array, dayIndex))
                    return false;
            }
            return true;
        }
    }
}
