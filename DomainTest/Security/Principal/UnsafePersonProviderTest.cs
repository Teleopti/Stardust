using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Principal
{
	[TestFixture]
	public class UnsafePersonProviderTest
	{
		private IUnsafePersonProvider target;

		[SetUp]
		public void Setup()
		{
			target = new UnsafePersonProvider();
		}

		[Test]
		public void ShouldReturnCurrentUser()
		{
			target.CurrentUser()
				.Should().Be.SameInstanceAs(SetupFixtureForAssembly.loggedOnPerson);
		}
	}
}