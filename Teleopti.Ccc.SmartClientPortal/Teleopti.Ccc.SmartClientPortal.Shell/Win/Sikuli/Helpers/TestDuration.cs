using System;

namespace Teleopti.Ccc.Win.Sikuli.Helpers
{
	public interface ITestDuration
	{
		void SetStart();
		void SetEnd();
		TimeSpan GetDuration();
		string GetDurationString();
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

		public string GetDurationString()
		{
			return GetDuration().ToString(@"mm\:ss");
		}

	}
}