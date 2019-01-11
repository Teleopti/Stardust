using System.Security.Claims;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.WebTest.Core.Authentication.DataProvider
{
	[TestFixture]
	public class TokenIdentityProviderTest
	{
		[Test]
		public void ShouldExtractWindowsAccountInformationFromClaimIdentity()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User =
				new ClaimsPrincipal(
					new[]
					{
						new ClaimsIdentity(new[]
						{
							new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/TOPTINET#kunning$$$m"),
						})
					});
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo(@"TOPTINET\kunning.m");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo(@"http://fakeschema.com/TOPTINET#kunning$$$m");
		}

		[Test]
		public void ShouldExtractWindowsAccountInformationFromTeleoptiIdentity()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User =
				new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("", null, null, null, "http://fakeschema.com/TOPTINET#kunningm"), null);
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo(@"TOPTINET\kunningm");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo(@"http://fakeschema.com/TOPTINET#kunningm");
		}

		[Test]
		public void ShouldExtractApplicationAccountInformationFromClaimIdentity()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User =
				new ClaimsPrincipal(new[]
				{
					new ClaimsIdentity(new[]
					{
						new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier),
					})
				});
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo("kunningm");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo("http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier);
		}

		[Test]
		public void ShouldExtractApplicationAccountInformationFromTeleoptiIdentity()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("", null, null, null, "http://fakeschema.com/kunningm@"), null);
			target.RetrieveToken().UserIdentifier.Should().Be.EqualTo("kunningm");
			target.RetrieveToken().OriginalToken.Should().Be.EqualTo("http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier);
		}

		[Test]
		public void ShouldExtractIsPersistentFromClaimIdentity()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User =
				new ClaimsPrincipal(
					new[]
					{
						new ClaimsIdentity(new[]
						{
							new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier),
							new Claim(ClaimTypes.IsPersistent, "true")
						})
					});
			target.RetrieveToken().IsPersistent.Should().Be.True();
		}

		[Test]
		public void ShouldSetIsPersistentToTrueIfOtherAuthenticationMethod()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User =
				new ClaimsPrincipal(
					new[]
					{
						new ClaimsIdentity(new[]
						{
							new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier)
						})
					});
			target.RetrieveToken().IsPersistent.Should().Be.True();
		}

		[Test]
		public void ShouldSetIsPersistentToFalse()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User =
				new ClaimsPrincipal(
					new[]
					{
						new ClaimsIdentity(new[]
						{
							new Claim(ClaimTypes.NameIdentifier, "http://fakeschema.com/kunningm" + TokenIdentityProvider.ApplicationIdentifier),
							new Claim(ClaimTypes.IsPersistent, "false")
						})
					});
			target.RetrieveToken().IsPersistent.Should().Be.False();
		}

		[Test]
		public void ShouldReturnNullIfNoNameIdentifier()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User =
				new ClaimsPrincipal(
					new[]
					{
						new ClaimsIdentity(new[] {new Claim(ClaimTypes.Country, "http://fakeschema.com/kunningm#TOPTINET")})
					});

			target.RetrieveToken().Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfNoTokenForTeleoptiIdentity()
		{
			var httpContext = new FakeHttpContext();
			var target = new TokenIdentityProvider(new FakeCurrentHttpContext(httpContext));

			httpContext.User = new TeleoptiPrincipalForLegacy(new TeleoptiIdentity("asdf",null,null,null,null),null);

			target.RetrieveToken().Should().Be.Null();
		}
	}
}