using System;
using StaffHubPoC.StaffHub;
using StaffHubPoC.Teleopti;

namespace StaffHubPoC.ConsoleApp
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var staffhubExporter = new StaffHubExporter();

			var teleoptiImporter = new TeleoptiImporter();
			teleoptiImporter.GetAllShifts();


		}
	}
}
