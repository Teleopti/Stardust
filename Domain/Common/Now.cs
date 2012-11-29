using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public sealed class Now : INow, IModifyNow
	{
		private readonly Func<IUserTimeZone> _userTimeZone;
		private DateTime? _fixedLocalDateTime;

		public Now(Func<IUserTimeZone> userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public DateTime LocalDateTime()
		{
			return _fixedLocalDateTime.HasValue ? 
					_fixedLocalDateTime.Value : 
					DateTime.Now;
		}

		public DateTime UtcDateTime()
		{
			if (_fixedLocalDateTime.HasValue)
			{
				return _userTimeZone().TimeZone() == null ? 
					_fixedLocalDateTime.Value : 
					TimeZoneHelper.ConvertToUtc(_fixedLocalDateTime.Value, _userTimeZone().TimeZone());
			}
			return DateTime.UtcNow;
		}

		public DateOnly DateOnly()
		{
			return new DateOnly(LocalDateTime());
		}

		void IModifyNow.SetNow(DateTime? localDateTime)
		{
			_fixedLocalDateTime = localDateTime;
		}

		public bool IsExplicitlySet()
		{
			return _fixedLocalDateTime.HasValue;
		}
	}
}