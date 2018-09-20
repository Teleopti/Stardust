using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[TestFixture(false)]
	[TestFixture(true)]
	public abstract class ResourceCalculationScenario : IConfigureToggleManager, IExtendSystem
	{
		private readonly bool _resourcePlannerHalfHourSkillTimeZon75509;

		protected ResourceCalculationScenario(bool resourcePlannerHalfHourSkillTimeZon75509)
		{
			_resourcePlannerHalfHourSkillTimeZon75509 = resourcePlannerHalfHourSkillTimeZon75509;
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if (_resourcePlannerHalfHourSkillTimeZon75509)
				toggleManager.Enable(Toggles.ResourcePlanner_HalfHourSkillTimeZone_75509);
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ResourceCalculateWithNewContext>();
		}
	}
}