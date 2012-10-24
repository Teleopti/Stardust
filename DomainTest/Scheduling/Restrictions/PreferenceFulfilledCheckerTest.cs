using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors"), TestFixture]
	public class PreferenceFulfilledCheckerTest
	{
		[Test]
		public static void ShouldFulfilled()
		{
			var restrictionChecker = MockRepository.GenerateMock<ICheckerRestriction>();
			restrictionChecker.Stub(x => x.CheckPreference()).Return(PermissionState.Satisfied);

			var target = new PreferenceFulfilledChecker(restrictionChecker);

			target.IsPreferenceFulfilled(null).Should().Be.EqualTo(true);
		}

		[Test]
		public static void ShouldNotFulfilled()
		{
			var restrictionChecker = MockRepository.GenerateMock<ICheckerRestriction>();
			restrictionChecker.Stub(x => x.CheckPreference()).Return(PermissionState.Broken);

			var target = new PreferenceFulfilledChecker(restrictionChecker);

			target.IsPreferenceFulfilled(null).Should().Be.EqualTo(false);
		}

		[Test]
		public static void ShouldReturnNull()
		{
			var restrictionChecker = MockRepository.GenerateMock<ICheckerRestriction>();
			restrictionChecker.Stub(x => x.CheckPreference()).Return(PermissionState.Unspecified);

			var target = new PreferenceFulfilledChecker(restrictionChecker);

			target.IsPreferenceFulfilled(null).Should().Be.EqualTo(null);
		}
	}
}