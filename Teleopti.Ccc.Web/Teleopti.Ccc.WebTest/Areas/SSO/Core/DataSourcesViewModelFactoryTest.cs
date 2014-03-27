using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.SSO.Core
{
	[TestFixture]
	public class DataSourcesViewModelFactoryTest
	{
		private static ApplicationDataSourcesViewModelFactory Target(IDataSourcesProvider dataSourcesProvider)
		{
			return new ApplicationDataSourcesViewModelFactory(new ApplicationAuthenticationType(null, new Lazy<IDataSourcesProvider>(() => dataSourcesProvider)));
		}

		[Test]
		public void ShouldRetrieveWindowsDataSources()
		{
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = Target(dataSourcesProvider);
			var dataSource = new FakeDataSource { DataSourceName = "Datasource" };

			dataSourcesProvider.Stub(x => x.RetrieveDatasourcesForApplication()).Return(new[] { dataSource });

			var result = target.DataSources();

			result.Single().Name.Should().Be("Datasource");
			result.Single().Type.Should().Be("application");
		}

	}
}