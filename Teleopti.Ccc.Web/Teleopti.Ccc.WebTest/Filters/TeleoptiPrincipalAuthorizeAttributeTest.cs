using System;
using System.Threading;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class TeleoptiPrincipalAuthorizeAttributeTest
	{
		[Test]
		public void ShouldReturnRedirectResultWhenGenericPrincipal()
		{
			var authenticationModule = MockRepository.GenerateMock<IAuthenticationModule>();
			authenticationModule.Stub(x => x.Issuer).Return("http://myissuer");
			authenticationModule.Stub(x => x.Realm).Return("http://mytime");
			var target = new TeleoptiPrincipalAuthorizeAttribute(authenticationModule, MockRepository.GenerateMock<IIdentityProviderProvider>());
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<RedirectResult>();
		}

		[Test, Ignore("Should work without other handling")]
		public void ShouldRedirectToStartAuthenticationSignIn()
		{
			var authenticationModule = MockRepository.GenerateMock<IAuthenticationModule>();
			var target = new TeleoptiPrincipalAuthorizeAttribute(authenticationModule, MockRepository.GenerateMock<IIdentityProviderProvider>());
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as RedirectToRouteResult;
			result.RouteValues.Values.Should().Have.SameValuesAs("Start", "Authentication", "");
		}

		[Test]
		public void ShouldGetDefaultHomeRealm()
		{
			var authenticationModule = MockRepository.GenerateMock<IAuthenticationModule>();
			authenticationModule.Stub(x => x.Issuer).Return("http://myissuer");
			authenticationModule.Stub(x => x.Realm).Return("http://mytime");
			var identityProviderProvider = MockRepository.GenerateMock<IIdentityProviderProvider>();
			identityProviderProvider.Stub(x => x.DefaultProvider()).Return("urn:ProviderX");
			var target = new TeleoptiPrincipalAuthorizeAttribute(authenticationModule, identityProviderProvider);
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as RedirectResult;
			result.Url.ToUpperInvariant().Should().Contain(Uri.EscapeDataString("urn:ProviderX").ToUpperInvariant());
		}

		[Test]
		public void ShouldRedirectToAreasAuthenticationSignIn()
		{
			var authenticationModule = MockRepository.GenerateMock<IAuthenticationModule>();
			authenticationModule.Stub(x => x.Issuer).Return("http://myissuer");
			authenticationModule.Stub(x => x.Realm).Return("http://mytime");
			var target = new TeleoptiPrincipalAuthorizeAttribute(authenticationModule, MockRepository.GenerateMock<IIdentityProviderProvider>());
			var filterTester = new FilterTester();
			filterTester.IsUser(Thread.CurrentPrincipal);
			filterTester.AddRouteDataToken("area", "MyTime");

			var result = filterTester.InvokeFilter(target) as RedirectResult;
			result.Url.Should().Contain("http://myissuer");
		}

		[Test]
		public void ShouldReturnActionsResultWhenAuthenticated()
		{
			var principal = MockRepository.GenerateMock<ITeleoptiPrincipal>();
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
			principal.Stub(x => x.Identity).Return(identity);
			identity.Stub(x => x.IsAuthenticated).Return(true);
			var target = new TeleoptiPrincipalAuthorizeAttribute(MockRepository.GenerateMock<IAuthenticationModule>(), MockRepository.GenerateMock<IIdentityProviderProvider>());
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());
			filterTester.IsUser(principal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnViewResultWithGenericPrincipalWhenExcluded()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(MockRepository.GenerateMock<IAuthenticationModule>(), MockRepository.GenerateMock<IIdentityProviderProvider>(), new[] { typeof(FilterTester.TestController) });
			var filterTester = new FilterTester();
			filterTester.ActionMethod(() => new ViewResult());
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target);

			result.Should().Be.OfType<ViewResult>();
		}

		[Test]
		public void ShouldReturnHttp403ForbiddenResultWhenAjax()
		{
			var target = new TeleoptiPrincipalAuthorizeAttribute(MockRepository.GenerateMock<IAuthenticationModule>(), MockRepository.GenerateMock<IIdentityProviderProvider>());
			var filterTester = new FilterTester();
			filterTester.IsAjaxRequest();
			filterTester.IsUser(Thread.CurrentPrincipal);

			var result = filterTester.InvokeFilter(target) as HttpStatusCodeResult;

			result.StatusCode.Should().Be(403); 
			// 403: forbidden.
			// 401 has strange behavior on iis7/ie/intranet (or something) and will display a dialog even on ajax requests!
		}
		
	}
}