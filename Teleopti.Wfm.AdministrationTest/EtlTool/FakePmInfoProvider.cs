using System;
using Teleopti.Analytics.Etl.Common.Service;

namespace Teleopti.Wfm.AdministrationTest.EtlTool
{
	public class FakePmInfoProvider : IPmInfoProvider
	{
		public string PmInstallation()
		{
			return "False";
		}

		public string Cube()
		{
			return String.Empty;
		}
	}
}