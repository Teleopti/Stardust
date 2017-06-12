using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388)]
	public class IntradayRequestProcessorWithSkillDaysTest : ISetup
	{
		public IntradayRequestProcessor Target;
		public FakePersonRepository PersonRepository;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeActivityRepository ActivityRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeCommandDispatcher CommandDispatcher;
		public IScheduleStorage ScheduleStorage;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldApproveRequestIfEnoughResourcesOnSkill()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillSetupHelper.CreateSkill(30, "skill", new TimePeriod(8, 0, 18, 00), false, activity);
			SkillRepository.Has(skill);

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			var period = new DateTimePeriod(2016, 12, 1, 8, 2016, 12, 1, 9);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
			{
				createSkillCombinationResource(period, new[] {skill.Id.GetValueOrDefault()}, 10)
			});

			SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(8, 0, 18, 00), false);
			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, period)).WithId();
			
			Target.Process(personRequest, period.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(ApproveRequestCommand));
			var skillCombinations =  SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
			skillCombinations.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
			skillCombinations.Resource.Should().Be.EqualTo(9);
			CollectionAssert.AreEqual(skillCombinations.SkillCombination, new[] {skill.Id.GetValueOrDefault()});
		}

		[Test]
		public void ShouldGetSkilldaysForCorrectDays()
		{
			Now.Is(new DateTime(2017, 5, 1, 22, 00, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillSetupHelper.CreateSkill(60, "skill", new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)), false, activity);
			skill.TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			SkillRepository.Has(skill);

			var agent = PersonRepository.Has(skill);
			var wfcs = new WorkflowControlSet().WithId();
			createWfcs(wfcs, absence);
			agent.WorkflowControlSet = wfcs;
			agent.PermissionInformation.SetDefaultTimeZone(skill.TimeZone);
			var period = new DateTimePeriod(2017, 5, 1, 21, 2017, 5, 2, 9);
			var requestPeriod = new DateTimePeriod(2017, 5, 1, 21, 2017, 5, 1, 23);

			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, scenario, activity, period, new ShiftCategory("category")));

			var periodDayOne = new DateTimePeriod(2017, 5, 1, 21, 2017, 5, 1, 22);
			var periodDayTwo1 = new DateTimePeriod(2017, 5, 1, 22, 2017, 5, 1, 23);
			var periodDayTwo2 = new DateTimePeriod(2017, 5, 1, 23, 2017, 5, 2, 0);

			SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), new[]
																				{
																					createSkillCombinationResource(periodDayOne,new [] {skill.Id.GetValueOrDefault()}, 100),
																					createSkillCombinationResource(periodDayTwo1,new [] {skill.Id.GetValueOrDefault()}, 1),
																					createSkillCombinationResource(periodDayTwo2,new [] {skill.Id.GetValueOrDefault()}, 1),
																					//createSkillCombinationResource(period1,new [] {skill3.Id.GetValueOrDefault(), skill.Id.GetValueOrDefault()}, 10),
																					//createSkillCombinationResource(period2,new [] {skill3.Id.GetValueOrDefault(), skill.Id.GetValueOrDefault()}, 10)
																				});


			var skillday = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime(), new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1)), false);
			var skillday2 = SkillSetupHelper.CreateSkillDay(skill, scenario, Now.UtcDateTime().AddDays(1), new TimePeriod(8, 0, 18, 00), false);
			SkillDayRepository.Has(new []{skillday, skillday2});

			var personRequest = new PersonRequest(agent, new AbsenceRequest(absence, requestPeriod)).WithId();

			Target.Process(personRequest, requestPeriod.StartDateTime);

			CommandDispatcher.LatestCommand.GetType().Should().Be.EqualTo(typeof(DenyRequestCommand));
			var skillCombinations = SkillCombinationResourceRepository.LoadSkillCombinationResources(period).First();
			skillCombinations.Resource.Should().Be.EqualTo(100);
			
		}


		private static void createWfcs(WorkflowControlSet wfcs, IAbsence absence)
		{
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
		}

		private static SkillCombinationResource createSkillCombinationResource(DateTimePeriod period1, Guid[] skillCombinations,double resource)
		{
			return new SkillCombinationResource
			{
				StartDateTime = period1.StartDateTime,
				EndDateTime = period1.EndDateTime,
				Resource = resource,
				SkillCombination = skillCombinations
			};
		}

		
		//private void skillCombAsserts(SkillCombinationResource skillCombinationResource, DateTimePeriod period, Guid[] skillCombinationResources, double resource)
		//{
		//	skillCombinationResource.StartDateTime.Should().Be.EqualTo(period.StartDateTime);
		//	skillCombinationResource.EndDateTime.Should().Be.EqualTo(period.EndDateTime);
		//	skillCombinationResource.Resource.Should().Be.EqualTo(resource);
		//	CollectionAssert.AreEqual(skillCombinationResource.SkillCombination, skillCombinationResources);
		//}
	}
}

	
