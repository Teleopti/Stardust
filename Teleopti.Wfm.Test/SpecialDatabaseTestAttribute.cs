using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Test
{
	public class SpecialDatabaseTestAttribute : InfrastructureTestAttribute
	{
		protected override void AfterTest()
		{
			base.AfterTest();

			DataSourceHelper.RestoreApplicationDatabase(0);
		}

	}
}