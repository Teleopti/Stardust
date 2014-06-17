using System.Collections.ObjectModel;
using System.Web;
using Microsoft.IdentityModel.Claims;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.Authentication.DataProvider
{
	[TestFixture]
	public class TokenIdentityProviderTest
	{
		private TokenIdentityProvider target;

		private HttpContextBase httpContext;

		[SetUp]
		public void Setup()
		{
			httpContext = MockRepository.GenerateStub<HttpContextBase>();

			var currentHttpContext = MockRepository.GenerateMock<ICurrentHttpContext>();
			currentHttpContext.Stub(x => x.Current()).Return(httpContext);

			target = new TokenIdentityProvider(currentHttpContext); 
		}

		[Test]
		public void ShouldExtractWindowsAccountInformationFromClaimIdentity()
		{
			httpContext.User =
				new ClaimsPrincipal(
					new ClaimsIdentityCollection(new Collection<IClaimsIdentity>
						{
							new ClaimsIdentity(new[] {new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/TOPTINET#kunningm")})
						}));
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo(@"TOPTINET\kunningm");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo(@"http://fakeschema.com/TOPTINET#kunningm");
		}

		[Test]
		public void ShouldExtractWindowsAccountInformationFromTeleoptiIdentity()
		{
			httpContext.User =
				new TeleoptiPrincipal(new TeleoptiIdentity("", null, null, null, "http://fakeschema.com/TOPTINET#kunningm"), null);
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo(@"TOPTINET\kunningm");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo(@"http://fakeschema.com/TOPTINET#kunningm");
		}

		[Test]
		public void ShouldExtractApplicationAccountInformationFromClaimIdentity()
		{
			httpContext.User =
				new ClaimsPrincipal(
					new ClaimsIdentityCollection(new Collection<IClaimsIdentity>
						{
							new ClaimsIdentity(new[] {new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/kunningm@@Teleopti WFM")})
						}));
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo("kunningm");
			target.RetrieveToken().DataSource.Should().Be.EqualTo("Teleopti WFM");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo("http://fakeschema.com/kunningm@@Teleopti WFM");
		}

		[Test]
		public void ShouldReturnNullIfNoNameIdentifier()
		{
			httpContext.User =
				new ClaimsPrincipal(
					new ClaimsIdentityCollection(new Collection<IClaimsIdentity>
						{
							new ClaimsIdentity(new[] {new Claim(ClaimTypes.Country, "http://fakeschema.com/kunningm#TOPTINET")})
						}));

			target.RetrieveToken().Should().Be.Null();
		}
	}
}