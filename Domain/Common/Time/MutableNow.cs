using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

		public DateTime UtcDateTime()
		{
			return _mutatedUtc ?? DateTime.UtcNow;
		}

		public void Reset()
		{
			_mutatedUtc = null;
		}

		public virtual void Is(DateTime? utc)
		{
			_mutatedUtc = utc;
		}

		public bool IsMutated()
		{
			return _mutatedUtc.HasValue;
		}

	}
}