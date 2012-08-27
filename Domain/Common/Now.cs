using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class Now : INow, IModifyNow
	{
		private DateTime? _fixedDateTime;

		public DateTime LocalTime()
		{
			return _fixedDateTime.HasValue ? 
					_fixedDateTime.Value.ToLocalTime() : 
					DateTime.Now;
		}

		public DateTime UtcTime()
		{
			return _fixedDateTime.HasValue ? 
				_fixedDateTime.Value : 
				DateTime.UtcNow;
		}

		public DateOnly Date()
		{
			return new DateOnly(LocalTime());
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