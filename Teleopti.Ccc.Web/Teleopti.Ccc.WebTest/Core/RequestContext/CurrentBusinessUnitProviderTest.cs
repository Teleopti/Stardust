using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class CurrentBusinessUnitProviderTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			currentPrincipalProvider = MockRepository.GenerateMock<ICurrentPrincipalProvider>();
			target = new CurrentBusinessUnitProvider(currentPrincipalProvider);
		}

		#endregion

		private ICurrentBusinessUnitProvider target;
		private ICurrentPrincipalProvider currentPrincipalProvider;

		[Test]
		public void ShouldReturnCurrentBusinessUnit()
		{
			var businessUnit = MockRepository.GenerateMock<IBusinessUnit>();
			var teleoptiPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("hej", null, businessUnit, null, AuthenticationTypeOption.Unknown), new Person());

			currentPrincipalProvider.Expect(x => x.Current()).Return(teleoptiPrincipal);

			target.CurrentBusinessUnit()
				.Should().Be.SameInstanceAs(businessUnit);
		}

		[Test]
		public void ShouldReturnNullWhenCurrentPrincipalNotDefined()
		{
			currentPrincipalProvider.Expect(x => x.Current()).Return(null);

			target.CurrentBusinessUnit()
				.Should().Be.Null();
		}
	}
}