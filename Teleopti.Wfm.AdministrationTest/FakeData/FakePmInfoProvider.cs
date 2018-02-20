using System;
using Teleopti.Analytics.Etl.Common.Service;

namespace Teleopti.Wfm.AdministrationTest.FakeData
{
	public class FakePmInfoProvider : IPmInfoProvider
	{
		private bool pmInstalled = false;

		public void SetPmInstalled(bool installed)
		{
			pmInstalled = installed;
		}

		public string PmInstallation()
		{
			return pmInstalled ? "True" : "False";
		}

		public string Cube()
		{
			return string.Empty;
		}
	}
}