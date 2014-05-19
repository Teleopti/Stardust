using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public class MutableNow : INow, IMutateNow
	{
		private DateTime? _mutatedUtc;

		public DateTime UtcDateTime()
		{
			return _mutatedUtc.HasValue ? _mutatedUtc.Value : DateTime.UtcNow;
		}

		public void Mutate(DateTime? utc)
		{
			_mutatedUtc = utc;
		}

		public bool IsMutated()
		{
			return _mutatedUtc.HasValue;
		}
	}
}