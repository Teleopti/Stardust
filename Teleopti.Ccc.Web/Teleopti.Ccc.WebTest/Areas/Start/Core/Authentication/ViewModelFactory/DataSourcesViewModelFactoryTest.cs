using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.WebTest.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core.Authentication.ViewModelFactory
{
	[TestFixture]
	public class DataSourcesViewModelFactoryTest
	{
		private static DataSourcesViewModelFactory Target(IDataSourcesProvider dataSourcesProvider)
		{
			return new DataSourcesViewModelFactory(
				new IAuthenticationType[]
					{
						new ApplicationAuthenticationType(null, new Lazy<IDataSourcesProvider>(() => dataSourcesProvider)),
						new WindowsAuthenticationType(null, new Lazy<IDataSourcesProvider>(() => dataSourcesProvider))
					});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveApplicationDataSources()
		{
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = Target(dataSourcesProvider);
			var dataSource = new FakeDataSource {DataSourceName = "Datasource"};

			dataSourcesProvider.Stub(x => x.RetrieveDatasourcesForApplication()).Return(new[] {dataSource});

			var result = target.DataSources();

			result.Single().Name.Should().Be("Datasource");
			result.Single().Type.Should().Be("application");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveWindowsDataSources()
		{
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = Target(dataSourcesProvider);
			var dataSource = new FakeDataSource { DataSourceName = "Datasource" };

			dataSourcesProvider.Stub(x => x.RetrieveDatasourcesForWindows()).Return(new[] { dataSource });

			var result = target.DataSources();

			result.Single().Name.Should().Be("Datasource");
			result.Single().Type.Should().Be("windows");
		}

	}
}