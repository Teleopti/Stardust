using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Ccc.WebTest.Areas.Start.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core.LayoutBase
{
	[TestFixture]
	public class ApplicationDataSourcesViewModelFactoryTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveDataSources()
		{
			var dataSourcesProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = new ApplicationDataSourcesViewModelFactory(dataSourcesProvider);
			var dataSource = new FakeDataSource {DataSourceName = "Datasource"};

			dataSourcesProvider.Stub(x => x.RetrieveDatasourcesForApplication()).Return(new[] {dataSource});

			var result = target.DataSources();

			result.Single().Name.Should().Be("Datasource");
			result.Single().Type.Should().Be("application");
		}
	}
}