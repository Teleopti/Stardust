using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IBlockInfo
	{
		DateOnlyPeriod BlockPeriod { get; }
		IList<double?> StandardDeviations { get; set; }
		double SumOfStandardDeviations { get; }
		double AverageOfStandardDeviations { get; }
		void LockDate(DateOnly date);
		IList<DateOnly> UnLockedDates();
		void ClearLocks();
	}

	public class BlockInfo : IBlockInfo
	{
		private readonly DateOnlyPeriod _blockPeriod;
		private readonly HashSet<DateOnly> _lockedDates; 

		public BlockInfo(DateOnlyPeriod blockPeriod)
		{
			_blockPeriod = blockPeriod;
		    StandardDeviations = new List<double?>();
			_lockedDates = new HashSet<DateOnly>();
		}

		public DateOnlyPeriod BlockPeriod
		{
			get { return _blockPeriod; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<double?> StandardDeviations { get; set; }

		public double SumOfStandardDeviations
		{
			get
			{
				var sum = 0.0;
				foreach (var standardDeviation in StandardDeviations)
				{
					if (!standardDeviation.HasValue) continue;
					sum += standardDeviation.Value;
				}
				return sum;
			}
		}

		public double AverageOfStandardDeviations
		{
			get
			{
				var sum = 0.0;
				var count = 0;
				foreach (var standardDeviation in StandardDeviations)
				{
					if (!standardDeviation.HasValue) continue;
					count++;
					sum += standardDeviation.Value;
				}
				if (count != 0)
					return sum / count;
				return 0;
			}
		}

		public void LockDate(DateOnly date)
		{
			if(_blockPeriod.Contains(date))
			{
				_lockedDates.Add(date);
			}
			else
			{
				throw new ArgumentOutOfRangeException("date", "Must be within the block period");
			}
		}

		public IList<DateOnly> UnLockedDates()
		{
			var ret = new List<DateOnly>();
			foreach (var dateOnly in _blockPeriod.DayCollection())
			{
				if(!_lockedDates.Contains(dateOnly))
					ret.Add(dateOnly);
			}

			return ret;
		}

		public void ClearLocks()
		{
			_lockedDates.Clear();
		}

		public override int GetHashCode()
        {
            return _blockPeriod.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var ent = obj as IBlockInfo;
            return ent != null && Equals(ent);
        }

        public virtual bool Equals(IBlockInfo other)
        {
            if (other == null)
                return false;

            return GetHashCode() == other.GetHashCode();
        }
	}
}