using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public sealed class Now : INow, IModifyNow
	{
		private DateTime? _fixedDateTime;

		public DateTime LocalDateTime()
		{
			return _fixedDateTime.HasValue ? 
					_fixedDateTime.Value.ToLocalTime() : 
					DateTime.Now;
		}

		public DateTime UtcDateTime()
		{
			return _fixedDateTime.HasValue ? 
				_fixedDateTime.Value : 
				DateTime.UtcNow;
		}

		public DateOnly DateOnly()
		{
			return new DateOnly(LocalDateTime());
		}

		void IModifyNow.SetNow(DateTime? dateTime)
		{
			if (dateTime.HasValue)
			{
				InParameter.VerifyDateIsUtc("dateTime", dateTime.Value);				
			}
			_fixedDateTime = dateTime;
		}
	}
}