using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	public class ApplicationAuthenticationTest : DatabaseTest
	{
		[Test]
		public void LogonShouldSucced()
		{
			var target = new ApplicationAuthentication();
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("arne", "anka");
			PersistAndRemoveFromUnitOfWork(person);

			target.TryAuthenticate("arne", "anka").Result
				.Should().Be.EqualTo(LogonResult.Success);
		}
	}
}