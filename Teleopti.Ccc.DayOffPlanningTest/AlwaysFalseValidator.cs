using System.Collections;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    public class AlwaysFalseValidator : IBinaryValidator
    {
        public bool Validate(BitArray array)
        {
            return false;
        }
    }
}