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
		private IGamificationSetting _gamificationSetting;

		[SetUp]
		public void Setup()
		{
			_gamificationSetting = new GamificationSetting("newGamification");
			_gamificationSetting.WithId();
			_gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			_gamificationSettingRepository.Stub(x => x.Load(_gamificationSetting.Id.Value)).Return(_gamificationSetting);
		}

		[Test]
		public void ShouldPersistNewGamification()
		{
			var gamificationSettingRepository = new FakeGamificationSettingRepository();
			var target = new GamificationSettingPersister(gamificationSettingRepository);

			var vm = target.Persist();

			var result = gamificationSettingRepository.LoadAll();
			result.Count.Should().Be.EqualTo(1);
			vm.Id.HasValue.Should().Be.True();
		}

		[Test]
		public void ShouldPersistGamificationDescription()
		{
			var expactedDescription = new Description("modifiedDescription");
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.Description.Should().Not.Be.EqualTo(expactedDescription);

			var result = target.PersistDescription(new GamificationDescriptionViewMode()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedDescription
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Name.Should().Be.EqualTo(expactedDescription.Name);
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsEnabled()
		{
			var expactedResult = true;
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AnsweredCallsBadgeEnabled.Should().Be.False();

			var result = target.PersistAnsweredCallsEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsGoldThreshold()
		{
			var expactedResult = 150;
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AnsweredCallsGoldThreshold.Should().Not.Be.EqualTo(expactedResult);

			var result = target.PersistAnsweredCallsGoldThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsSilverThreshold()
		{
			var expactedResult = 110;
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AnsweredCallsSilverThreshold.Should().Not.Be.EqualTo(expactedResult);

			var result = target.PersistAnsweredCallsSilverThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

		[Test]
		public void ShouldPersistGamificationAnsweredCallsBronzeThreshold()
		{
			var expactedResult = 90;
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AnsweredCallsBronzeThreshold.Should().Not.Be.EqualTo(expactedResult);

			var result = target.PersistAnsweredCallsBronzeThreshold(new GamificationAnsweredCallsThresholdViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

		[Test]
		public void ShouldPersistGamificationAHTEnabled()
		{
			var expactedResult = true;
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AHTBadgeEnabled.Should().Be.False();

			var result = target.PersistAHTEnabled(new GamificationThresholdEnabledViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

		[Test]
		public void ShouldPersistGamificationAHTGoldThreshold()
		{
			TimeSpan expactedResult = new TimeSpan(0,2,30);
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AHTGoldThreshold.Should().Not.Be.EqualTo(expactedResult);

			var result = target.PersistAHTGoldThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

		[Test]
		public void ShouldPersistGamificationAHTSilverThreshold()
		{
			TimeSpan expactedResult = new TimeSpan(0,3,30);
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AHTSilverThreshold.Should().Not.Be.EqualTo(expactedResult);

			var result = target.PersistAHTSilverThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

		[Test]
		public void ShouldPersistGamificationAHTBronzeThreshold()
		{
			TimeSpan expactedResult = new TimeSpan(0,4,30);
			var target = new GamificationSettingPersister(_gamificationSettingRepository);
			_gamificationSetting.AHTBronzeThreshold.Should().Not.Be.EqualTo(expactedResult);

			var result = target.PersistAHTBronzeThreshold(new GamificationAHTThresholdViewModel()
			{
				GamificationSettingId = _gamificationSetting.Id.Value,
				Value = expactedResult
			});

			result.GamificationSettingId.Should().Be.EqualTo(_gamificationSetting.Id);
			result.Value.Should().Be.EqualTo(expactedResult);
		}

	}
}
