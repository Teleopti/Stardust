using System.Collections.ObjectModel;
using System.Web;
using Microsoft.IdentityModel.Claims;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

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
							new ClaimsIdentity(new[] {new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/TOPTINET#kunning$$$m")})
						}));
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo(@"TOPTINET\kunning.m");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo(@"http://fakeschema.com/TOPTINET#kunning$$$m");
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
							new ClaimsIdentity(new[] {new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier)})
						}));
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo("kunningm");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo("http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier);
		}

		[Test]
		public void ShouldExtractIsPersistentFromClaimIdentity()
		{
			httpContext.User =
				new ClaimsPrincipal(
					new ClaimsIdentityCollection(new Collection<IClaimsIdentity>
						{
							new ClaimsIdentity(new[]
							{
								new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier),
								new Claim(ClaimTypes.IsPersistent, "true")
							})
						}));
			target.RetrieveToken().IsPersistent.Should().Be.True();
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