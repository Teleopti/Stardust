using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.OvertimeScheduling
{
	[TestFixture(true)]
	[TestFixture(false)]
	public abstract class OvertimeSchedulingScenario : IConfigureToggleManager
	{
		protected readonly bool _resourcePlannerOvertimeNightShifts44311;

		protected OvertimeSchedulingScenario(bool resourcePlannerOvertimeNightShifts44311)
		{
			_resourcePlannerOvertimeNightShifts44311 = resourcePlannerOvertimeNightShifts44311;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerOvertimeNightShifts44311)
				toggleManager.Enable(Toggles.ResourcePlanner_OvertimeNightShifts_44311);
		}
	}
}