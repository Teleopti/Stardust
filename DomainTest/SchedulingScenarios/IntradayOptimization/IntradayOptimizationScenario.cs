using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	public abstract class IntradayOptimizationScenario : IConfigureToggleManager
	{
		private readonly bool _skillGroupDeleteAfterCalculation;
		private readonly bool _intradayIslands;
		private readonly bool _jumpOutWhenLargeGroupIsHalfOptimized;

		protected IntradayOptimizationScenario(bool skillGroupDeleteAfterCalculation, bool intradayIslands, bool jumpOutWhenLargeGroupIsHalfOptimized)
		{
			_skillGroupDeleteAfterCalculation = skillGroupDeleteAfterCalculation;
			_intradayIslands = intradayIslands;
			_jumpOutWhenLargeGroupIsHalfOptimized = jumpOutWhenLargeGroupIsHalfOptimized;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_intradayIslands)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_IntradayIslands_36939);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_IntradayIslands_36939);
			}
			if (_skillGroupDeleteAfterCalculation)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_SkillGroupDeleteAfterCalculation_37048);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_SkillGroupDeleteAfterCalculation_37048);
			}
			if (_jumpOutWhenLargeGroupIsHalfOptimized)
			{
				toggleManager.Enable(Toggles.ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049);
			}
			else
			{
				toggleManager.Disable(Toggles.ResourcePlanner_JumpOutWhenLargeGroupIsHalfOptimized_37049);
			}
			//35043?
		}
	}
}