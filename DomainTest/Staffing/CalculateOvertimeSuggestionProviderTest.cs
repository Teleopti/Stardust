using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Intraday;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	[Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388)]
	public class CalculateOvertimeSuggestionProviderTest : ISetup
	{
		public CalculateOvertimeSuggestionProvider Target;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public MutableNow Now;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonForOvertimeProvider PersonForOvertimeProvider;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonForOvertimeProvider>().For<IPersonForOvertimeProvider>();
		}

		[Test]
		public void ShouldGiveSuggestionForOverTime()
		{
			var now = new DateTime(2017,02,15,8,0,0).Utc();
			Now.Is(now);
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("SkillA", activity);
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			PersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel>
										   {
											   new SuggestedPersonsModel
											   {
												   PersonId = agent.Id.GetValueOrDefault(),
												   End = now.AddHours(1),
												   TimeToAdd = 60
											   }
										   });

			SkillCombinationResourceRepository.PersistSkillCombinationResource(now, new List<SkillCombinationResource>
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = now,
																					   EndDateTime = now.AddHours(1),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(1),
																					   EndDateTime = now.AddHours(2),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemandPerHour(scenario, new DateOnly(now),TimeSpan.FromHours(1), new Tuple<int, TimeSpan> (9, TimeSpan.FromHours(2))));

			var model = Target.GetOvertimeSuggestions(new List<Guid> {skill.Id.GetValueOrDefault()}, now.AddHours(1), now.AddHours(2));
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(1)).CalculatedResource.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateOverTimeModels()
		{
			var now = new DateTime(2017, 02, 15, 8, 0, 0).Utc();
			Now.Is(now);
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("SkillA", activity);
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			PersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel>
										   {
											   new SuggestedPersonsModel
											   {
												   PersonId = agent.Id.GetValueOrDefault(),
												   End = now.AddHours(1),
												   TimeToAdd = 60
											   }
										   });

			SkillCombinationResourceRepository.PersistSkillCombinationResource(now, new List<SkillCombinationResource>
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = now,
																					   EndDateTime = now.AddHours(1),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(1),
																					   EndDateTime = now.AddHours(2),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemandPerHour(scenario, new DateOnly(now), TimeSpan.FromHours(1), new Tuple<int, TimeSpan>(9, TimeSpan.FromHours(2))));

			var model = Target.GetOvertimeSuggestions(new List<Guid> { skill.Id.GetValueOrDefault() }, now.AddHours(1), now.AddHours(2));
			model.OverTimeModels.First(x => x.StartDateTime == now.AddHours(1)).EndDateTime.Should().Be.EqualTo(now.AddHours(2));
			model.OverTimeModels.First(x => x.StartDateTime == now.AddHours(1)).ActivityId.Should().Be.EqualTo(activity.Id);
			model.OverTimeModels.First(x => x.StartDateTime == now.AddHours(1)).PersonId.Should().Be.EqualTo(agent.Id);

		}

		[Test]
		public void ShouldAddOverTimeAfterShiftsEnd()
		{
			var now = new DateTime(2017, 02, 15, 8, 0, 0).Utc();
			Now.Is(now);
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("SkillA", activity);
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			PersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel>
										   {
											   new SuggestedPersonsModel
											   {
												   PersonId = agent.Id.GetValueOrDefault(),
												   End = now.AddHours(1),
												   TimeToAdd = 60
											   }
										   });

			SkillCombinationResourceRepository.PersistSkillCombinationResource(now, new List<SkillCombinationResource>
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = now,
																					   EndDateTime = now.AddHours(1),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(1),
																					   EndDateTime = now.AddHours(2),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemandPerHour(scenario, new DateOnly(now), TimeSpan.FromHours(1), new Tuple<int, TimeSpan>(9, TimeSpan.FromHours(2))));

			var model = Target.GetOvertimeSuggestions(new List<Guid> { skill.Id.GetValueOrDefault() }, now, now.AddHours(2));
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now).CalculatedResource.Should().Be.EqualTo(1);
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(1)).CalculatedResource.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldOnlyAddShiftToUnderstaffedIntervals()
		{
			var now = new DateTime(2017, 02, 15, 8, 0, 0).Utc();
			Now.Is(now);
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("SkillA", activity);
			skill.DefaultResolution = 60;
			var agent = PersonRepository.Has(skill);
			PersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel>
										   {
											   new SuggestedPersonsModel
											   {
												   PersonId = agent.Id.GetValueOrDefault(),
												   End = now.AddHours(1),
												   TimeToAdd = 180
											   }
										   });

			SkillCombinationResourceRepository.PersistSkillCombinationResource(now, new List<SkillCombinationResource>
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = now,
																					   EndDateTime = now.AddHours(1),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(1),
																					   EndDateTime = now.AddHours(2),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(2),
																					   EndDateTime = now.AddHours(3),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });

			SkillDayRepository.Has(skill.CreateSkillDayWithDemandPerHour(scenario, new DateOnly(now), TimeSpan.FromHours(1), new Tuple<int, TimeSpan>(9, TimeSpan.FromHours(2))));

			var model = Target.GetOvertimeSuggestions(new List<Guid> { skill.Id.GetValueOrDefault() }, now, now.AddHours(3));
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now).CalculatedResource.Should().Be.EqualTo(1);
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(1)).CalculatedResource.Should().Be.EqualTo(2);
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(2)).CalculatedResource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldAddOnDifferentAgentsIfOkStaffedInbetweenTheirShiftsEnds()
		{
			var now = new DateTime(2017, 02, 15, 8, 0, 0).Utc();
			Now.Is(now);
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("Activity");
			var skill = SkillRepository.Has("SkillA", activity);
			skill.DefaultResolution = 60;
			var agent1 = PersonRepository.Has(skill);
			var agent2 = PersonRepository.Has(skill);
			var agent3 = PersonRepository.Has(skill);
			var agent4 = PersonRepository.Has(skill);
			PersonForOvertimeProvider.Fill(new List<SuggestedPersonsModel>
										   {
											   new SuggestedPersonsModel
											   {
												   PersonId = agent1.Id.GetValueOrDefault(),
												   End = now.AddHours(1),
												   TimeToAdd = 180
											   },
											   new SuggestedPersonsModel
											   {
												   PersonId = agent2.Id.GetValueOrDefault(),
												   End = now.AddHours(1),
												   TimeToAdd = 180
											   },
											   new SuggestedPersonsModel
											   {
												   PersonId = agent3.Id.GetValueOrDefault(),
												   End = now.AddHours(1),
												   TimeToAdd = 180
											   },
												new SuggestedPersonsModel
											   {
												   PersonId = agent4.Id.GetValueOrDefault(),
												   End = now.AddHours(3),
												   TimeToAdd = 180
											   }
										   });

			SkillCombinationResourceRepository.PersistSkillCombinationResource(now, new List<SkillCombinationResource>
																			   {
																				   new SkillCombinationResource
																				   {
																					   StartDateTime = now,
																					   EndDateTime = now.AddHours(1),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(1),
																					   EndDateTime = now.AddHours(2),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(2),
																					   EndDateTime = now.AddHours(3),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(3),
																					   EndDateTime = now.AddHours(4),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   },
																					new SkillCombinationResource
																				   {
																					   StartDateTime = now.AddHours(4),
																					   EndDateTime = now.AddHours(5),
																					   Resource = 1,
																					   SkillCombination = new[] {skill.Id.GetValueOrDefault()}
																				   }
																			   });
			
			var skillDay = skill.CreateSkillDayWithDemandOnIntervalWithSkillDataPeriodDuplicate(scenario, new DateOnly(now), 1, new Tuple<TimePeriod, double>(new TimePeriod(9, 10), 2), new Tuple<TimePeriod, double>(new TimePeriod(11, 12), 2));
			SkillDayRepository.Has(skillDay);

			var model = Target.GetOvertimeSuggestions(new List<Guid> { skill.Id.GetValueOrDefault() }, now, now.AddHours(5));

			model.SkillStaffingIntervals.First(x => x.StartDateTime == now).CalculatedResource.Should().Be.EqualTo(1);
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(1)).CalculatedResource.Should().Be.EqualTo(2);
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(2)).CalculatedResource.Should().Be.EqualTo(1);
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(3)).CalculatedResource.Should().Be.EqualTo(2);
			model.SkillStaffingIntervals.First(x => x.StartDateTime == now.AddHours(4)).CalculatedResource.Should().Be.EqualTo(1);
		}

		
	}
}
