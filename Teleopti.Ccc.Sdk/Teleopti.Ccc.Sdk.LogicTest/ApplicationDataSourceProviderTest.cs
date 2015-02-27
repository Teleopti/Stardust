using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	[TestFixture]
	public class ApplicationDataSourceProviderTest
	{
		private IRepositoryFactory repositoryFactory;
		private ApplicationDataSourceProvider target;
		private IAvailableDataSourcesProvider availableDataSourcesProvider;
		private IFindApplicationUser checkLogOn;

		[SetUp]
		public void Setup()
		{
			repositoryFactory = MockRepository.GenerateStrictMock<IRepositoryFactory>();
			availableDataSourcesProvider = MockRepository.GenerateStrictMock<IAvailableDataSourcesProvider>();
			checkLogOn = MockRepository.GenerateStrictMock<IFindApplicationUser>();
			target = new ApplicationDataSourceProvider(availableDataSourcesProvider, repositoryFactory, checkLogOn);
		}

		[Test]
		public void VerifyDataSourceContainerIsReturned()
		{
			var dataSource = MockRepository.GenerateStrictMock<IDataSource>();

			availableDataSourcesProvider.Stub(x => x.AvailableDataSources()).Return(new List<IDataSource> { dataSource });

			var dataSources = target.DataSourceList();
			Assert.AreEqual(1, dataSources.Count());
			var container = dataSources.ElementAt(0);
			Assert.AreEqual(dataSource, container.DataSource);
			Assert.AreEqual(AuthenticationTypeOption.Application, container.AuthenticationTypeOption);
			Assert.AreEqual(repositoryFactory, container.RepositoryFactory);

		}
	}
}