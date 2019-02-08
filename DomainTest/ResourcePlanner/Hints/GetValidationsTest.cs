using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Hints
{
	[DomainTest]
	[NoDefaultData]
	public class GetValidationsTest
	{
		public GetValidations Target;
		public FakePlanningPeriodRepository PlanningPeriodRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakeExistingForecastRepository SkillRepository;
		
		[Test]
		public void ShouldNotCrashDueToNoSchedulesLoaded()
		{
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, 1);
			ScenarioRepository.Has();
			PersonRepository.Has(new Person().WithId().WithPersonPeriod().WithSchedulePeriodOneDay(DateOnly.Today));
			
			Assert.DoesNotThrow(() => { Target.Execute(planningPeriod.Id.Value); });
		}
		
		[Test]
		public void ShouldGroupByResourceTypeAndOrderByResourceName()
		{
			var skillMissingForecast = new SkillMissingForecast()
			{
				SkillName = "b",
				SkillId = Guid.NewGuid(),
				Periods = new []{new DateOnlyPeriod(2018,1,1,2018,2,1) }
			};
			var skill = new Skill(skillMissingForecast.SkillName).WithId(skillMissingForecast.SkillId);
			
			var person1 = PersonRepository.Has(skill);
			person1.SetName(new Name("a","a"));
			var person2=PersonRepository.Has();
			person2.SetName(new Name("z","z"));
			
			var planningPeriod = PlanningPeriodRepository.Has(DateOnly.Today, 1);
			ScenarioRepository.Has();
			PersonRepository.Has(new []{person1, person2});
			
			SkillRepository.CustomResult=new []{skillMissingForecast};
			
			var hintResult = Target.Execute(planningPeriod.Id.Value);

			hintResult.InvalidResources.First().ResourceName.Should().Be.EqualTo("a a");
			hintResult.InvalidResources.Last().ResourceName.Should().Be.EqualTo("b");
		}
	}
}