using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Gamification.core.DataProvider;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WebTest.Areas.Gamification.Core.DataProvider
{
	[TestFixture]
	class GamificationSettingPersisterTest
	{
		[Test]
		public void ShouldPersistNewGamification()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository);

			target.Persist();

			var result = gamificationSettingRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
		}
	}
}
