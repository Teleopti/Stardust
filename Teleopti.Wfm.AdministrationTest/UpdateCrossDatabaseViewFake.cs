using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Wfm.AdministrationTest
{
	public class UpdateCrossDatabaseViewFake : IUpdateCrossDatabaseView
	{
		public void Execute(string analyticsDbConnectionString, string aggDatabase)
		{
		}
	}
}