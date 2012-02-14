using System;
using System.Collections;
using System.Globalization;

namespace Teleopti.Ccc.TestCommon
{
    public static class BitArrayHelper
    {
        public static string ToString(BitArray bitArray)
        {
            string result = string.Empty;
            foreach (bool c in bitArray)
            {
                result = result + Convert.ToInt16(c).ToString(CultureInfo.InvariantCulture);
            }
            return result;
        }

        public static string ToWeeklySeparatedString(BitArray bitArray)
        {
            string result = string.Empty;
            int counter = 0;
            foreach (bool c in bitArray)
            {
                counter++;
                int reminder = counter%7;
                if(counter>1 && reminder==1)
                {
                    result += "-";
                }
                result += Convert.ToInt16(c).ToString(CultureInfo.InvariantCulture);
            }
            return result;
        }
    }
}
