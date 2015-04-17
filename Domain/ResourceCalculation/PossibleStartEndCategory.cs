using System;
using System.Collections.Generic;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    

    public class PossibleStartEndCategory : IEquatable<IPossibleStartEndCategory>, IPossibleStartEndCategory
    {
        private int? _hashCode;
		public TimeSpan StartTime { get; set; }
    	private TimeSpan _endTime;
    	public TimeSpan EndTime
    	{
    		get { return _endTime; }
			set
			{
				_endTime = value;
			}
    	}

    	public IShiftCategory ShiftCategory { get; set; }
        public IList<DateTimePeriod>  ActivityPeriods { get; set; }

        public PossibleStartEndCategory()
        {
            ActivityPeriods = new List<DateTimePeriod>();
        }
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
                _hashCode = StartTime.GetHashCode() ^ EndTime.GetHashCode() ^ catHash;
            }

            return _hashCode.Value;
        }

    }
}
