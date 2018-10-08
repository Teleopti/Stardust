using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public sealed class ThisIsNow : INow
	{
		private readonly DateTime _utcTheTime;

		public ThisIsNow(string timeInUtc)
			: this(timeInUtc.Utc())
		{
		}

		public ThisIsNow(DateTime utcTheTime)
		{
			_utcTheTime = utcTheTime;
		}

		public DateTime UtcDateTime()
		{
			return _utcTheTime;
		}
	}
}