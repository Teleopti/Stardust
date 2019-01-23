using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using IAuthenticationModule = Teleopti.Ccc.Web.Filters.IAuthenticationModule;
using RedirectToRouteResult = System.Web.Mvc.RedirectToRouteResult;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class TeleoptiPrincipalAuthorizeAttributeTest
	{
		[Test]
		public void ShouldReturnRedirectResultWhenGenericPrincipal()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(),new CheckTenantUserExistsFake());
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<RedirectToRouteResult>();
		}

		[Test, Ignore("Should work without other handling")]
		public void ShouldRedirectToStartAuthenticationSignIn()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new CheckTenantUserExistsFake());
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("Start", "Authentication", "");
		}

		[Test]
		public void ShouldGetDefaultHomeRealm()
		{
			var identityProviderProvider = MockRepository.GenerateMock<IIdentityProviderProvider>();
			identityProviderProvider.Stub(x => x.DefaultProvider()).Return("urn:ProviderX");
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), identityProviderProvider, new CheckTenantUserExistsFake());
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("Return", "Hash", "Start", "http://myissuer/?wa=wsignin1.0&wtrealm=http%3a%2f%2fmytime&wctx=ru%3dfoo%2f&whr=urn%3aProviderX");
		}

		[Test]
		public void ShouldRedirectToAreasAuthenticationSignIn()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new CheckTenantUserExistsFake());
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);
			filterTester.AddRouteDataToken("area", "MyTime");

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues["redirectUrl"].ToString().Should().Contain(new FakeAuthenticationModule().Issuer(null).ToString());
		}

		[Test]
		public void ShouldReturnActionsResultWhenAuthenticated()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new CheckTenantUserExistsFake());
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());
			filterTester.IsUser(new TeleoptiPrincipal(new TeleoptiIdentity("_", null, null, null, null, null), null));

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnViewResultWithGenericPrincipalWhenExcluded()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new CheckTenantUserExistsFake(), new[] {typeof (FilterTester.TestController)});
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldBeAbleAccessControllersExcludedByBaseType()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new CheckTenantUserExistsFake(), new[] { typeof(FilterTester.TestController) });
			var filterTester = new FilterTester();
			filterTester.UseController(new TestControllerProxy(() => new ViewResult()));

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnHttp403ForbiddenResultWhenAjax()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new CheckTenantUserExistsFake());
			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<HttpUnauthorizedResult>();
		}

		[Test]
		public void ShouldRedirectToTenantInfoWhenNoTenantAdminUsers()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(new FakeAuthenticationModule(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new CheckTenantUserExistsFake(true));
			var filterTester = new FilterTester();

			var result = filterTester.InvokeFilter(target) as RedirectResult;
			result.Url.Should().Be.EqualTo("MultiTenancy/TenantAdminInfo");
		}

		public class TestControllerProxy : FilterTester.TestController
		{
			public TestControllerProxy(Func<ActionResult> controllerAction) : base(controllerAction)
			{
			}
		}

		public class FakeAuthenticationModule : IAuthenticationModule
		{
			public Uri Issuer(HttpContextBase request)
			{
				return new Uri("http://myissuer");
			}

			public string Realm => "http://mytime";
		}
	}

	public class CheckTenantUserExistsFake : ICheckTenantUserExists
	{
		private readonly bool _returnEmpty;

		public CheckTenantUserExistsFake(bool returnEmpty = false)
		{
			_returnEmpty = returnEmpty;
		}

		public bool Exists()
		{
			return !_returnEmpty;
		}
	}
}