using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public sealed class MutableNow : INow, IMutateNow
	{
		private DateTime? _fakedUtcDateTime;

		public DateTime UtcDateTime()
		{
			return _fakedUtcDateTime.HasValue ?
				       _fakedUtcDateTime.Value :
				       DateTime.UtcNow;
		}

		public void SetNow(DateTime? utcDateTime)
		{
			_fakedUtcDateTime = utcDateTime;
		}

		public bool IsExplicitlySet()
		{
			return _fakedUtcDateTime.HasValue;
		}
	}
}