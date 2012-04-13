using NUnit.Framework;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Helper
{
	public abstract class AnalyticsDatabaseTest : DatabaseTestWithoutTransaction
	{
		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			DataSourceHelper.ClearAnalyticsData();
			SetupForAnalyticsDatabaseTest();
		}

		protected virtual void SetupForAnalyticsDatabaseTest()
		{
		}
	}
}