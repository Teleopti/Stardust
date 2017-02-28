using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_42663)]
	public class AddOverTimeTest
	{
		public IAddOverTime Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public MutableNow Now;

		[Test] 
		[Ignore("WIP")]//temporary test
		public void ShouldAddOneResourceToAllIntervals()
		{
			Now.Is("2017-02-08 07:00");
			var period = new DateTimePeriod(2017, 02, 08, 8, 2017, 02, 08, 9);
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skillA", activity).WithId();
			skill.DefaultResolution = 60;
			IntervalLengthFetcher.Has(60);
			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new List<SkillCombinationResource>
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = period.StartDateTime,
																					   EndDateTime = period.EndDateTime,
																					   Resource = 10,
																					   SkillCombination = new []{skill.Id.GetValueOrDefault()}
																				   }
																			   });
			ScheduleForecastSkillReadModelRepository.Persist(new List<SkillStaffingInterval>
															 {
																 new SkillStaffingInterval
																 {
																	 StartDateTime = period.StartDateTime,
																	 EndDateTime = period.EndDateTime,
																	 Forecast = 1,
																	 SkillId = skill.Id.GetValueOrDefault()
																 }
															 }, Now.UtcDateTime());

			SkillDayRepository.Has(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2016, 12, 19), 1));

			//var result = Target.GetSuggestion(new OverTimeSuggestionModel{SkillIds = new [] {skill.Id.GetValueOrDefault()}, TimeSerie = );
			//result.SuggestedStaffingWithOverTime.FirstOrDefault().Should().Be.EqualTo(11);
		}
	}
}
