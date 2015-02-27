using System;

namespace Teleopti.Ccc.Win.Sikuli
{
	public interface ITestDuration
	{
		void SetStart();
		void SetEnd();
		TimeSpan GetDuration();
	}

	internal class TestDuration : ITestDuration
	{
		private DateTime _starTime;
		private DateTime _endTime;

		public void SetStart()
		{
			_starTime = DateTime.Now;
		}

		public void SetEnd()
		{
			_endTime = DateTime.Now;
		}

		public TimeSpan GetDuration()
		{
			return _endTime - _starTime;
		}
	}
}