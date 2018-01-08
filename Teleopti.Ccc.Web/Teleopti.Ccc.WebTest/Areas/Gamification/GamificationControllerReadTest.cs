using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Gamification.Controller;

namespace Teleopti.Ccc.WebTest.Areas.Gamification
{
	[TestFixture]
	[GamificationTest]
	public class GamificationControllerReadTest
	{
		public FakeGamificationSettingRepository GamificationSettingRepository;
		public GamificationController Target;

		[Test]
		public void ShouldGetGamificationSetting()
		{
			const string expectedName = "Default";
			var gamificationSetting = new GamificationSetting(expectedName).WithId();
			GamificationSettingRepository.Add(gamificationSetting);
			
			var result = Target.LoadGamification(gamificationSetting.Id.Value);

			result.Name.Should().Be.EqualTo(expectedName);
		}

		[Test]
		public void ShouldReturnNUllWhenCannotFindGamificationSetting()
		{
			var result = Target.LoadGamification(Guid.NewGuid());

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetGamificationList()
		{
			var gs1 = new GamificationSetting("g1").WithId();
			var gs2 = new GamificationSetting("g2").WithId();
			
			GamificationSettingRepository.Add(gs1);
			GamificationSettingRepository.Add(gs2);
			
			var result = Target.LoadGamificationList();

			result.Count.Should().Be.EqualTo(2);
			result[0].Value.Should().Be.EqualTo(gs1.Description);
			result[1].Value.Should().Be.EqualTo(gs2.Description);
		}
	}
}
