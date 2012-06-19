using System;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    

    public class PossibleStartEndCategory : IEquatable<IPossibleStartEndCategory>, IPossibleStartEndCategory
    {
        private int? _hashCode;
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
        public IShiftCategory ShiftCategory { get; set; }
        // holds the best value of this combination
        public double ShiftValue { get; set; }

        public bool Equals(IPossibleStartEndCategory other)
        {
            return other != null && GetHashCode().Equals(other.GetHashCode());
        }
		
		public override bool Equals(object obj)
		{
			var poss = obj as IPossibleStartEndCategory;
			return poss != null && Equals(poss);
		}

        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                var catHash = 0;
                if (ShiftCategory != null)
                    catHash = ShiftCategory.GetHashCode();
                _hashCode = StartTime.GetHashCode() ^ EndTime.GetHashCode() ^ catHash ^ ShiftValue.GetHashCode() ;
            }

            return _hashCode.Value;
        }

    }
}
