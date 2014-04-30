using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
    [TestFixture]
    public class WindowsDataSourceProviderTest
    {
        private MockRepository mocks;
        private IWindowsUserProvider windowsUserProvider;
        private IRepositoryFactory repositoryFactory;
        private WindowsDataSourceProvider target;
        private IAvailableDataSourcesProvider availableDataSourcesProvider;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            windowsUserProvider = mocks.StrictMock<IWindowsUserProvider>();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            availableDataSourcesProvider = mocks.StrictMock<IAvailableDataSourcesProvider>();
            target = new WindowsDataSourceProvider(availableDataSourcesProvider, repositoryFactory, windowsUserProvider);
        }

        [Test]
        public void VerifyDataSourceContainerIsReturned()
        {
            IDataSource dataSource = mocks.StrictMock<IDataSource>();
            IUnitOfWorkFactory unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();
            IUnitOfWork unitOfWork = mocks.StrictMock<IUnitOfWork>();
            IPersonRepository personRepository = mocks.StrictMock<IPersonRepository>();
            IPerson foundPerson = mocks.StrictMock<IPerson>();
            IPerson person;
            using(mocks.Record())
            {
                Expect.Call(availableDataSourcesProvider.AvailableDataSources()).Return(new List<IDataSource> {dataSource});
                Expect.Call(windowsUserProvider.DomainName).Return("toptinet").Repeat.Once();
				Expect.Call(windowsUserProvider.UserName).Return("robink").Repeat.Once();
                Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(dataSource.Application).Return(unitOfWorkFactory);
                Expect.Call(personRepository.TryFindIdentityAuthenticatedPerson(@"toptinet\robink", out person)).OutRef
                    (foundPerson).Return(true);
				Expect.Call(dataSource.AuthenticationTypeOption).Return(AuthenticationTypeOption.Windows);
                Expect.Call(unitOfWork.Dispose);
            }
            using (mocks.Playback())
            {
                var dataSources = target.DataSourceList();
                Assert.AreEqual(1,dataSources.Count());
                var container = dataSources.ElementAt(0);
                Assert.AreEqual(dataSource,container.DataSource);
                Assert.AreEqual(AuthenticationTypeOption.Windows, container.AuthenticationTypeOption);
                Assert.AreEqual(repositoryFactory,container.RepositoryFactory);
                Assert.AreEqual(foundPerson,container.User);
            }
        }

		[Test]
		public void ShouldOnlyReturnDataSourceGivenWindowsAuthenticationIsPossible()
		{
			IDataSource dataSource = mocks.StrictMock<IDataSource>();
			using (mocks.Record())
			{
				Expect.Call(availableDataSourcesProvider.AvailableDataSources()).Return(new List<IDataSource> { dataSource });
				Expect.Call(dataSource.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			}
			using (mocks.Playback())
			{
				target.DataSourceList().Should().Be.Empty();
			}
		}
    }
}