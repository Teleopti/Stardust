using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class ApplicationDataSourceProviderTest
    {
        private MockRepository mocks;
        private IRepositoryFactory repositoryFactory;
        private ApplicationDataSourceProvider target;
        private IAvailableDataSourcesProvider availableDataSourcesProvider;
        private IFindApplicationUser checkLogOn;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            availableDataSourcesProvider = mocks.StrictMock<IAvailableDataSourcesProvider>();
            checkLogOn = mocks.StrictMock<IFindApplicationUser>();
            target = new ApplicationDataSourceProvider(availableDataSourcesProvider, repositoryFactory, checkLogOn);
        }

        [Test]
        public void VerifyDataSourceContainerIsReturned()
        {
            IDataSource dataSource = mocks.StrictMock<IDataSource>();
            using(mocks.Record())
            {
                Expect.Call(availableDataSourcesProvider.AvailableDataSources()).Return(new List<IDataSource> {dataSource});
            }
            using (mocks.Playback())
            {
                var dataSources = target.DataSourceList();
                Assert.AreEqual(1,dataSources.Count());
                var container = dataSources.ElementAt(0);
                Assert.AreEqual(dataSource,container.DataSource);
                Assert.AreEqual(AuthenticationTypeOption.Application, container.AuthenticationTypeOption);
                Assert.AreEqual(repositoryFactory,container.RepositoryFactory);
            }
        }
    }
}