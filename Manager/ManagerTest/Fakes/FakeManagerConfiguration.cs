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

		public FakeManagerConfiguration(int allowedNodeDownTimeSeconds = 15)
		{
			AllowedNodeDownTimeSeconds = allowedNodeDownTimeSeconds;
		}
	}
}
