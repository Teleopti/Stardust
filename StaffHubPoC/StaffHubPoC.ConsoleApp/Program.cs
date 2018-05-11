using System;

namespace StaffHubPoC.ConsoleApp
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var integrator = new Integrator();
			integrator.Publish();
		}
	}
}
