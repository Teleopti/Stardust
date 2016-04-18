using System;

namespace Manager.Integration.Test
{
	public class TestJobTimerParams
	{
		public TestJobTimerParams(string name,
								 TimeSpan duration)
		{
			Name = name;
			Duration = duration;
		}

		public string Name { get; private set; }
		public TimeSpan Duration { get; set; }
	}
}