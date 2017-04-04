using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class SchedulePartAssemblerTest
    {
	    [Test]
	    public void PayrollSpecialProjection()
	    {
		    var person = PersonFactory.CreatePerson().WithId();

		    var personRepository = new FakePersonRepositoryLegacy();
		    var dateTimePeriodAssembler = new DateTimePeriodAssembler();
		    var scenarioRepository = new FakeCurrentScenario();
		    var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
		    var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
			    new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
				    activityAssembler),
			    new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
				    activityAssembler),
			    new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
				    new FakeMultiplicatorDefinitionSetRepository()));
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
		    var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(shiftCategoryAssembler,
				    dayOffAssembler, activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy(),
			    new TenantPeopleLoader(new FakeTenantLogonDataManager()));
		    var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
		    var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
		    var scheduleStorage = new FakeScheduleStorage();
		    var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
		    var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
		    var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
			    personDayOffAssembler,
			    personMeetingAssembler,
			    new ProjectedLayerAssembler(dateTimePeriodAssembler),
			    dateTimePeriodAssembler,
			    sdkProjectionServiceFactory, tagAssembler);
		    target.PersonRepository = personRepository;
		    target.ScheduleStorage = scheduleStorage;
		    target.TimeZone = TimeZoneInfo.Utc;
			
		    var act = new Activity("sdf").WithId();
			var ass = new PersonAssignment(person, scenarioRepository.Current(), new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000,1,1,12,2000,1,2,12));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			scheduleStorage.Add(ass);
			
		    target.SpecialProjection = "midnightSplit";
		    target.TimeZone = TimeZoneInfo.Utc;
		    var dto = target.DomainEntityToDto(scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,new ScheduleDictionaryLoadOptions(false,false), new DateOnlyPeriod(2000,1,1,2000,1,1), scenarioRepository.Current())[person].ScheduledDay(new DateOnly(2000,1,1)));
		    dto.ProjectedLayerCollection.Count.Should().Be.EqualTo(2);
	    }
		
	    [Test]
        public void VerifyDtoToDo()
        {
			var person = PersonFactory.CreatePerson().WithId();

			var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person);

			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
					new FakeMultiplicatorDefinitionSetRepository()));
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
			var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
			var scheduleStorage = new FakeScheduleStorage();
			var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
			var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());

			var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
				personDayOffAssembler,
				personMeetingAssembler,
				new ProjectedLayerAssembler(dateTimePeriodAssembler),
				dateTimePeriodAssembler,
				sdkProjectionServiceFactory, tagAssembler);
			target.PersonRepository = personRepository;
			target.ScheduleStorage = scheduleStorage;
			target.TimeZone = TimeZoneInfo.Utc;
			
			DateOnly date = new DateOnly(1900,1,1);
            SchedulePartDto dto = new SchedulePartDto
                                      {
										  Date = new DateOnlyDto { DateTime = date.Date },
                                          TimeZoneId = TimeZoneInfo.Utc.Id,
                                          PersonId = Guid.NewGuid(),
                                          PersonDayOff = new PersonDayOffDto()
                                      };

		    Assert.Throws<NotSupportedException>(() => target.DtoToDomainEntity(dto));
        }

        [Test]
        public void VerifyDoToDtoWithNullValue()
        {
			var personRepository = new FakePersonRepositoryLegacy();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
					new FakeMultiplicatorDefinitionSetRepository()));
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
			var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
			var scheduleStorage = new FakeScheduleStorage();
			var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
			var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
			var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
				personDayOffAssembler,
				personMeetingAssembler,
				new ProjectedLayerAssembler(dateTimePeriodAssembler),
				dateTimePeriodAssembler,
				sdkProjectionServiceFactory, tagAssembler);
			target.PersonRepository = personRepository;
			target.ScheduleStorage = scheduleStorage;
			target.TimeZone = TimeZoneInfo.Utc;
			
			Assert.IsNotNull(target.DomainEntityToDto(null));
        }
		
        [Test]
        public void VerifyDoToDtoWithProjection()
        {
			var person = PersonFactory.CreatePerson().WithId();

			var personRepository = new FakePersonRepositoryLegacy();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var scenarioRepository = new FakeCurrentScenario();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
					new FakeMultiplicatorDefinitionSetRepository()));
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
			var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
			var scheduleStorage = new FakeScheduleStorage();
			var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
			var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
			var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
				personDayOffAssembler,
				personMeetingAssembler,
				new ProjectedLayerAssembler(dateTimePeriodAssembler),
				dateTimePeriodAssembler,
				sdkProjectionServiceFactory, tagAssembler);
			target.PersonRepository = personRepository;
			target.ScheduleStorage = scheduleStorage;
			target.TimeZone = TimeZoneInfo.Utc;

			var act = new Activity("sdf").WithId();
	        act.InContractTime = true;
			
			var actMeeting = new Activity("Meeting").WithId();
	        actMeeting.InContractTime = false;
			
			var ass = new PersonAssignment(person, scenarioRepository.Current(), new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 1, 18));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			scheduleStorage.Add(ass);

	        var meeting = new Meeting(person, new[] {new MeetingPerson(person, false)}, "subj", "location", "", actMeeting,
		        scenarioRepository.Current());
	        meeting.EndDate = meeting.StartDate = new DateOnly(2000, 1, 1);
			meeting.StartTime = TimeSpan.FromHours(16);
			meeting.EndTime = TimeSpan.FromHours(17);

	        var scheduledDay = scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenarioRepository.Current())[person].ScheduledDay(new DateOnly(2000, 1, 1));
	        meeting.GetPersonMeetings(person).ForEach(scheduledDay.Add);

	        var dto = target.DomainEntityToDto(scheduledDay);
	        dto.ContractTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(5));
        }
		
        [Test]
        public void VerifyDoToDtoWithPersonAssignment()
        {
			var person = PersonFactory.CreatePerson().WithId();

			var personRepository = new FakePersonRepositoryLegacy();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var scenarioRepository = new FakeCurrentScenario();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
					new FakeMultiplicatorDefinitionSetRepository()));
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
			var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
			var scheduleStorage = new FakeScheduleStorage();
			var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
			var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
			var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
				personDayOffAssembler,
				personMeetingAssembler,
				new ProjectedLayerAssembler(dateTimePeriodAssembler),
				dateTimePeriodAssembler,
				sdkProjectionServiceFactory, tagAssembler);
			target.PersonRepository = personRepository;
			target.ScheduleStorage = scheduleStorage;
			target.TimeZone = TimeZoneInfo.Utc;

			var act = new Activity("sdf").WithId();
			act.InContractTime = true;

			var ass = new PersonAssignment(person, scenarioRepository.Current(), new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 1, 18));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			scheduleStorage.Add(ass);

			var dto = target.DomainEntityToDto(scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenarioRepository.Current())[person].ScheduledDay(new DateOnly(2000, 1, 1)));
	        dto.PersonAssignmentCollection.Count.Should().Be.EqualTo(1);
			dto.ContractTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(6));
		}

		[Test]
		public void ShouldIgnoreNullAssignmentsWhenCreatingDto()
		{
			var person = PersonFactory.CreatePerson().WithId();

			var personRepository = new FakePersonRepositoryLegacy();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var scenarioRepository = new FakeCurrentScenario();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
					new FakeMultiplicatorDefinitionSetRepository()));
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
			var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
			var scheduleStorage = new FakeScheduleStorage();
			var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
			var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
			var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
				personDayOffAssembler,
				personMeetingAssembler,
				new ProjectedLayerAssembler(dateTimePeriodAssembler),
				dateTimePeriodAssembler,
				sdkProjectionServiceFactory, tagAssembler);
			target.PersonRepository = personRepository;
			target.ScheduleStorage = scheduleStorage;
			target.TimeZone = TimeZoneInfo.Utc;
			
			var dto = target.DomainEntityToDto(scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenarioRepository.Current())[person].ScheduledDay(new DateOnly(2000, 1, 1)));
			dto.PersonAssignmentCollection.Should().Be.Empty();
		}

        [Test]
        public void VerifyDoToDtoWithPersonAbsence()
        {
			var person = PersonFactory.CreatePerson().WithId();

			var personRepository = new FakePersonRepositoryLegacy();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var scenarioRepository = new FakeCurrentScenario();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
					new FakeMultiplicatorDefinitionSetRepository()));
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
			var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
			var scheduleStorage = new FakeScheduleStorage();
			var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
			var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
			var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
				personDayOffAssembler,
				personMeetingAssembler,
				new ProjectedLayerAssembler(dateTimePeriodAssembler),
				dateTimePeriodAssembler,
				sdkProjectionServiceFactory, tagAssembler);
			target.PersonRepository = personRepository;
			target.ScheduleStorage = scheduleStorage;
			target.TimeZone = TimeZoneInfo.Utc;
			
			var absence = new Absence {Description = new Description("Ill","SI")}.WithId();

			var personAbsence = new PersonAbsence(person, scenarioRepository.Current(), new AbsenceLayer(absence, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 1, 18)));
			scheduleStorage.Add(personAbsence);

			var dto = target.DomainEntityToDto(scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenarioRepository.Current())[person].ScheduledDay(new DateOnly(2000, 1, 1)));
	        dto.PersonAbsenceCollection.Count.Should().Be.EqualTo(1);
        }
		
        [Test]
        public void VerifyDoToDtoWithPersonDayOff()
        {
			var person = PersonFactory.CreatePerson().WithId();

			var personRepository = new FakePersonRepositoryLegacy();
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var scenarioRepository = new FakeCurrentScenario();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
				new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
					activityAssembler),
				new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
					new FakeMultiplicatorDefinitionSetRepository()));
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
			var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(shiftCategoryAssembler,
					dayOffAssembler, activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));
			var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
			var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
			var scheduleStorage = new FakeScheduleStorage();
			var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
			var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
			var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
				personDayOffAssembler,
				personMeetingAssembler,
				new ProjectedLayerAssembler(dateTimePeriodAssembler),
				dateTimePeriodAssembler,
				sdkProjectionServiceFactory, tagAssembler);
			target.PersonRepository = personRepository;
			target.ScheduleStorage = scheduleStorage;
			target.TimeZone = TimeZoneInfo.Utc;

			var dayOff = new DayOffTemplate(new Description("DO","DO")).WithId();

			var personDayOff = new PersonAssignment(person,scenarioRepository.Current(),new DateOnly(2000,1,1)).WithId();
			personDayOff.SetDayOff(dayOff);
			scheduleStorage.Add(personDayOff);

			var dto = target.DomainEntityToDto(scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenarioRepository.Current())[person].ScheduledDay(new DateOnly(2000, 1, 1)));
	        dto.PersonDayOff.Should().Not.Be.Null();
        }

	    [Test]
	    public void ShouldNotUseAbsencesInSpecialProjectionExcludeAbsences()
	    {
		    var person = PersonFactory.CreatePerson().WithId();

		    var personRepository = new FakePersonRepositoryLegacy();
		    var dateTimePeriodAssembler = new DateTimePeriodAssembler();
		    var scenarioRepository = new FakeCurrentScenario();
		    var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
		    var assignmentAssembler = new PersonAssignmentAssembler(new FakeShiftCategoryRepository(),
			    new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
				    activityAssembler),
			    new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(), dateTimePeriodAssembler,
				    activityAssembler),
			    new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
				    new FakeMultiplicatorDefinitionSetRepository()));
		    var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
		    var personAbsenceAssembler = new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler);
		    var dayOffAssembler = new DayOffAssembler(new FakeDayOffTemplateRepository());
		    var shiftCategoryRepository = new FakeShiftCategoryRepository();
		    var shiftCategoryAssembler = new ShiftCategoryAssembler(shiftCategoryRepository);
		    var personAssembler = new PersonAssembler(personRepository,
			    new WorkflowControlSetAssembler(shiftCategoryAssembler,
				    dayOffAssembler, activityAssembler,
				    absenceAssembler), new PersonAccountUpdaterDummy(),
			    new TenantPeopleLoader(new FakeTenantLogonDataManager()));
		    var personDayOffAssembler = new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler);
		    var personMeetingAssembler = new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler);
		    var scheduleStorage = new FakeScheduleStorage();
		    var sdkProjectionServiceFactory = new SdkProjectionServiceFactory();
		    var tagAssembler = new ScheduleTagAssembler(new FakeScheduleTagRepository());
		    var target = new SchedulePartAssembler(assignmentAssembler, personAbsenceAssembler,
			    personDayOffAssembler,
			    personMeetingAssembler,
			    new ProjectedLayerAssembler(dateTimePeriodAssembler),
			    dateTimePeriodAssembler,
			    sdkProjectionServiceFactory, tagAssembler);
		    target.PersonRepository = personRepository;
		    target.ScheduleStorage = scheduleStorage;
		    target.TimeZone = TimeZoneInfo.Utc;
			
		    var absence = new Absence {Description = new Description("Ill", "SI")}.WithId();

		    var personAbsence = new PersonAbsence(person, scenarioRepository.Current(),
			    new AbsenceLayer(absence, new DateTimePeriod(2000, 1, 1, 16, 2000, 1, 2, 2)));
		    scheduleStorage.Add(personAbsence);

			var act = new Activity("sdf").WithId();
			act.InContractTime = true;

			var ass = new PersonAssignment(person, scenarioRepository.Current(), new DateOnly(2000, 1, 1));
			ass.AddActivity(act, new DateTimePeriod(2000, 1, 1, 12, 2000, 1, 2, 2));
			ass.SetShiftCategory(new ShiftCategory("asd").WithId());
			scheduleStorage.Add(ass);

		    var scheduleDay =
			    scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
				    new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1), scenarioRepository.Current())[person].ScheduledDay(new DateOnly(
					    2000, 1, 1));

			target.SpecialProjection = "excludeAbsences";
		    var dto = target.DomainEntityToDto(scheduleDay);
		    Assert.AreEqual(1, dto.ProjectedLayerCollection.Count);
		    Assert.AreEqual(act.Id, dto.ProjectedLayerCollection.Single().PayloadId);

		    target.SpecialProjection = "excludeAbsencesMidnightSplit";
		    dto = target.DomainEntityToDto(scheduleDay);
		    Assert.AreEqual(2, dto.ProjectedLayerCollection.Count);
		    Assert.AreEqual(act.Id, dto.ProjectedLayerCollection.First().PayloadId);
		    Assert.AreEqual(act.Id, dto.ProjectedLayerCollection.Last().PayloadId);
	    }
    }
}
