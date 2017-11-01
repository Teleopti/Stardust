using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

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

		[Test]
		public void ShouldPersistGamificationDescription()
		{
			var expactedDescription = new Description("modifiedDescription");
			IGamificationSetting gamificationSetting  = new GamificationSetting("newGamification");
			gamificationSetting.WithId();
			var gamificationId = gamificationSetting.Id.Value;
			var gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			gamificationSettingRepository.Stub(x => x.Load(gamificationId)).Return(gamificationSetting);

			var target = new GamificationSettingPersister(gamificationSettingRepository);
			var result = target.PersistDescription(new GamificationDescriptionViewMode()
			{
				GamificationSettingId = gamificationId,
				Value = expactedDescription
			});

			result.GamificationSettingId.Should().Be.EqualTo(gamificationId);
			result.Value.Name.Should().Be.EqualTo(expactedDescription.Name);
		}
	}
}
