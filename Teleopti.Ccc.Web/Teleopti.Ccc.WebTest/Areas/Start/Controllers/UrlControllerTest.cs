using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Start.Controllers;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Ccc.WebTest.Filters;

namespace Teleopti.Ccc.WebTest.Areas.Start.Controllers
{
    [TestFixture]
    public class UrlControllerTest
    {
		private readonly SignatureCreator signatureCreator =
			new SignatureCreator(new FakeConfigReader(new Dictionary<string, string>
			{
				{
					"CertificateModulus",
					"tcQWMgdpQeCd8+gzB3rYQAehHXF5mBGdyFMkJMEmcQmTlkpg22xLNz/kNYXZ7j2Cuhls+PBORzZkfBsNoL1vErT+N9Es4EEWOt6ntNe7wujqQqktUT/QOWEMJ8zJQM3bn7Oj9H5StBr7DWSRzgEjOc7knDcb4KCQL3ceXqmqwSonPfP1hp+bE8rZuxDISYiZVEkm417YzUHBk3ppV30Q9zvfL9IZX0q/ebCTRnLFockl7yOVucomvo8j4ssFPCAYgASoNvzWq+s5UTzYELl1I7F3hQnFwx0bIpQFmGbZ5BbNczc6rVYtCX5KDMsVaJSUcXBAnqGd20hq/ICkBR658w=="
				},
				{"CertificateExponent", "AQAB"},
				{
					"CertificateP",
					"8r2FFhgc78WZf/uKEjHPyiLL9FkcjbPsdLB/Dd6AEOVuzpVFlBJsai31gyLIUU3zY6gE/NdMZzQ7ejsjhbpC4/ptbJguTpIOGB+7dX+/DEdwZkx8rIlNG32VDIdP6kqpwPzhtGVfNiq8xHaS+SHTQf6JSWQtNKgVbilWgYyEZ9k="
				},
				{
					"CertificateQ",
					"v7Hm0iP49ReGhVvKdAsDUgcK0olmVGAKwsxsFnXUGkAWydh+T3QaChYkYBS+h5cX4UBlil5FaJKtGq4wduKDkMCN8TNHh3n2k05rh4DxxPmLvhCqkQgvMB/22E+z10VAmjKPq7BnAq/lc2rGJWa1lq3qaeSkcF6agCPQVYd6vKs="
				},
				{
					"CertificateDP",
					"7mr7dvIEKfVZiX0U5j4Kq611yfBkvUHFs+9PO94Yx3+yUDIJfyCBX+D4Te8x9bmsn2t+SqFlJ9EDwlCn2UdTP/zO0WS/xuhp84PnaccpbPQWEER8CDNrit7UMNQOyD7BcQ5w2fDfjaJ4ejdEsHJqv100luNQC3I0alkr4F6WBjE="
				},
				{
					"CertificateDQ",
					"nZ2rSlGlm/BR/Ujx9+QuQL3lmiK7btjhQDZREU6krUjQ8/n8MVwnJO/7zLyBxH7pdZ47X0AQFeG0T2G2G6o3v0dz7kTZpX0Uzx4FsA7Hu8vrqMWPWVy/X/SIRGeUWYZpjd/Q3bxXlpAGO5Ypggsnd9NcEOGci4BdzMqlvA1/T60="
				},
				{
					"CertificateInverseQ",
					"4OVuc98gImxH9yLmvMimnr9zFxw6pwRd++/A7q1Uj8BrCKhzD8rY6QosNZCKlMjWdHNYtAWrDDqxwCRPUYhmdOm+JEkxBL7yelodmongNXUS0J9kf9c+k8fjgEsB02J15QqryyuRvw0Z638BheD9Ry3NN/REO0pdnj2LHxa17V0="
				},
				{
					"CertificateD",
					"YQOGoS8pc9rSE1OUoOJlN0+bI57kOlD0uO3/NYrN3Lkyx51tMtALGTMFt7d4SNsVwgQ+EGQaM5IJcd/ylx9kgESQBvSjEhJLLiKWukQG2BH+rpOjN2Fq3qU4mqmHpQn6tbNox98Af1aDNnO+Coi652jQxbv4Kh0ot9zJHddK5wuTxIQDLAxyb50f/ReG3UekxZoEOZEtj1oEd+QB/py+hP7Xp010Wfzy82g5Ec3ELjzeNPtijxmO+WExoF5zALIUYd8ClH+ayr2Ab3rD0Dv8VM1Y08npzSs6d5OOAAnG+245koiwkgJoXvZg0EVkcdJxZsMxIGL/OWEl82VnqC6mUQ=="
				}
			}));

		[Test]
        public void ShouldSignUrlCorrectly()
        {
			const string url = "http://my.url.com";
			const string applicationPath = "/TeleoptiCCC/Web/";
			
	        var signed = signatureCreator.Create(url + applicationPath);

			IAuthenticationModule authenticationModule = new TeleoptiPrincipalAuthorizeAttributeTest.FakeAuthenticationModule();
            IIdentityLogon identityLogon = new FakeAuthenticator();
	        var urlController = new UrlController(CurrentHttpContext(url, applicationPath),authenticationModule, identityLogon, signatureCreator, new Now());
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
		    Guid? personId = Guid.NewGuid();
            IIdentityLogon identityLogon = new FakeAuthenticator();
		    identityLogon.LogonIdentityUser().Person.SetId(personId);
			 const string url = "http://my.url.com";
			 const string applicationPath = "/TeleoptiCCC/Web/";
			 IAuthenticationModule authenticationModule = new TeleoptiPrincipalAuthorizeAttributeTest.FakeAuthenticationModule();
             var target = new UrlController(CurrentHttpContext(url, applicationPath), authenticationModule, identityLogon, signatureCreator, new Now());
		    ((JsonResult)target.AuthenticationDetails()).Data.ToString().Should().Contain(personId.ToString());
	    }

		[Test]
		public void ShouldHandleErrorWhenReturnPersonId()
		{
			const string url = "http://my.url.com";
			const string applicationPath = "/TeleoptiCCC/Web/";
			IIdentityLogon identityLogon = new FakeAuthenticator();
			identityLogon.LogonIdentityUser().Person = null;
			IAuthenticationModule authenticationModule = new TeleoptiPrincipalAuthorizeAttributeTest.FakeAuthenticationModule();
			var context = CurrentHttpContext(url, applicationPath);
			var target = new UrlController(context, authenticationModule, identityLogon, signatureCreator, new Now());
			((JsonResult)target.AuthenticationDetails()).Data.Should().Be.EqualTo("");
			context.Current().Response.StatusCode.Should().Be.EqualTo(401);
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
					 new TeleoptiIdentity("test", null, null, null, null, null), new PersonAndBusinessUnit(person, null));
			 IAuthenticationModule authenticationModule = new TeleoptiPrincipalAuthorizeAttributeTest.FakeAuthenticationModule();
			 var target = new UrlController(CurrentHttpContext(url, applicationPath), authenticationModule, identityLogon, signatureCreator, new Now());
			 var result = ((RedirectResult) target.RedirectToWebLogin(""));
			 result.Url.Should().Not.Contain("start/Url/RedirectToWebLogin");
		 }
    }
}