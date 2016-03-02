using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stardust.Manager.Interfaces;

namespace ManagerTest.Fakes
{
	class FakeManagerConfiguration : IManagerConfiguration
	{
		public int AllowedNodeDownTimeSeconds { get; set; }
		public string ConnectionString { get; set; }
		public string Route { get; set; }

		public int CheckNewJobIntervalSeconds { get; set; }

		public FakeManagerConfiguration(int allowedNodeDownTimeSeconds = 600, int checkNewJobIntervalSeconds = 10) //10 minutes
		{
			AllowedNodeDownTimeSeconds = allowedNodeDownTimeSeconds;
			CheckNewJobIntervalSeconds = checkNewJobIntervalSeconds;
		}
	}
}
