using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
	[TestFixture]
	public class UnsafePersonProviderTest
	{
		private IUnsafePersonProvider target;
		private ITeleoptiPrincipal beforePrincipal;
		private IPerson currentLoggedOnPerson;

		[SetUp]
		public void Setup()
		{
			target = new UnsafePersonProvider();
			beforePrincipal = TeleoptiPrincipal.CurrentPrincipal;
			currentLoggedOnPerson = new Person();
			var currentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity("test", null, null, null, null), currentLoggedOnPerson);
			Thread.CurrentPrincipal = currentPrincipal;
		}

		[Test]
		public void ShouldReturnCurrentUser()
		{
			target.CurrentUser()
				.Should().Be.SameInstanceAs(currentLoggedOnPerson);
		}

		[TearDown]
		public void Teardown()
		{
			Thread.CurrentPrincipal = beforePrincipal;
		}
	}
}