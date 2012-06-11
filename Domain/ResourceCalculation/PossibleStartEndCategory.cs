using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    

    public class PossibleStartEndCategory : IEquatable<PossibleStartEndCategory>, IPossibleStartEndCategory
    {
        private int? _hashCode;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public IShiftCategory ShiftCategory { get; set; }
        // holds the best value of this combination
        public double ShiftValue { get; set; }

        public bool Equals(PossibleStartEndCategory other)
        {
            return other != null && GetHashCode().Equals(other.GetHashCode());
        }

        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                var catHash = 0;
                if (ShiftCategory != null)
                    catHash = ShiftCategory.GetHashCode();
                _hashCode = StartTime.GetHashCode() ^ EndTime.GetHashCode() ^ catHash;
            }

            return _hashCode.Value;
        }

    }
}
