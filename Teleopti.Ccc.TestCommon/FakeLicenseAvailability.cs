using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeLicenseAvailability: ILicenseAvailability
	{
		private IList<string> _paths = new List<string>();

		public void HasLicense(string path)
		{
			if (!_paths.Contains(path)) _paths.Add(path);
		}

		public bool IsLicenseEnabled(string licensePath)
		{
			if (_paths.Contains(licensePath)) return true;
			return false;
		}
	}
}
