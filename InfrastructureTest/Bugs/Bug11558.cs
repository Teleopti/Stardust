using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Bugs
{
    [TestFixture]
    [Category("LongRunning")]
    public class Bug11558 
    {
        private IDataSource dataSource;

        [SetUp]
        public void Setup()
        {
			var dsFactory = new DataSourcesFactory(new EnversConfiguration(), new NoPersistCallbacks(), DataSourceConfigurationSetter.ForTest(), new CurrentHttpContext(), null);
            dataSource = dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(ConnectionStringHelper.ConnectionStringUsedInTests, 1),
								  ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
        }

        [Test]
        public void CanSetTimeoutValueOnConfig()
        {
            const string query = @"select 1 WAITFOR DELAY :waitFor";

            using(var uow = dataSource.Application.CreateAndOpenUnitOfWork())
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