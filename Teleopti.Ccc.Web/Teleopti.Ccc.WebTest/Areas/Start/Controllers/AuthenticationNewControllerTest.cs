using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Areas.Start.Models.LayoutBase;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Startup;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
	[TestFixture]
	public class AuthenticationNewControllerTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldReturnSignInView()
		{
			var layoutBaseViewModelFactory = MockRepository.GenerateMock<ILayoutBaseViewModelFactory>();
			var target = new AuthenticationNewController(layoutBaseViewModelFactory, null, null);
			var layoutBaseViewModel = new LayoutBaseViewModel();

			layoutBaseViewModelFactory.Stub(x => x.CreateLayoutBaseViewModel()).Return(layoutBaseViewModel);

			var result = target.SignIn();
			result.ViewName.Should().Be.EqualTo(string.Empty);
			Assert.That(result.ViewBag.LayoutBase, Is.SameAs(layoutBaseViewModel));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveDataSources()
		{
			var dataSourcesViewModelFactory = MockRepository.GenerateMock<IDataSourcesViewModelFactory>();
			var target = new AuthenticationNewController(null, new[] { dataSourcesViewModelFactory }, null);
			var dataSourcees = new[] { new DataSourceViewModelNew() };
			dataSourcesViewModelFactory.Stub(x => x.DataSources()).Return(dataSourcees);

			var result = target.DataSources();

			var data = result.Data as IEnumerable<DataSourceViewModelNew>;
			data.Should().Have.SameSequenceAs(dataSourcees);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldAuthenticateUserRetrievingBusinessUnits()
		{
			var target = new AuthenticationNewController(null, null, MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>());
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult());

			target.BusinessUnits(authenticationModel);

			authenticationModel.AssertWasCalled(x => x.AuthenticateUser());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRetrieveBusinessUnits()
		{
			var authenticationResult = new AuthenticateResult
				{
					DataSource = new FakeDataSource(),
					Person = new Person()
				};
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(authenticationResult);
			var businessUnitViewModels = new[] {new BusinessUnitViewModel()};
			var businessUnitViewModelFactory = MockRepository.GenerateMock<IBusinessUnitsViewModelFactory>();
			businessUnitViewModelFactory.Stub(x => x.BusinessUnits(authenticationResult.DataSource, authenticationResult.Person)).Return(businessUnitViewModels);
			var target = new AuthenticationNewController(null, null, businessUnitViewModelFactory);

			var result = target.BusinessUnits(authenticationModel);

			result.Data.Should().Be.SameInstanceAs(businessUnitViewModels);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test, Ignore]
		public void ShouldReturnErrorIfAuthenticationUnsuccessfulRetrievingBusinessUnits()
		{
			var target = new AuthenticationNewController(null, null, null);
			var authenticationModel = MockRepository.GenerateMock<IAuthenticationModel>();
			authenticationModel.Stub(x => x.AuthenticateUser()).Return(new AuthenticateResult {Successful = false});

			var result = target.BusinessUnits(authenticationModel);

			target.Response.StatusCode.Should().Be(400);
			target.Response.TrySkipIisCustomErrors.Should().Be.True();
			target.ModelState.Values.Single().Errors.Single().ErrorMessage.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
			(result.Data as ModelStateResult).Errors.Single().Should().Be(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

	}

	[TestFixture]
	public class BusinessUnitsViewModelFactoryTest
	{
		[Test]
		public void ShouldRetrieveBusinessUnitsForPerson()
		{
			var businessUnitProvider = MockRepository.GenerateMock<IBusinessUnitProvider>();
			var target = new BusinessUnitsViewModelFactory(businessUnitProvider);
			var dataSource = new FakeDataSource();
			var person = new Person();
			var businessUnit = new BusinessUnit("bu");
			businessUnit.SetId(Guid.NewGuid());
			businessUnitProvider.Stub(x => x.RetrieveBusinessUnitsForPerson(dataSource, person)).Return(new[] {businessUnit});
			
			var result = target.BusinessUnits(dataSource, person);

			result.Single().Id.Should().Be(businessUnit.Id);
			result.Single().Name.Should().Be(businessUnit.Name);
		}
	}

	[TestFixture]
	public class AuthenticationModelBinderTest
	{

		private static ModelBindingContext BindingContext(NameValueCollection values)
		{
			return new ModelBindingContext
				{
					ValueProvider = new NameValueCollectionValueProvider(
						values,
						CultureInfo.CurrentCulture)
				};
		}

		[Test]
		public void ShouldBindApplicationAuthenticationModel()
		{
			var target = new AuthenticationModelBinder(new Lazy<IAuthenticator>(() => null));
			var bindingContext = BindingContext(new NameValueCollection
				{
					{"type", "application"},
					{"datasource", "mydata"},
					{"username", "name"},
					{"password", "pwd"}
				});

			var result = target.BindModel(null, bindingContext) as ApplicationAuthenticationModel;

			result.DataSourceName.Should().Be("mydata");
			result.UserName.Should().Be("name");
			result.Password.Should().Be("pwd");
		}

		[Test]
		public void ShouldBindWindowsAuthenticationModel()
		{
			var target = new AuthenticationModelBinder(new Lazy<IAuthenticator>(() => null));
			var bindingContext = BindingContext(new NameValueCollection
				{
					{"type", "windows"},
					{"datasource", "mydata"}
				});

			var result = target.BindModel(null, bindingContext) as WindowsAuthenticationModel;

			result.DataSourceName.Should().Be("mydata");
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldThrowOnUnknownType()
		{
			var target = new AuthenticationModelBinder(null);
			var bindingContext = BindingContext(new NameValueCollection
				{
					{"type", "non existing"}
				});

			Assert.Throws<NotImplementedException>(() => target.BindModel(null, bindingContext));
		}
	}

	[TestFixture]
	public class WindowsAuthenticationModelTest
	{
		[Test]
		public void ShouldAuthenticateUser()
		{
			var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var expectedResult = new AuthenticateResult();
			authenticator.Stub(x => x.AuthenticateWindowsUser("mydata")).Return(expectedResult);
			var target = new WindowsAuthenticationModel(authenticator) { DataSourceName = "mydata" };

			var result = target.AuthenticateUser();

			result.Should().Be.SameInstanceAs(expectedResult);
		}
	}

	[TestFixture]
	public class ApplicationAuthenticationModelTest
	{
		[Test]
		public void ShouldAuthenticateUser()
	    {
	        var authenticator = MockRepository.GenerateMock<IAuthenticator>();
			var expectedResult = new AuthenticateResult();
			authenticator.Stub(x => x.AuthenticateApplicationUser("mydata", "username", "password")).Return(expectedResult);
			var target = new ApplicationAuthenticationModel(authenticator)
				{
					DataSourceName = "mydata",
					Password = "password",
					UserName = "username"
				};

	        var result = target.AuthenticateUser();

			result.Should().Be.SameInstanceAs(expectedResult);
		}
	}

}
