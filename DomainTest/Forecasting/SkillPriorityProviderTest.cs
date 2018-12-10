using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[DomainTest]
	[RemoveMeWithToggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312)]
	public class SkillPriorityProviderTest
	{
		public ISkillPriorityProvider Target;

		[Test]
		public void ShouldReturnActualValues()
		{
			var skill = new Skill("_") {Priority = 1, OverstaffingFactor = new Percent(.75)};
			var priority = Target.GetPriority(skill);
			priority.Should().Be.EqualTo(1);

			var overstaffingFactor = Target.GetOverstaffingFactor(skill);
			overstaffingFactor.Should().Be.EqualTo(new Percent(.75));
		}

		[Test]
		[Toggle(Toggles.ResourcePlanner_HideSkillPrioSliders_41312)]
		public void ShouldReturnNeutralValues()
		{
			var skill = new Skill("_") { Priority = 1, OverstaffingFactor = new Percent(.75) };
			var priority = Target.GetPriority(skill);
			priority.Should().Be.EqualTo(4);

			var overstaffingFactor = Target.GetOverstaffingFactor(skill);
			overstaffingFactor.Should().Be.EqualTo(new Percent(.5));
		}
	}
}