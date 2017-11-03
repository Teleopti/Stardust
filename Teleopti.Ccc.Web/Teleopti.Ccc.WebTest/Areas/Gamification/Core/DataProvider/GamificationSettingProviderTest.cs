using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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
			gamificationSettingRepository.Stub(x => x.Load(id)).Return(gamificationSetting);
			var mapper = new GamificationSettingMapper();

			var target = new GamificationSettingProvider(gamificationSettingRepository, mapper);

			var result = target.GetGamificationSetting(id);

			result.Description.Name.Should().Be.EqualTo(expactedName);
		}
	}
}
