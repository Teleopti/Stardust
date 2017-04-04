using System;

namespace Teleopti.Ccc.Domain.Common.Time
{
	public class TimePassingSimulator
	{
		private readonly bool _dayPassed;
		private readonly bool _hourPassed;
		private readonly bool _minutePassed;

		public TimePassingSimulator(DateTime from, DateTime to)
		{
			_dayPassed =
				from.Year != to.Year ||
				from.Month != to.Month ||
				from.Day != to.Day;
			_hourPassed =
				_dayPassed ||
				from.Hour != to.Hour;
			_minutePassed =
				_hourPassed ||
				from.Minute != to.Minute;
		}

		public void IfDayPassed(Action action)
		{
			if (_dayPassed)
				action.Invoke();
		}

		public void IfHourPassed(Action action)
		{
			if (_hourPassed)
				action.Invoke();
		}

		public void IfMinutePassed(Action action)
		{
			if (_minutePassed)
				action.Invoke();
		}
	}
}