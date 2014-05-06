using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.FeatureFlags
{
	public class ToggleUserProviderTest
	{
		[Test]
		public void ShouldGetCurrentUserIdAsString()
		{
			var currentUser = new Person();
			currentUser.SetId(Guid.NewGuid());
			var loggedOnUser = MockRepository.GenerateStub<ILoggedOnUser>();
			loggedOnUser.Expect(x => x.CurrentUser()).Return(currentUser);

			var target = new ToggleUserProvider(loggedOnUser);
			target.CurrentUser()
				.Should().Be.EqualTo(currentUser.Id.Value.ToString());
		}
	}
}