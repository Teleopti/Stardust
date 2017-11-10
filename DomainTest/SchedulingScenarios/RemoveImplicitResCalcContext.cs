using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RemoveImplicitResCalcContext_46680)]
	public enum RemoveImplicitResCalcContext
	{
		RemoveImplicitResCalcContextTrue,
		RemoveImplicitResCalcContextFalse
	}
}