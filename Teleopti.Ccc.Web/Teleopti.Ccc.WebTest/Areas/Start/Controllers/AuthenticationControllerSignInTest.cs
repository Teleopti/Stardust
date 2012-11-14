using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationControllerSignInTest
	{
		[Test]
		public void ShouldPopulateApplicationDatasources()
		{
			var dataSourceProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = new AuthenticationController(new AuthenticationViewModelFactory(dataSourceProvider, null), null, null, null, MockRepository.GenerateMock<ILayoutBaseViewModelFactory>(), null);

			dataSourceProvider.Stub(x => x.RetrieveDatasourcesForApplication()).Return(new List<IDataSource> { new FakeDataSource { DataSourceName = "ds1" } });

			var result = target.SignIn() as ViewResult;
			var model = result.ViewData.Model as SignInViewModel;

			model.ApplicationSignIn.DataSources.Select(x => x.Name)
				.Should().Have.SameValuesAs(new[] { "ds1" });
		}

		[Test]
		public void ShouldPopulateWindowsDatasources()
		{
			var dataSourceProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = new AuthenticationController(new AuthenticationViewModelFactory(dataSourceProvider, null), null, null, null, MockRepository.GenerateMock<ILayoutBaseViewModelFactory>(), null);

			dataSourceProvider.Stub(x => x.RetrieveDatasourcesForWindows()).Return(new List<IDataSource> { new FakeDataSource { DataSourceName = "ds2" } });

			var result = target.SignIn() as ViewResult;
			var model = result.ViewData.Model as SignInViewModel;

			model.WindowsSignIn.DataSources.Select(x => x.Name)
				.Should().Have.SameValuesAs(new[] { "ds2" });
		}
	}
}
