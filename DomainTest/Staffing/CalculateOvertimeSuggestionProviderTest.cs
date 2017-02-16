﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
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
	[Toggle(Toggles.Staffing_ReadModel_UseSkillCombination_42663), Toggle(Toggles.StaffingActions_UseRealForecast_42663)]
	public class CalculateOvertimeSuggestionProviderTest : ISetup
	{
		public CalculateOvertimeSuggestionProvider Target;
		public FakeSkillRepository SkillRepository;
		public FakeActivityRepository ActivityRepository;
		public MutableNow Now;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		//public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonForOvertimeProvider PersonForOvertimeProvider;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;

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
			//var period = new DateTimePeriod(now, now.AddHours(2));
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

			var intervals = Target.GetOvertimeSuggestions(new List<Guid> {skill.Id.GetValueOrDefault()}, now.AddHours(1), now.AddHours(2));
			intervals.FirstOrDefault(x => x.StartDateTime == now.AddHours(1)).CalculatedResource.Should().Be.EqualTo(2);
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonForOvertimeProvider>().For<IPersonForOvertimeProvider>();
		}
	}
}
