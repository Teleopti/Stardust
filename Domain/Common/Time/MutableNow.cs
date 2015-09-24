using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public class MutableNow : INow, IMutateNow
	{
		private DateTime? _mutatedUtc;

		public MutableNow()
		{
		}

		public MutableNow(DateTime utc)
		{
			Is(utc);
		}

		public MutableNow(string utc)
		{
			this.Is(utc);
		}

		public DateTime UtcDateTime()
		{
			return _mutatedUtc ?? DateTime.UtcNow;
		}

		public void Reset()
		{
			_mutatedUtc = null;
		}

		public void Is(DateTime? utc)
		{
			_mutatedUtc = utc;
		}

		public bool IsMutated()
		{
			return _mutatedUtc.HasValue;
		}

	}
}