using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	[TestFixture]
	public class WindowsDataSourceProviderTest
	{
		private IWindowsUserProvider windowsUserProvider;
		private IRepositoryFactory repositoryFactory;
		private WindowsDataSourceProvider target;
		private IAvailableDataSourcesProvider availableDataSourcesProvider;

		[SetUp]
		public void Setup()
		{
			windowsUserProvider = MockRepository.GenerateStrictMock<IWindowsUserProvider>();
			repositoryFactory = MockRepository.GenerateStrictMock<IRepositoryFactory>();
			availableDataSourcesProvider = MockRepository.GenerateStrictMock<IAvailableDataSourcesProvider>();
			target = new WindowsDataSourceProvider(availableDataSourcesProvider, repositoryFactory, windowsUserProvider);
		}

		[Test]
		public void VerifyDataSourceContainerIsReturned()
		{
			var dataSource = MockRepository.GenerateStrictMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateStrictMock<IUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateStrictMock<IUnitOfWork>();
			var personRepository = MockRepository.GenerateStrictMock<IPersonRepository>();
			var foundPerson = MockRepository.GenerateStrictMock<IPerson>();
			IPerson person;

			availableDataSourcesProvider.Stub(x => x.AvailableDataSources()).Return(new List<IDataSource> { dataSource });
			windowsUserProvider.Stub(x => x.DomainName).Return("toptinet").Repeat.Once();
			windowsUserProvider.Stub(x => x.UserName).Return("robink").Repeat.Once();
			repositoryFactory.Stub(x => x.CreatePersonRepository(unitOfWork)).Return(personRepository);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			personRepository.Stub(x => x.TryFindIdentityAuthenticatedPerson(@"toptinet\robink", out person)).OutRef(foundPerson).Return(true);
			dataSource.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Windows);
			unitOfWork.Stub(x => x.Dispose());

			var dataSources = target.DataSourceList();
			Assert.AreEqual(1, dataSources.Count());
			var container = dataSources.ElementAt(0);
			Assert.AreEqual(dataSource, container.DataSource);
			Assert.AreEqual(AuthenticationTypeOption.Windows, container.AuthenticationTypeOption);
			Assert.AreEqual(repositoryFactory, container.RepositoryFactory);
			Assert.AreEqual(foundPerson, container.User);

		}

		[Test]
		public void ShouldOnlyReturnDataSourceGivenWindowsAuthenticationIsPossible()
		{
			var dataSource = MockRepository.GenerateStrictMock<IDataSource>();
			availableDataSourcesProvider.Stub(x => x.AvailableDataSources()).Return(new List<IDataSource> { dataSource });
			dataSource.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			target.DataSourceList().Should().Be.Empty();
		}
	}
}