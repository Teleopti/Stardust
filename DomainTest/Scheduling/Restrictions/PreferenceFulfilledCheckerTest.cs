using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class PreferenceFulfilledCheckerTest
	{
		[Test]
		public void ShouldFulfilled()
		{
			var restrictionChecker = MockRepository.GenerateMock<ICheckerRestriction>();
			restrictionChecker.Stub(x => x.CheckPreference()).Return(PermissionState.Satisfied);

			var target = new PreferenceFulfilledChecker(restrictionChecker);

			target.IsPreferenceFulfilled(null).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldNotFulfilled()
		{
			var restrictionChecker = MockRepository.GenerateMock<ICheckerRestriction>();
			restrictionChecker.Stub(x => x.CheckPreference()).Return(PermissionState.Broken);

			var target = new PreferenceFulfilledChecker(restrictionChecker);

			target.IsPreferenceFulfilled(null).Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnNull()
		{
			var restrictionChecker = MockRepository.GenerateMock<ICheckerRestriction>();
			restrictionChecker.Stub(x => x.CheckPreference()).Return(PermissionState.Unspecified);

			var target = new PreferenceFulfilledChecker(restrictionChecker);

			target.IsPreferenceFulfilled(null).Should().Be.EqualTo(null);
		}
	}
}