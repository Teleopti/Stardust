using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public sealed class UtcNow : IUtcNow, IModifyUtcNow
	{
		private DateTime? _fixedUtcDateTime;

		public DateTime UtcDateTime()
		{
			return _fixedUtcDateTime.GetValueOrDefault(DateTime.UtcNow);
		}

		void IModifyUtcNow.SetUtcNow(DateTime? localDateTime)
		{
			_fixedUtcDateTime = localDateTime;
		}

		public bool IsExplicitlySet()
		{
			return _fixedUtcDateTime.HasValue;
		}
	}
}