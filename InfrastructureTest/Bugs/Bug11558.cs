using System.Collections.Generic;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			var dsFactory = new DataSourcesFactory(new EnversConfiguration(), new List<IDenormalizer>());
            dataSource = dsFactory.Create(SetupFixtureForAssembly.Sql2005conf(ConnectionStringHelper.ConnectionStringUsedInTests, 1),
								  ConnectionStringHelper.ConnectionStringUsedInTestsMatrix);
        }

        [Test]
        public void CanSetTimeoutValueOnConfig()
        {
            const string query = @"select 1 WAITFOR DELAY :waitFor";

            using(var uow = dataSource.Application.CreateAndOpenUnitOfWork())
            {

                var session = grabUowSession(uow);
                Assert.Throws<DataSourceException>(() =>
                                        session.CreateSQLQuery(query)
                                            .SetString("waitFor", "00:00:02")
                                            .UniqueResult<int>()
                                            );

            }
        }

        private static ISession grabUowSession(IUnitOfWork uow)
        {
            return (ISession)uow.GetType()
                .GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(uow, null);
        }
    }
}