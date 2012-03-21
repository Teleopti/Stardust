using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class CultureProviderTest
	{
		private static TeleoptiPrincipal SetupTeleoptiPrincipal(int testLcid)
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(testLcid));
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(testLcid));
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("name", null, null, null, AuthenticationTypeOption.Unknown), person);
			return principal;
		}


		[Test, SetUICulture("en-US"), SetCulture("en-US")]
		public void ShouldBeAbleToGetDefaultForUnauthenticated()
		{
			var currentPrincipalProvider = MockRepository.GenerateStub<ICurrentPrincipalProvider>();
			currentPrincipalProvider.Stub(p => p.Current()).Return(null);

			var target = new CultureProvider(currentPrincipalProvider);

			var result = target.GetCulture();

			result.LCID.Should().Be.EqualTo(1033);
		}

		[Test, SetUICulture("en-US"), SetCulture("en-US")]
		public void ShouldGetCultureFromPrincipalIfPresent()
		{
			const int testLcid = 1053;

			var principal = SetupTeleoptiPrincipal(testLcid);

			var currentPrincipalProvider = MockRepository.GenerateStub<ICurrentPrincipalProvider>();
			currentPrincipalProvider.Stub(p => p.Current()).Return(principal);

			var target = new CultureProvider(currentPrincipalProvider);

			var result = target.GetCulture();

			result.LCID.Should().Be.EqualTo(testLcid);
		}
	}
}