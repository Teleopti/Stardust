using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.WebTest.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core.LayoutBase
{
	[TestFixture]
	public class WindowsDataSourcesViewModelFactoryTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveDataSources()
		{
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = new WindowsDataSourcesViewModelFactory(dataSourcesProvider);
			var dataSource = new FakeDataSource { DataSourceName = "Datasource" };

			dataSourcesProvider.Stub(x => x.RetrieveDatasourcesForWindows()).Return(new[] { dataSource });

			var result = target.DataSources();

			result.Single().Name.Should().Be("Datasource");
			result.Single().Type.Should().Be("windows");
		}
	}
}