using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationNewControllerTest
	{
		[Test]
		public void ShouldReturnSignInView()
		{
			var layoutBaseViewModelFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var target = new AuthenticationNewController(layoutBaseViewModelFactory, null);
			var layoutBaseViewModel = new LayoutBaseViewModel();

			layoutBaseViewModelFactory.Stub(x => x.CreateLayoutBaseViewModel()).Return(layoutBaseViewModel);

			var result = target.SignIn();
			result.ViewName.Should().Be.EqualTo(string.Empty);
			Assert.That(result.ViewBag.LayoutBase, Is.SameAs(layoutBaseViewModel));
		}

		[Test]
		public void ShouldRetrieveDataSources()
		{
			var dataSourcesViewModelFactory = MockRepository.GenerateMock<IDataSourcesViewModelFactory>();
			var target = new AuthenticationNewController(null, new[] { dataSourcesViewModelFactory });
			var dataSourcees = new[] { new DataSourceViewModelNew() };
			dataSourcesViewModelFactory.Stub(x => x.DataSources()).Return(dataSourcees);

			var result = target.DataSources();

			var data = result.Data as IEnumerable<DataSourceViewModelNew>;
			data.Should().Have.SameSequenceAs(dataSourcees);
		}

		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		//public void ShouldLoadDataSources()
		//{
		//    var dataSourceProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
		//    var target = new AuthenticationNewController(null, dataSourceProvider);

		//    dataSourceProvider.Stub(x => x.RetrieveDatasourcesForWindows()).Return(new List<IDataSource> { new FakeDataSource { DataSourceName = "windows" } });
		//    dataSourceProvider.Stub(x => x.RetrieveDatasourcesForApplication()).Return(new List<IDataSource> { new FakeDataSource { DataSourceName = "app" } });
		//    var result = target.LoadDataSources().Data as IEnumerable<DataSourceViewModel>;
		//    result.Count().Should().Be.EqualTo(2);
		//    result.First().Name.Should().Be.EqualTo("app");
		//    result.First().IsApplicationLogon.Should().Be.True();
		//    result.Last().Name.Should().Be.EqualTo("windows");
		//    result.Last().DisplayName.Should().Be.EqualTo("windows" + " " + Resources.WindowsLogonWithBrackets);
		//    result.Last().IsApplicationLogon.Should().Be.False();
		//}

	}
}
