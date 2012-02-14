using System;
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
using Teleopti.Interfaces.Infrastructure;

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

		public sealed class FakeDataSource : IDataSource
		{
			public IUnitOfWorkFactory Statistic { get; set; }
			public IUnitOfWorkFactory Application { get; set; }
			public IAuthenticationSettings AuthenticationSettings { get; set; }
			public string DataSourceName { get; set; }

			public void ResetStatistic()
			{
				throw new NotImplementedException();
			}

			public string Server { get; set; }

			public string InitialCatalog { get; set; }
			public string OriginalFileName { get; set; }
			public AuthenticationTypeOption AuthenticationTypeOption { get; set; }

			public void Dispose()
			{
			}
		}
	}
}
