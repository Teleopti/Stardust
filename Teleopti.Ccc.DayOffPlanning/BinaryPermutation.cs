using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanning
{
    public class BinaryPermutation
    {
        private readonly BitArray _array;
        private readonly IList<IBinaryValidator> _validators;
        private readonly int _startIndex;
        private readonly int _endIndex;

        public BinaryPermutation(BitArray array, IBinaryValidator validator)
            : this(array, new List<IBinaryValidator> { validator }) { }

        public BinaryPermutation(BitArray array, IEnumerable<IBinaryValidator> validatorCollection)
            : this(array, validatorCollection, 0, array.Count-1){}

        public BinaryPermutation(BitArray array, IEnumerable<IBinaryValidator> validatorCollection, int startIndex, int endIndex)
        {
            _validators = new List<IBinaryValidator>();
            _array = array;
            _startIndex = startIndex;
            _endIndex = endIndex;
            foreach (IBinaryValidator validator in validatorCollection)
            {
                _validators.Add(validator);
            }
        }


        /// <summary>
        /// Iterates all the states and adds the valid states to a list.
        /// </summary>
        /// <returns></returns>
        public IList<BitArray> IterateAll()
        {
            IList<BitArray> result = new List<BitArray>();

            ////calculate the total runs - each bit can be 1 or 0 and there are count of those

            int totalRuns = (int)Math.Pow(2, _endIndex-_startIndex+1);
            int currentRuns = totalRuns;

            do
            {
                if (Validate(_array))
                {
                    BitArray clone = (BitArray)_array.Clone();
                    result.Add(clone);
                }
                FlipBitLeftToRight(_array, _startIndex, _endIndex);

                currentRuns--;
            }

            while (currentRuns > 0);

            return result;

        }

        /// <summary>
        /// Iterates till the first valid state. If no valied states found, returns null.
        /// </summary>
        /// <returns></returns>
        public BitArray FindFirstValid()
        {
            BitArray result = null;
            ////calculate the total runs - each bit can be 1 or 0 and there are count of those
            int totalRuns = (int)Math.Pow(2, _endIndex - _startIndex + 1);
            int currentRuns = totalRuns;

            do
            {
                if (Validate(_array))
                {
                    result = (BitArray)_array.Clone();
                    break;
                }
                FlipBitLeftToRight(_array, _startIndex, _endIndex);

                currentRuns--;
            }
            while (currentRuns > 0);
            return result;
        }

        /// <summary>
        /// Flips the bits left to right starting at <paramref name="startIndex"/> and ending with <paramref name="endIndex"/>.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        private static void FlipBitLeftToRight(BitArray array, int startIndex, int endIndex)
        {

            //flip the last bit

            array[startIndex] = !array[startIndex];


            for (int index = startIndex; index < endIndex; index++)
            {

                //if previous bit is fliped to zero - the current bit should flip to one

                if (!array[index])
                {

                    array[index + 1] = !array[index + 1];

                }

                else
                {
                    //stop the bit flipping loop
                    break;
                }

            }

        }

        private bool Validate(BitArray array)
        {
            foreach (IBinaryValidator validator in _validators)
            {
                if(!validator.Validate(array))
                    return false;
            }
            return true;
        }

        //private static void FlipRightToLeftBit(BitArray array)
        //{
        //    //flip the last bit
        //    array[array.Length - 1] = !array[array.Length - 1];
        //    for (int j = array.Length - 1; j > 0; j--)
        //    {
        //        //if previous bit is fliped to zero - the current bit should flip to one
        //        if (!array[j])
        //        {
        //            array[j - 1] = !array[j - 1];
        //        }
        //        else
        //        {
        //            //stop the bit flipping loop
        //            break;
        //        }
        //    }
        //}

    }
}