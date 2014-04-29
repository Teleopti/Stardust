using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.DataProvider
{
	[TestFixture]
	public class DataSourcesProviderTest
	{
		private IApplicationData applicationData;
		private IDataSourcesProvider target;
		private IAvailableIdentityDataSources _availableIdentityDataSources;
		private ITokenIdentityProvider tokenIdentityProvider;
		private IAvailableApplicationTokenDataSource availableApplicationTokenDataSource;

		[SetUp]
		public void Setup()
		{
			applicationData = MockRepository.GenerateMock<IApplicationData>();
			_availableIdentityDataSources = MockRepository.GenerateMock<IAvailableIdentityDataSources>();
			availableApplicationTokenDataSource = MockRepository.GenerateMock<IAvailableApplicationTokenDataSource>();
			tokenIdentityProvider = MockRepository.GenerateMock<ITokenIdentityProvider>();
			target = new DataSourcesProvider(applicationData, _availableIdentityDataSources, availableApplicationTokenDataSource, tokenIdentityProvider);
		}

		[Test]
		public void ApplicationDatasourcesShouldReturnFullListOfAvailableDatasources()
		{
			var dataSources = new List<IDataSource>();

			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(dataSources);

			target.RetrieveDatasourcesForApplication()
				.Should().Be.SameInstanceAs(dataSources);
		}

		[Test]
		public void WindowsDatasourcesShouldReturnAvailableDatasource()
		{
			var validDs = MockRepository.GenerateMock<IDataSource>();
			var invalidDs = MockRepository.GenerateMock<IDataSource>();
			var dsList = new[] {validDs, invalidDs};
			var token = new TokenIdentity {OriginalToken = @"domain\user"};

			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(dsList);
			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(token);
			_availableIdentityDataSources.Stub(x => x.AvailableDataSources(dsList, token.OriginalToken)).Return(new[] {validDs});

			target.RetrieveDatasourcesForIdentity().Should().Have.SameValuesAs(new[] {validDs});
		}

		[Test]
		public void ApplicationIdentityTokenDatasourcesShouldReturnAvailableDatasource()
		{
			var validDs = MockRepository.GenerateMock<IDataSource>();
			const string dataSource = "Teleopti CCC";
			var tokenIdentity = new TokenIdentity {UserIdentifier = "user", DataSource = dataSource};

			validDs.Stub(x => x.DataSourceName).Return(dataSource);
			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(new []{validDs});

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(tokenIdentity);
			availableApplicationTokenDataSource.Stub(x => x.IsDataSourceAvailable(validDs, tokenIdentity.UserIdentifier)).Return(true);

			target.RetrieveDatasourcesForApplicationIdentityToken().Should().Have.SameValuesAs(new[] {validDs});
		}

		[Test]
		public void ApplicationIdentityTokenDatasourcesShouldReturnNullWhenNoAvailableDatasource()
		{
			var validDs = MockRepository.GenerateMock<IDataSource>();
			const string dataSource = "Teleopti CCC";
			var tokenIdentity = new TokenIdentity { UserIdentifier = "user", DataSource = dataSource };

			validDs.Stub(x => x.DataSourceName).Return("Wrong Teleopti CCC");
			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(new[] { validDs });

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(tokenIdentity);

			target.RetrieveDatasourcesForApplicationIdentityToken().Should().Be.Null();

			availableApplicationTokenDataSource.AssertWasNotCalled(x => x.IsDataSourceAvailable(null, tokenIdentity.UserIdentifier));
		}

		[Test]
		public void ApplicationIdentityTokenDatasourcesShouldReturnNullWhenNoUserFoundInAvailableDatasource()
		{
			var validDs = MockRepository.GenerateMock<IDataSource>();
			const string dataSource = "Teleopti CCC";
			var tokenIdentity = new TokenIdentity { UserIdentifier = "user", DataSource = dataSource };

			validDs.Stub(x => x.DataSourceName).Return(dataSource);
			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(new[] { validDs });
			availableApplicationTokenDataSource.Stub(x => x.IsDataSourceAvailable(validDs, tokenIdentity.UserIdentifier)).Return(false);

			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(tokenIdentity);

			target.RetrieveDatasourcesForApplicationIdentityToken().Should().Be.Null();
		}

		[Test]
		public void WindowsDatasourcesShouldReturnNullIfWinAccountIsNull()
		{
			tokenIdentityProvider.Stub(x => x.RetrieveToken()).Return(null);

			target.RetrieveDatasourcesForIdentity().Should().Be.Null();
		}

		[Test]
		public void CanFindDataSourceByName()
		{
			var hit = new testDataSource("gnaget!");
			var dataSources = new[] {new testDataSource("heja"), hit};

			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(dataSources);

			target.RetrieveDataSourceByName("gnaget!").Should().Be.SameInstanceAs(hit);
		}

		[Test]
		public void DataSourceShouldReturnNullIfNonExisting()
		{
			var dataSources = new[] {new testDataSource("heja")};

			applicationData.Stub(x => x.RegisteredDataSourceCollection).Return(dataSources);

			target.RetrieveDataSourceByName("gnaget").Should().Be.Null();
		}

		private class testDataSource : IDataSource
		{
			private readonly string _name;

			public testDataSource(string name)
			{
				_name = name;
			}

			public IUnitOfWorkFactory Statistic
			{
				get { throw new NotImplementedException(); }
			}

			public IUnitOfWorkFactory Application
			{
				get { throw new NotImplementedException(); }
			}

			public IAuthenticationSettings AuthenticationSettings
			{
				get { throw new NotImplementedException(); }
			}

			public string DataSourceName
			{
				get { return _name; }
			}

			public void ResetStatistic()
			{
				throw new NotImplementedException();
			}
			public string OriginalFileName { get; set; }
			public AuthenticationTypeOption AuthenticationTypeOption { get; set; }

			public void Dispose()
			{
			}
		}
	}
}