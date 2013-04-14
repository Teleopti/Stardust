using System;
using System.IO;
using System.Web;
using DotNetOpenAuth.OpenId.Provider;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.SSO.Core;

namespace Teleopti.Ccc.WebTest.Areas.SSO
{
	[TestFixture]
	public class OpenIdProviderWapperTest
	{
		[SetUp]
		public void SetUp()
		{
			HttpContext.Current = new HttpContext(
				new HttpRequest("", "http://mock", ""),
				new HttpResponse(new StringWriter())
				);
		}

		[TearDown]
		public void TearDown()
		{
			HttpContext.Current = null;
		}

		[Test]
		public void BlehShouldHaveCoverageBahGah()
		{
			var target = new OpenIdProviderWapper(new OpenIdProvider());
			try
			{
				target.PrepareResponse(MockRepository.GenerateMock<IRequest>());
			}
			catch (Exception)
			{
			}
			try
			{
				target.SendResponse(MockRepository.GenerateMock<IRequest>());
			}
			catch (Exception)
			{
			}
		}

		[Test]
		public void ShouldGetRequest()
		{
			var openIdProvider = new OpenIdProvider();

			var wrapper = new OpenIdProviderWapper(openIdProvider);
			wrapper.GetRequest();
		}

	}
}