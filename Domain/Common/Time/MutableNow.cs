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
			Mutate(utc);
		}

		public MutableNow(string utc)
		{
			Mutate(utc);
		}

		public DateTime UtcDateTime()
		{
			return _mutatedUtc.HasValue ? _mutatedUtc.Value : DateTime.UtcNow;
		}

		public void Mutate(DateTime? utc)
		{
			_mutatedUtc = utc;
		}

		public void Mutate(string utc)
		{
			Mutate(utc.Utc());
		}

		public bool IsMutated()
		{
			return _mutatedUtc.HasValue;
		}

	}
}