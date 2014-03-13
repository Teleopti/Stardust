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
		private MockRepository mocks;
		private IDataSourcesProvider target;
		private IAvailableWindowsDataSources availableWindowsDataSources;
		private ITokenIdentityProvider tokenIdentityProvider;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			applicationData = mocks.StrictMock<IApplicationData>();
			availableWindowsDataSources = mocks.StrictMock<IAvailableWindowsDataSources>();
			tokenIdentityProvider = mocks.DynamicMock<ITokenIdentityProvider>();
			target = new DataSourcesProvider(applicationData, availableWindowsDataSources, tokenIdentityProvider);
		}

		[Test]
		public void ApplicationDatasourcesShouldReturnFullListOfAvailableDatasources()
		{
			var dataSources = new List<IDataSource>();

			using (mocks.Record())
			{
				Expect.Call(applicationData.RegisteredDataSourceCollection)
					.Return(dataSources);
			}

			using (mocks.Playback())
			{
				target.RetrieveDatasourcesForApplication()
					.Should().Be.SameInstanceAs(dataSources);
			}
		}

		[Test]
		public void WindowsDatasourcesShouldReturnAvailableDatasource()
		{
			var validDs = mocks.StrictMock<IDataSource>();
			var invalidDs = mocks.StrictMock<IDataSource>();
			var dsList = new[] { validDs, invalidDs };
			var winAccount = new TokenIdentity {UserIdentifier = "user", UserDomain = "domain"};

			using (mocks.Record())
			{
				Expect.Call(applicationData.RegisteredDataSourceCollection)
					.Return(dsList);

				Expect.Call(tokenIdentityProvider.RetrieveToken()).Return(winAccount);
				Expect.Call(availableWindowsDataSources.AvailableDataSources(dsList, winAccount.UserDomain, winAccount.UserIdentifier))
					.Return(new[] { validDs });
			}

			using (mocks.Playback())
			{
				target.RetrieveDatasourcesForWindows()
						.Should().Have.SameValuesAs(new[] { validDs });
			}
		}

		[Test]
		public void WindowsDatasourcesShouldReturnNullIfWinAccountIsNull()
		{
			using (mocks.Record())
			{
				Expect.Call(tokenIdentityProvider.RetrieveToken()).Return(null);
			}

			using (mocks.Playback())
			{
				target.RetrieveDatasourcesForWindows()
					.Should().Be.Null();
			}
		}

		[Test]
		public void CanFindDataSourceByName()
		{
			var hit = new testDataSource("gnaget!");
			var dataSources = new[] { new testDataSource("heja"), hit };

			using (mocks.Record())
			{
				Expect.Call(applicationData.RegisteredDataSourceCollection)
					.Return(dataSources);
			}
			using (mocks.Playback())
			{
				target.RetrieveDataSourceByName("gnaget!")
					.Should().Be.SameInstanceAs(hit);
			}
		}

		[Test]
		public void DataSourceShouldReturnNullIfNonExisting()
		{
			var dataSources = new[] { new testDataSource("heja") };

			using (mocks.Record())
			{
				Expect.Call(applicationData.RegisteredDataSourceCollection)
					.Return(dataSources);
			}
			using (mocks.Playback())
			{
				target.RetrieveDataSourceByName("gnaget")
					.Should().Be.Null();
			}
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