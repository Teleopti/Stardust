using StaffHubPoC.StaffHub;
using StaffHubPoC.Teleopti;

namespace StaffHubPoC
{
	public class Integrator
	{
		public void Publish()
		{
			var importer = new TeleoptiImporter();
			var exporter = new StaffHubExporter();

			var teleoptiShifts = importer.GetAllShifts();
			exporter.PostShifts(teleoptiShifts);
		}
	}
}
