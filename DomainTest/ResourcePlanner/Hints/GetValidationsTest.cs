using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SeamlessPlanningForPreferences_76288)]
	public class GetValidationsTest
	{
		public GetValidations Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		
		[Test]
		public void ShouldNotCrashDueToNoSchedulesLoaded()
		{
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, 1);
			ScenarioRepository.Has();
			PersonRepository.Has(new Person().WithId().WithPersonPeriod().WithSchedulePeriodOneDay(DateOnly.Today));
			
			Assert.DoesNotThrow(() => { Target.Execute(planningPeriod.Id.Value); });
		}
	}
}