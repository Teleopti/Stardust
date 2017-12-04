using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class GetImportUsersFake : IGetImportUsers
	{
		public ConflictModel GetConflictionUsers(string importConnectionString, string userPrefix)
		{
			return new ConflictModel
			{
				ConflictingUserModels = new ImportUserModel[] {},
				NotConflicting = new ImportUserModel[] { },
			};
		}
	}
}