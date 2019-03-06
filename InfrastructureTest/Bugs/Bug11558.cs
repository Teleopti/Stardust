using NUnit.Framework;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
    [TestFixture]
    [Category("BucketB")]
    public class Bug11558 
    {
        [Test]
        public void CanSetTimeoutValueOnConfig()
		{
			var dsFactory = DataSourceFactoryFactory.MakeLegacyWay().Make();
	        using (
		        var dataSource =
			        dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(InfraTestConfigReader.ApplicationConnectionString(), 1),
				        InfraTestConfigReader.AnalyticsConnectionString()))
	        {
		        const string query = @"select 1 WAITFOR DELAY :waitFor";

		        using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
		        {

			        var session = uow.FetchSession();
			        Assert.Throws<DataSourceException>(() =>
				        session.CreateSQLQuery(query)
					        .SetString("waitFor", "00:00:02")
					        .UniqueResult<int>()
				        );

		        }
	        }
        }
    }
}