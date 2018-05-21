using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Purge
{
	[TestFixture]
	[Category("BucketB")]
	class PurgeSettingTest
	{

		[Test]
		public void ShouldContainSecurityPurgeSetting()
		{
			var query = "select 1 from PurgeSetting where [key] = 'DaysToKeepSecurityAudit'";
			var dsFactory = DataSourceHelper.MakeLegacyWay(null).Make();
			using (
				var dataSource =
					dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(InfraTestConfigReader.ConnectionString, 1),
						InfraTestConfigReader.AnalyticsConnectionString))

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{

				var session = uow.FetchSession();
				var result = session.CreateSQLQuery(query)
					.UniqueResult<int>();
				result.Should().Be.EqualTo(1);
			}
		}
	}
}
