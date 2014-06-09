using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class IntervalLevelMaxSeatInfo
	{
		private readonly bool _isMaxSeatReached;
		private readonly double _maxSeatBoostingFactor;

		public IntervalLevelMaxSeatInfo()
		{
			_isMaxSeatReached = false;
			_maxSeatBoostingFactor = 0.0001;
		}

		public IntervalLevelMaxSeatInfo(bool isMaxSeatReached, double maxSeatBoostingFactor)
		{
			_isMaxSeatReached = isMaxSeatReached;
			_maxSeatBoostingFactor = maxSeatBoostingFactor;
		}

		public bool IsMaxSeatReached
		{
			get { return _isMaxSeatReached; }
		}

		public double  MaxSeatBoostingFactor
		{
			get { return _maxSeatBoostingFactor; }
		}
	}
}
