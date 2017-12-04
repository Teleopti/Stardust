using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class UpdateCrossDatabaseViewFake : IUpdateCrossDatabaseView
	{
		public void Execute(string analyticsDbConnectionString, string aggDatabase)
		{
		}
	}
}