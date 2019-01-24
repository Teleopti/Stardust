using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	public class PlanningGroupTest
	{
		[Test]
		public void ShouldRemoveNonDefaultPlanningGroupSetting()
		{
			var planningGroup = new PlanningGroup();
			var setting = new PlanningGroupSettings();
			planningGroup.AddSetting(setting);
			planningGroup.RemoveSetting(setting);

			planningGroup.Settings.Where(x=>!x.Default).Should().Be.Empty();
		}
		
		[Test]
		public void CannotRemoveDefaultPlanningGroupSetting()
		{
			var planningGroup = new PlanningGroup();
			Assert.Throws<ArgumentException>(() => planningGroup.RemoveSetting(planningGroup.Settings.Single(x=>x.Default)));
		}

		[Test]
		public void ShouldAddDefaultPlanningGroupSettingWhenCreatingPlanningGroup()
		{
			var planningGroup = new PlanningGroup();
			planningGroup.Settings.Single().Default.Should().Be.True();
		}
		
		[Test]
		public void CannotAddDefaultPlanningGroupSetting()
		{
			var planningGroup = new PlanningGroup();
			var anotherDefaultSetting = new PlanningGroupSettings();
			anotherDefaultSetting.SetAsDefault();
			Assert.Throws<ArgumentException>(() => planningGroup.AddSetting(anotherDefaultSetting));
		}

		[Test]
		public void ShouldHaveCorrectDefaultPreferenceValue()
		{
			var planningGroup = new PlanningGroup();
			planningGroup.Settings.PreferenceValue.Should().Be.EqualTo(new Percent(0.8));
		}
		
		[Test]
		public void ShouldHaveCorrectDefaultTeamSettings()
		{
			var planningGroup = new PlanningGroup();
			planningGroup.Settings.TeamSettings.GroupPageType.Should().Be.EqualTo(GroupPageType.SingleAgent);
		}
	}
}