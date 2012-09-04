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
			return _fixedLocalDateTime.HasValue ? 
				TimeZoneHelper.ConvertToUtc(_fixedLocalDateTime.Value, _userTimeZone().TimeZone()) : 
				DateTime.UtcNow;
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