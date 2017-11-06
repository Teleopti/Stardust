using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;

namespace Teleopti.Ccc.WebTest.Areas.Gamification.Core.DataProvider
{
	[TestFixture]
	class GamificationSettingProviderTest
	{
		[Test]
		public void ShouldGetGamificationSetting()
		{
			var id = Guid.NewGuid();
			var expactedName = "Default";
			var gamificationSetting = new GamificationSetting(expactedName);
			var gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			gamificationSettingRepository.Stub(x => x.Get(id)).Return(gamificationSetting);
			var mapper = new GamificationSettingMapper();

			var target = new GamificationSettingProvider(gamificationSettingRepository, mapper);
			var result = target.GetGamificationSetting(id);

			result.Description.Name.Should().Be.EqualTo(expactedName);
		}

		[Test]
		public void ShouldReturnNUllWhenCannotFindGamificationSetting()
		{
			var id = Guid.NewGuid();
			var gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			gamificationSettingRepository.Stub(x => x.Get(id)).Return(null);
			var mapper = new GamificationSettingMapper();

			var target = new GamificationSettingProvider(gamificationSettingRepository, mapper);
			var result = target.GetGamificationSetting(id);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetGamificationList()
		{
			var gs1 = new GamificationSetting("g1");
			gs1.WithId(Guid.NewGuid());
			var gs2 = new GamificationSetting("g2");
			gs2.WithId(Guid.NewGuid());
			var gamificationSettings = new List<IGamificationSetting>() { gs1, gs2};
			var gamificationSettingRepository = MockRepository.GenerateMock<IGamificationSettingRepository>();
			gamificationSettingRepository.Stub(x => x.LoadAll()).Return(gamificationSettings);
			var mapper = new GamificationSettingMapper();

			var target = new GamificationSettingProvider(gamificationSettingRepository, mapper);
			var result = target.GetGamificationList();

			result.Count.Should().Be.EqualTo(2);
			result[0].Value.Should().Be.EqualTo(gs1.Description);
			result[1].Value.Should().Be.EqualTo(gs2.Description);
		}
	}
}
