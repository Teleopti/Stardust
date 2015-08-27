using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest
{
	public class CheckDatabaseVersionsFake : ICheckDatabaseVersions
	{
		public VersionResultModel GetVersions(VersionCheckModel versionCheckModel)
		{
			return new VersionResultModel {AppVersionOk = true};
		}
	}
}