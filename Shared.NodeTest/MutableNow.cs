using System;
using Stardust.Node.Workers;

namespace NodeTest
{
	public class MutableNow : INow
	{
		private DateTime? _mutatedUtc;

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

	}
}