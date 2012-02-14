using System.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    public class AlwaysTrueValidator : IBinaryValidator
    {
        public bool Validate(BitArray array)
        {
            return true;
        }
    }
}