using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.Sdk.WcfHost.Ioc;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
	[DomainTest]
    public class SchedulePartAssemblerTest : IIsolateSystem, IExtendSystem
	{
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeMeetingRepository MeetingRepository;
		public IScheduleStorage ScheduleStorage;

		public ISchedulePartAssembler Target;

		[Test]
	    public void PayrollSpecialProjection()
	    {
		    var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);
			
		    var act = new Activity("sdf").WithId();
			ActivityRepository.Has(act);

			var scenario = ScenarioRepository.Has("Default");

			var ass = new PersonAssignment(person, scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000,1,1,12,2000,1,2,12));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			PersonAssignmentRepository.Add(ass);
			
		    Target.SpecialProjection = "midnightSplit";
		    Target.TimeZone = TimeZoneInfo.Utc;
			var dto = Target.DomainEntityToDto(ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario)[person]
				.ScheduledDay(new DateOnly(2000, 1, 1)));
		    dto.ProjectedLayerCollection.Count.Should().Be.EqualTo(2);
	    }
		
	    [Test]
        public void VerifyDtoToDo()
        {
			Target.TimeZone = TimeZoneInfo.Utc;
			
			DateOnly date = new DateOnly(1900,1,1);
            SchedulePartDto dto = new SchedulePartDto
                                      {
										  Date = new DateOnlyDto { DateTime = date.Date },
                                          TimeZoneId = TimeZoneInfo.Utc.Id,
                                          PersonId = Guid.NewGuid(),
                                          PersonDayOff = new PersonDayOffDto()
                                      };

		    Assert.Throws<NotSupportedException>(() => Target.DtoToDomainEntity(dto));
        }

        [Test]
        public void VerifyDoToDtoWithNullValue()
        {
			Target.TimeZone = TimeZoneInfo.Utc;
			
			Assert.IsNotNull(Target.DomainEntityToDto(null));
        }
		
        [Test]
        public void VerifyDoToDtoWithProjection()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var act = new Activity("sdf").WithId();
			act.InContractTime = true;
			ActivityRepository.Has(act);

			var actMeeting = new Activity("Meeting").WithId();
			actMeeting.InContractTime = false;
			ActivityRepository.Has(actMeeting);

			var scenario = ScenarioRepository.Has("Default");

			Target.TimeZone = TimeZoneInfo.Utc;
			
			var ass = new PersonAssignment(person, scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 1, 18));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			PersonAssignmentRepository.Add(ass);

	        var meeting = new Meeting(person, new[] {new MeetingPerson(person, false)}, "subj", "location", "", actMeeting,
		        scenario);
	        meeting.EndDate = meeting.StartDate = new DateOnly(2000, 1, 1);
			meeting.StartTime = TimeSpan.FromHours(16);
			meeting.EndTime = TimeSpan.FromHours(17);
			MeetingRepository.Has(meeting);

			var scheduledDay = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario)[person]
				.ScheduledDay(new DateOnly(2000, 1, 1));
	        
	        var dto = Target.DomainEntityToDto(scheduledDay);
	        dto.ContractTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(5));
        }
		
        [Test]
        public void VerifyDoToDtoWithPersonAssignment()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var act = new Activity("sdf").WithId();
			act.InContractTime = true;
			ActivityRepository.Has(act);
			
			var scenario = ScenarioRepository.Has("Default");

			Target.TimeZone = TimeZoneInfo.Utc;
			
			var ass = new PersonAssignment(person, scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 1, 18));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			PersonAssignmentRepository.Add(ass);

			var dto = Target.DomainEntityToDto(ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario)[person]
				.ScheduledDay(new DateOnly(2000, 1, 1)));
	        dto.PersonAssignmentCollection.Count.Should().Be.EqualTo(1);
			dto.ContractTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(6));
		}

		[Test]
		public void ShouldIgnoreNullAssignmentsWhenCreatingDto()
		{
			var person = PersonFactory.CreatePerson().WithId();
			PersonRepository.Has(person);

			var scenario = ScenarioRepository.Has("Default");

			Target.TimeZone = TimeZoneInfo.Utc;

			var dto = Target.DomainEntityToDto(ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario)[person]
				.ScheduledDay(new DateOnly(2000, 1, 1)));
			dto.PersonAssignmentCollection.Should().Be.Empty();
		}

        [Test]
        public void VerifyDoToDtoWithPersonAbsence()
        {
			var person = PersonFactory.CreatePerson().WithId();
			
			Target.TimeZone = TimeZoneInfo.Utc;

			var scenario = ScenarioRepository.Has("Default");
			var absence = new Absence {Description = new Description("Ill","SI")}.WithId();
			AbsenceRepository.Has(absence);

			var personAbsence = new PersonAbsence(person, scenario, new AbsenceLayer(absence, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 1, 18)));
			PersonAbsenceRepository.Add(personAbsence);

			var dto = Target.DomainEntityToDto(ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario)[person]
				.ScheduledDay(new DateOnly(2000, 1, 1)));
	        dto.PersonAbsenceCollection.Count.Should().Be.EqualTo(1);
        }
		
        [Test]
        public void VerifyDoToDtoWithPersonDayOff()
        {
			var person = PersonFactory.CreatePerson().WithId();

			var scenario = ScenarioRepository.Has("Default");
			Target.TimeZone = TimeZoneInfo.Utc;

			var dayOff = new DayOffTemplate(new Description("DO","DO")).WithId();

			var personDayOff = new PersonAssignment(person,scenario,new DateOnly(2000,1,1)).WithId();
			personDayOff.SetDayOff(dayOff);
			PersonAssignmentRepository.Add(personDayOff);

			var dto = Target.DomainEntityToDto(ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario)[person]
				.ScheduledDay(new DateOnly(2000, 1, 1)));
	        dto.PersonDayOff.Should().Not.Be.Null();
        }

	    [Test]
	    public void ShouldNotUseAbsencesInSpecialProjectionExcludeAbsences()
	    {
		    var person = PersonFactory.CreatePerson().WithId();

			var scenario = ScenarioRepository.Has("Default");
			Target.TimeZone = TimeZoneInfo.Utc;
			
		    var absence = new Absence {Description = new Description("Ill", "SI")}.WithId();
			AbsenceRepository.Has(absence);

		    var personAbsence = new PersonAbsence(person, scenario,
			    new AbsenceLayer(absence, new DateTimePeriod(2000, 1, 1, 16, 2000, 1, 2, 2)));
		    PersonAbsenceRepository.Add(personAbsence);

			var act = new Activity("sdf").WithId();
			act.InContractTime = true;
			ActivityRepository.Has(act);

			var ass = new PersonAssignment(person, scenario, new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 2, 2));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			PersonAssignmentRepository.Add(ass);

		    var scheduleDay =
			    ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				    new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenario)[person].ScheduledDay(new DateOnly(
					    2000, 1, 1));

			Target.SpecialProjection = "excludeAbsences";
		    var dto = Target.DomainEntityToDto(scheduleDay);
		    Assert.AreEqual(1, dto.ProjectedLayerCollection.Count);
		    Assert.AreEqual(act.Id, dto.ProjectedLayerCollection.Single().PayloadId);

		    Target.SpecialProjection = "excludeAbsencesMidnightSplit";
		    dto = Target.DomainEntityToDto(scheduleDay);
		    Assert.AreEqual(2, dto.ProjectedLayerCollection.Count);
		    Assert.AreEqual(act.Id, dto.ProjectedLayerCollection.First().PayloadId);
		    Assert.AreEqual(act.Id, dto.ProjectedLayerCollection.Last().PayloadId);
		}
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new AssemblerModule());
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<TenantPeopleLoader>().For<ITenantPeopleLoader>();
			isolate.UseTestDouble<FakeTenantLogonDataManager>().For<ITenantLogonDataManagerClient>();
		}
	}
}
