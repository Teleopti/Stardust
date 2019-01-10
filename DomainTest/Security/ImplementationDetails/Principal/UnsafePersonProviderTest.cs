using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Principal
{
	[TestFixture]
	public class UnsafePersonProviderTest
	{
		private IUnsafePersonProvider target;
		private IPerson currentLoggedOnPerson;

		[SetUp]
		public void Setup()
		{
			target = new UnsafePersonProvider();
			currentLoggedOnPerson = new Person();
			var currentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("test", null, null, null, null), currentLoggedOnPerson);
			Thread.CurrentPrincipal = currentPrincipal;
		}

		[TearDown]
		public void Teardown()
		{
			Thread.CurrentPrincipal = null;
		}

		[Test]
		public void ShouldReturnCurrentUser()
		{
			target.CurrentUser()
				.Should().Be.SameInstanceAs(currentLoggedOnPerson);
		}

	}
}