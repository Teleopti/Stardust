using System.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class ArrayValueValidator : IBinaryValidator
    {
        private readonly int _arrayValue;

        public ArrayValueValidator(int arrayValue)
        {
            _arrayValue = arrayValue;
        }

        public ArrayValueValidator(BitArray original)
        {
            _arrayValue = value(original);
        }

        public bool Validate(BitArray array)
        {
            return value(array) == _arrayValue;
        }

        private static int value(BitArray array)
        {
            int value = 0;
            foreach (bool b in array)
            {
                if (b)
                    value++;
            }
            return value;
        }
    }
}