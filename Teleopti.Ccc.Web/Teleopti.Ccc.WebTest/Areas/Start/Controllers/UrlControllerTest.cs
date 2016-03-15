using System;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Ccc.WebTest.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
    [TestFixture]
    public class UrlControllerTest
    {
        [Test]
        public void ShouldSignUrlCorrectly()
        {
			const string url = "http://my.url.com";
			const string applicationPath = "/TeleoptiCCC/Web/";

			var signatureCreator = new SignatureCreator();
	        var signed = signatureCreator.Create(url + applicationPath);

			IAuthenticationModule authenticationModule = new TeleoptiPrincipalAuthorizeAttributeTest.FakeAuthenticationModule();
            IIdentityLogon identityLogon = new FakeAuthenticator();
	        var urlController = new UrlController(CurrentHttpContext(url, applicationPath),authenticationModule, identityLogon, signatureCreator);
            var result = urlController.Index();
            Assert.AreEqual(url+applicationPath, ((result as JsonResult).Data as dynamic).Url);
            Assert.AreEqual(signed, ((result as JsonResult).Data as dynamic).Signature);
        }
		
        private static ICurrentHttpContext CurrentHttpContext(string url, string applicationPath)
        {
			var httpResponse = MockRepository.GenerateStub<HttpResponseBase>();
			httpResponse.Stub(x => x.ApplyAppPathModifier("~/")).Return(applicationPath);
			var httpRequest = MockRepository.GenerateStub<HttpRequestBase>();
			httpRequest.Stub(x => x.Url).Return(new Uri(url));
			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			var httpContext = MockRepository.GenerateStub<HttpContextBase>();
			httpContext.Stub(x => x.Response).Return(httpResponse);
			httpContext.Stub(x => x.Request).Return(httpRequest);
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);
			return currentHttpContext;
        }

	    [Test]
	    public void ShouldReturnPersonId()
	    {
		    IPerson person = new Person();
		    Guid? personId = Guid.NewGuid();
            IIdentityLogon identityLogon = new FakeAuthenticator();
		    person.SetId(personId);
			 const string url = "http://my.url.com";
			 const string applicationPath = "/TeleoptiCCC/Web/";
		    System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
					 new TeleoptiIdentity("test", null, null, null, null), person );
			 IAuthenticationModule authenticationModule = new TeleoptiPrincipalAuthorizeAttributeTest.FakeAuthenticationModule();
             var target = new UrlController(CurrentHttpContext(url, applicationPath), authenticationModule, identityLogon, new SignatureCreator());
		    target.AuthenticationDetails().Should().Be.Equals(personId);
	    }

		 [Test]
		 public void ShouldRedirectToHomePage()
		 {
			 IPerson person = new Person();
			 Guid? personId = Guid.NewGuid();
			 IIdentityLogon identityLogon = new FakeAuthenticator();
			 person.SetId(personId);
			 const string url = "http://my.url.com/start/Url/RedirectToWebLogin";
			 const string applicationPath = "/TeleoptiCCC/Web/";
			 System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
					 new TeleoptiIdentity("test", null, null, null, null), person);
			 IAuthenticationModule authenticationModule = new TeleoptiPrincipalAuthorizeAttributeTest.FakeAuthenticationModule();
			 var target = new UrlController(CurrentHttpContext(url, applicationPath), authenticationModule, identityLogon, new SignatureCreator());
			 var result = ((RedirectResult) target.RedirectToWebLogin());
			 result.Url.Should().Not.Contain("start/Url/RedirectToWebLogin");
		 }
    }
}