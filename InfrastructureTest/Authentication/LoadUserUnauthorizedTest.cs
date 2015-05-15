using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Authentication;

namespace Teleopti.Ccc.InfrastructureTest.Authentication
{
	public class LoadUserUnauthorizedTest
	{
		[Test]
		public void ShouldLoadMyself()
		{
			var target = new LoadUserUnauthorized();
			target.LoadFullPersonInSeperateTransaction(SetupFixtureForAssembly.DataSource.Application,
				SetupFixtureForAssembly.loggedOnPerson.Id.Value)
				.Should().Be.EqualTo(SetupFixtureForAssembly.loggedOnPerson);
		}
	}
}