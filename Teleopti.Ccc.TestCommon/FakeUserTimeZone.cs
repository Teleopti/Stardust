using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeUserTimeZone : IUserTimeZone
	{
		private TimeZoneInfo _timeZone;

		public FakeUserTimeZone()
		{
			_timeZone = TimeZoneInfo.Utc;
		}

		public FakeUserTimeZone(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public TimeZoneInfo TimeZone()
		{
			return _timeZone;
		}

		public void Is(TimeZoneInfo timeZone)
		{
			_timeZone = timeZone;
		}

		public void IsHawaii()
		{
			Is(TimeZoneInfoFactory.HawaiiTimeZoneInfo());
		}

		public void IsSweden()
		{
			Is(TimeZoneInfoFactory.StockholmTimeZoneInfo());
		}

		public void IsChina()
		{
			Is(TimeZoneInfoFactory.ChinaTimeZoneInfo());
		}

		public void IsNewYork()
		{
			Is(TimeZoneInfoFactory.NewYorkTimeZoneInfo());
		}

		public void IsAustralia()
		{
			Is(TimeZoneInfoFactory.AustralianTimeZoneInfo());
		}

		public void IsNewZealand()
		{
			Is(TimeZoneInfoFactory.NewZealandTimeZoneInfo());
		}
	}
}