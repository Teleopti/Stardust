using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.DomainTest.ResourceCalculation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestWithStaticDependenciesDONOTUSE]
	[TestFixture, SetCulture("en-US")]
	public class MultiAbsenceRequestsUpdaterWithValidatorTest : IIsolateSystem
	{
		public IMultiAbsenceRequestsUpdater Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public MutableNow Now;
		public FakeSchedulingResultStateHolder_DoNotUse SchedulingResultStateHolder;
		public FakeLoggedOnUser LoggedOnUser;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeSchedulingResultStateHolder_DoNotUse>().For<ISchedulingResultStateHolder>();
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
		}

		[Test]
		public void ShouldPendingAndOnlyValidateStaffingThresholdValidator()
		{
			var personRequest = updateAbsenceRequestWithStaffingThresholdValidator(-0.15d);
			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Trim().Should().Contain(Resources.InsufficientStaffingDays);
		}

		[Test]
		public void ShouldApproveAndOnlyValidateStaffingThresholdValidator()
		{
			LoggedOnUser.SetFakeLoggedOnUser(PersonFactory.CreatePerson());
			var personRequest = updateAbsenceRequestWithStaffingThresholdValidator(-0.05d);
			personRequest.IsApproved.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Should().Be(null);
		}

		[Test]
		public void ShouldLoadResourcesAndOnlyValidateStaffingThresholdValidator()
		{
			var skill = SkillFactory.CreateSkillWithId("skill1");
			SkillRepository.Add(skill);
			var personRequest = updateAbsenceRequestWithStaffingThresholdValidator(-0.15d);
			personRequest.IsPending.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Trim().Should().Contain(Resources.InsufficientStaffingDays);
			Assert.IsTrue(SchedulingResultStateHolder.SkillDays.Count > 0);
		}

		private PersonRequest updateAbsenceRequestWithStaffingThresholdValidator(double relativeDifference)
		{
			Now.Is(new DateTime(2016, 12, 1, 10, 0, 0));
			var scenario = ScenarioRepository.Has("scenario");
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.Tracker = Tracker.CreateDayTracker();

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});

			var person = PersonFactory.CreatePerson(wfcs).WithId();
			var activity = ActivityRepository.Has("activity");
			var period = new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 1, 13);
			PersonAssignmentRepository.Has(PersonAssignmentFactory.CreateAssignmentWithMainShift(person
				, scenario, activity, period, new ShiftCategory("category")));

			var skill = createSkillWithOpenHours();
			setPersonPeriodWithSkill(person, skill);
			setSkillStaffPeriodHolder(skill, relativeDifference);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, period)).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			Target.UpdateAbsenceRequest(new List<Guid> {personRequest.Id.GetValueOrDefault()},
				new Dictionary<Guid, IEnumerable<IAbsenceRequestValidator>>
				{
					[personRequest.Id.GetValueOrDefault()] = new[] {new StaffingThresholdValidator()}
				});
			return personRequest;
		}

		private static ISkill createSkillWithOpenHours()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			foreach (var workload in skill.WorkloadCollection)
			{
				foreach (var templateWeek in workload.TemplateWeekCollection)
				{
					templateWeek.Value.ChangeOpenHours(new List<TimePeriod>
					{
						new TimePeriod(8, 0, 17, 0)
					});
				}
			}
			return skill;
		}

		private static void setPersonPeriodWithSkill(IPerson person, ISkill skill)
		{
			var periodDateOnly = new DateOnly(2016, 1, 1);
			var personPeriod = PersonPeriodFactory.CreatePersonPeriodWithSkillsWithSite(periodDateOnly, skill);
			person.AddPersonPeriod(personPeriod);
		}

		private void setSkillStaffPeriodHolder(ISkill skill, double relativeDifference)
		{
			var skillDateTimePeriod = new DateTimePeriod(2016, 1, 1, 2016, 12, 31);
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();
			skillStaffPeriod.Stub(x => x.RelativeDifference).Return(relativeDifference);
			skillStaffPeriod.Stub(x => x.Period).Return(skillDateTimePeriod);
			skillStaffPeriod.Stub(x => x.DateTimePeriod).Return(skillDateTimePeriod);

			var skillStaffPeriodHolder = new FakeSkillStaffPeriodHolder();
			skillStaffPeriodHolder.SetDictionary(new SkillSkillStaffPeriodExtendedDictionary { { skill, new SkillStaffPeriodDictionary(skill) { skillStaffPeriod } } });
			SchedulingResultStateHolder.SetSkillStaffPeriodHolder(skillStaffPeriodHolder);
		}
	}
}
