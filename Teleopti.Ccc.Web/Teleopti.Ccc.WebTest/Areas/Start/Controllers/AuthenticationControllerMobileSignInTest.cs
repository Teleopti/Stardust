using System.Collections.Generic;
using System.Linq;
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
	public class AuthenticationControllerMobileSignInTest
	{
		[Test, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void ShouldPopulateApplicationDatasources()
		{
			var dataSourceProvider = MockRepository.GenerateMock<IDataSourcesProvider>();
			var target = new AuthenticationController(new AuthenticationViewModelFactory(dataSourceProvider, null), null, null,
			                                          null, MockRepository.GenerateMock<ILayoutBaseViewModelFactory>(), null);

			dataSourceProvider.Stub(x => x.RetrieveDatasourcesForApplication()).Return(new List<IDataSource>
			                                                                           	{
			                                                                           		new AuthenticationControllerSignInTest.
			                                                                           			FakeDataSource {DataSourceName = "ds1"}
			                                                                           	});

			var result = target.MobileSignIn();
			var model = result.ViewData.Model as SignInViewModel;

			model.ApplicationSignIn.DataSources.Select(x => x.Name)
				.Should().Have.SameValuesAs(new[] {"ds1"});
		}
	}
}