using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Helper
{
    public static class MathHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static MinMax<double>? GetMinMax(IEnumerable<double> items)
        {
            double minimum = double.MaxValue;
            double maximum = double.MinValue;

            foreach (double item in items)
            {
                    if (item < minimum)
                        minimum = item;
                    if (item > maximum)
                        maximum = item;
            }
            if (minimum == double.MaxValue && maximum == double.MinValue)
                return null;
            return new MinMax<double>(minimum, maximum);
        }
    }
}
