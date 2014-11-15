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
			currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			target = new CurrentBusinessUnit(new CurrentIdentity(currentTeleoptiPrincipal));
		}

		#endregion

		private ICurrentBusinessUnit target;
		private ICurrentTeleoptiPrincipal currentTeleoptiPrincipal;

		[Test]
		public void ShouldReturnCurrentBusinessUnit()
		{
			var businessUnit = MockRepository.GenerateMock<IBusinessUnit>();
			var teleoptiPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("hej", null, businessUnit, null), new Person());

			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);

			target.Current()
				.Should().Be.SameInstanceAs(businessUnit);
		}

		[Test]
		public void ShouldReturnNullWhenCurrentPrincipalNotDefined()
		{
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(null);

			target.Current()
				.Should().Be.Null();
		}
	}
}