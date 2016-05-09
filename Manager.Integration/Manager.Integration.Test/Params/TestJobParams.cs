using System;

namespace Manager.Integration.Test.Params
{
	public class TestJobParams
	{
		public TestJobParams(string name,
								 TimeSpan duration)
		{
			Name = name;
			Duration = duration;
		}

		public string Name { get; private set; }
		public TimeSpan Duration { get; set; }
	}
}