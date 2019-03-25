using System;

namespace NodeTest.JobHandlers
{
	public class TestJobParams
	{
		
		public TestJobParams(string name, 
								 int duration,
								 bool exitApplication = false)
		{
			ExitApplication = exitApplication;
			Name = name;
			Duration = duration;
		}

		public string Name { get; private set; }

		public int Duration { get; set; }

		public bool ExitApplication { get; set; }
	}
}