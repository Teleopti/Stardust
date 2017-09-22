using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[IoCTest]
	public class UserUiCultureTest : ISetup
	{
		public IUserUiCulture Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(1053));
			var principal = new TeleoptiPrincipal(new TeleoptiIdentity("name", null, null, null, null), person);
			system.UseTestDouble(new FakeCurrentTeleoptiPrincipal(principal)).For<ICurrentTeleoptiPrincipal>();
		}

		[Test]
		public void ShouldGetCultureFromPrincipal()
		{
			var result = Target.GetUiCulture();

			result.LCID.Should().Be.EqualTo(1053);
		}
	}
}