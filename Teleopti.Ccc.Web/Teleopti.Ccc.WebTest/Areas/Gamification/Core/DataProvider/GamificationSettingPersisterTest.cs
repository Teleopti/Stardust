using System;
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
		private IGamificationSettingRepository _gamificationSettingRepository;
		private Guid _gamificationSettingId;

		[SetUp]
		public void Setup()
		{
			IGamificationSetting gamificationSetting = new GamificationSetting("newGamification");
			gamificationSetting.WithId();
			_gamificationSettingId = gamificationSetting.Id.Value;
			_gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			_gamificationSettingRepository.Stub(x => x.Load(_gamificationSettingId)).Return(gamificationSetting);
		}

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

			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			var result = target.PersistDescription(new GamificationDescriptionViewMode()
			{
				GamificationSettingId = _gamificationSettingId,
				Value = expactedDescription
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSettingId);
			result.Value.Name.Should().Be.EqualTo(expactedDescription.Name);
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsEnabled()
		{
			var expactedResult = true;

			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			var result = target.PersistAnsweredCallsEnabled(new GamificationAnsweredCallsEnabledViewModel()
			{
				GamificationSettingId = _gamificationSettingId,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSettingId);
			result.Value.Should().Be.EqualTo(expactedResult);
		}
	}
}
