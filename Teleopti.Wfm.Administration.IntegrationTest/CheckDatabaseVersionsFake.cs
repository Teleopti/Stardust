using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class CheckDatabaseVersionsFake : ICheckDatabaseVersions
	{
		public VersionResultModel GetVersions(string appConnectionString)
		{
			return new VersionResultModel {AppVersionOk = true};
		}
	}
}