using System;
using System.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class LockedValueValidator : IBinaryValidator
    {
        private readonly BitArray _locks;
        private readonly BitArray _originalArray;

        public LockedValueValidator(BitArray originalArray, BitArray locks)
        {
            _locks = locks;
            _originalArray = originalArray;
        }

        public bool Validate(BitArray array)
        {
            return CheckLocks(array, _originalArray, _locks);
        }

        private static bool CheckLocks(BitArray array, BitArray originalArray, BitArray locks)
        {
            int index = -1;
            foreach (bool locked in locks)
            {
                index++;
                if(locked)
                {
                    if(array[index] != originalArray[index])
                        return false;
                }
            }
            return true;
        }
    }
}