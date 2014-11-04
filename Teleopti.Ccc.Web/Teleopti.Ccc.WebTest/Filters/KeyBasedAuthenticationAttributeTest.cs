using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.WebTest.Filters
{
	[TestFixture]
	public class KeyBasedAuthenticationAttributeTest
	{
		 
		[Test]
		public async void ShouldReturnClaimOnCorrectPassword()
		{
			var target = new KeyBasedAuthenticationAttribute();
			
			var request = new HttpRequestMessage();
			request.Headers.Authorization = createBasicCredentials("Teleopti WFM", "!#¤atAbgT%");
			var httpActionContext =
				new HttpActionContext(
					new HttpControllerContext { ControllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(object) }, Request = request },
					new CustomHttpActionDescriptorForTest(new HttpControllerDescriptor { ControllerType = typeof(object) }));
			var httpAuthenticationContext = new HttpAuthenticationContext(httpActionContext, null);
			await target.AuthenticateAsync(httpAuthenticationContext, new CancellationToken());

			httpAuthenticationContext.Principal.Should().Not.Be.Null();
		}

		[Test]
		public async void ShouldNotReturnClaimOnInCorrectPassword()
		{
			var target = new KeyBasedAuthenticationAttribute();

			var request = new HttpRequestMessage();
			request.Headers.Authorization = createBasicCredentials("Teleopti WFM", "InCorrect");
			var httpActionContext =
				new HttpActionContext(
					new HttpControllerContext
					{
						ControllerDescriptor = new HttpControllerDescriptor {ControllerType = typeof (object)},
						Request = request
					},
					new CustomHttpActionDescriptorForTest(new HttpControllerDescriptor {ControllerType = typeof (object)}));
			var httpAuthenticationContext = new HttpAuthenticationContext(httpActionContext, null);
			await target.AuthenticateAsync(httpAuthenticationContext, new CancellationToken());

			httpAuthenticationContext.Principal.Should().Be.Null();
			httpAuthenticationContext.ErrorResult.Should().Be.OfType<AuthenticationFailureResult>();
		}

		private static AuthenticationHeaderValue createBasicCredentials(string userName, string password)
		{
			string toEncode = userName + ":" + password;
			//we use UTF8 because of the strange characters
			Encoding encoding = Encoding.UTF8;
			byte[] toBase64 = encoding.GetBytes(toEncode);
			string parameter = Convert.ToBase64String(toBase64);

			return new AuthenticationHeaderValue("Basic", parameter);
		}
	}

}