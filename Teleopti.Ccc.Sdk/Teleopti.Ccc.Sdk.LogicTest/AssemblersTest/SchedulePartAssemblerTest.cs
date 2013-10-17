using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Is=Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class SchedulePartAssemblerTest
    {
        private SchedulePartAssembler target;
        private ICurrentScenario scenarioRepository;
        private MockRepository mocks;
        private IScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto> assignmentAssembler;
        private IScheduleDataAssembler<IPersonAbsence, PersonAbsenceDto> absenceAssembler;
				private IScheduleDataAssembler<IPersonAssignment, PersonDayOffDto> dayOffAssembler;
        private IScheduleDataAssembler<IPersonMeeting, PersonMeetingDto> personMeetingAssembler;
    	private ISdkProjectionServiceFactory sdkProjectionServiceFactory;
        private IScheduleDataAssembler<IPreferenceDay, PreferenceRestrictionDto> _preferenceRestrictionAssembler;
        private IScheduleDataAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto> _studentDayAssembler;
        private IPersonRepository personRepository;
        private IScheduleRepository scheduleRepository;
        private IPerson person;
        private DateTimePeriodAssembler dateTimePeriodAssembler;
		private IScheduleTagAssembler _tagAssembler;

	    [SetUp]
        public void Setup()
        {
            person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            mocks = new MockRepository();
			scenarioRepository = mocks.StrictMock<ICurrentScenario>();
            assignmentAssembler = mocks.DynamicMock<IScheduleDataAssembler<IPersonAssignment, PersonAssignmentDto>>();
            absenceAssembler = mocks.DynamicMock<IScheduleDataAssembler<IPersonAbsence, PersonAbsenceDto>>();
            dayOffAssembler = mocks.DynamicMock<IScheduleDataAssembler<IPersonAssignment, PersonDayOffDto>>();
            personMeetingAssembler = mocks.DynamicMock<IScheduleDataAssembler<IPersonMeeting, PersonMeetingDto>>();
            _preferenceRestrictionAssembler =
                mocks.DynamicMock<IScheduleDataAssembler<IPreferenceDay, PreferenceRestrictionDto>>();
            _studentDayAssembler =
                mocks.DynamicMock<IScheduleDataAssembler<IStudentAvailabilityDay, StudentAvailabilityDayDto>>();
            scheduleRepository = mocks.StrictMock<IScheduleRepository>();
            personRepository = mocks.StrictMock<IPersonRepository>();
        	sdkProjectionServiceFactory = mocks.DynamicMock<ISdkProjectionServiceFactory>();
            dateTimePeriodAssembler = new DateTimePeriodAssembler();
	        _tagAssembler = mocks.DynamicMock<IScheduleTagAssembler>();
            target = new SchedulePartAssembler(scenarioRepository, assignmentAssembler, absenceAssembler, dayOffAssembler,
                                               personMeetingAssembler, _preferenceRestrictionAssembler,
                                               _studentDayAssembler,
                                               new ProjectedLayerAssembler(dateTimePeriodAssembler),
                                               dateTimePeriodAssembler,
															  sdkProjectionServiceFactory, _tagAssembler);
            target.PersonRepository = personRepository;
            target.ScheduleRepository = scheduleRepository;
			  target.TimeZone = (TimeZoneInfo.Utc);
        }

		[Test]
		public void PayrollSpecialProjection()
		{
			target = new SchedulePartAssembler(scenarioRepository, assignmentAssembler, absenceAssembler, dayOffAssembler,
											  personMeetingAssembler, _preferenceRestrictionAssembler,
											  _studentDayAssembler,
											  new ProjectedLayerAssembler(dateTimePeriodAssembler),
											  dateTimePeriodAssembler,
											  new SdkProjectionServiceFactory(),_tagAssembler);
			IActivity act = new Activity("sdf");
			act.SetId(Guid.NewGuid());
			var innerDic = new Dictionary<IPerson, IScheduleRange>();
			var schedDic = new ScheduleDictionaryForTest(new Scenario("d"),
			                                             new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1)),
			                                             innerDic);
			var schedDay = ExtractedSchedule.CreateScheduleDay(schedDic, new Person(), new DateOnly(2000, 2, 1));
			schedDay.Person.SetId(Guid.NewGuid());
			var ass = new PersonAssignment(schedDay.Person, schedDay.Scenario, new DateOnly(2000,1,1));
			ass.AddMainLayer(act, schedDay.DateOnlyAsPeriod.Period().MovePeriod(TimeSpan.FromHours(12)));
			ass.SetShiftCategory(new ShiftCategory("asd"));
			schedDay.Add(ass);
			using (mocks.Record())
			{
				//why do I need to do this? Totally unneeded for what I want to test. mockeri, mockera...
				Expect.Call(assignmentAssembler.DomainEntityToDto(ass)).Return(new PersonAssignmentDto());
				Expect.Call(absenceAssembler.DomainEntitiesToDtos(null)).Return(new List<PersonAbsenceDto>()).IgnoreArguments();
				Expect.Call(personMeetingAssembler.DomainEntitiesToDtos(null)).Return(new List<PersonMeetingDto>()).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.SpecialProjection = "midnightSplit";
				target.TimeZone = (TimeZoneInfo.Utc);
				var dto = target.DomainEntityToDto(schedDay);
				Assert.AreEqual(2, dto.ProjectedLayerCollection.Count);				
			}
		}

        [Test]
        public void VerifyDtoToDo()
        {
            DateOnly date = new DateOnly(1900,1,1);
            SchedulePartDto dto = new SchedulePartDto
                                      {
										  Date = new DateOnlyDto { DateTime = date },
                                          TimeZoneId = TimeZoneInfo.Utc.Id,
                                          PersonId = Guid.NewGuid(),
                                          PersonDayOff = new PersonDayOffDto()
                                      };
            dto.PreferenceRestriction = new PreferenceRestrictionDto();

            StudentAvailabilityRestrictionDto restriction = new StudentAvailabilityRestrictionDto();
            StudentAvailabilityDayDto studeDay = new StudentAvailabilityDayDto();
            studeDay.StudentAvailabilityRestrictions.Add(restriction);
            dto.StudentAvailabilityDay = studeDay;

            IScheduleDictionary dic = mocks.StrictMock<IScheduleDictionary>();
            IScheduleRange range = mocks.StrictMock<IScheduleRange>();
            IScheduleDay part = mocks.StrictMock<IScheduleDay>();
            var assResult = Enumerable.Empty<IPersonAssignment>();
            IPersonAbsence absResult = mocks.StrictMock<IPersonAbsence>();
            IStudentAvailabilityDay studentDay = mocks.StrictMock<IStudentAvailabilityDay>();
      
            using(mocks.Record())
            {
                Expect.Call(personRepository.Load(dto.PersonId)).Return(person);
                Expect.Call(scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null)).Return(dic);
                LastCall.Constraints(new[]
                                         {
                                             Is.Matching<IPerson>(t => t == person),
                                             Is.Matching<IScheduleDictionaryLoadOptions>(t => t.LoadNotes.Equals(false) && t.LoadRestrictions.Equals(true)),
																						 //is depending on timezone
                                             Is.Anything(),
                                             Is.Null()
                                         });
                Expect.Call(dic[person]).Return(range);
                Expect.Call(range.ScheduledDay(date)).Return(part);
                Expect.Call(part.Person).Return(person).Repeat.Any();
								Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfo.Utc));
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.FullDayAbsence);
                part.Clear<IPersonAssignment>();
                part.Clear<IStudentAvailabilityDay>();
                part.DeleteFullDayAbsence(part);
                Expect.Call(assignmentAssembler.DtosToDomainEntities(dto.PersonAssignmentCollection)).Return(assResult);
                Expect.Call(absenceAssembler.DtosToDomainEntities(dto.PersonAbsenceCollection)).Return(new []{absResult});
                Expect.Call(_studentDayAssembler.DtoToDomainEntity(dto.StudentAvailabilityDay)).Return(
                    studentDay);

                part.Add(absResult);
                part.Add(studentDay);

                Expect.Call(scenarioRepository.Current()).Return(null).Repeat.AtLeastOnce();
            }
            using(mocks.Playback())
            {
                Assert.AreSame(part, target.DtoToDomainEntity(dto));
            }
        }

        [Test]
        public void VerifyNoDayOffDoesNothingButClearsPersonDayOffs()
        {
            DateOnly date = new DateOnly(1900, 1, 1);
			SchedulePartDto dto = new SchedulePartDto { Date = new DateOnlyDto { DateTime = date }, TimeZoneId = TimeZoneInfo.Utc.Id, PersonId = Guid.NewGuid() };

            IScheduleDictionary dic = mocks.StrictMock<IScheduleDictionary>();
            IScheduleRange range = mocks.StrictMock<IScheduleRange>();
            IScheduleDay part = mocks.StrictMock<IScheduleDay>();
            var assResult = Enumerable.Empty<IPersonAssignment>();
            IPersonAbsence absResult = mocks.StrictMock<IPersonAbsence>();
            
            using (mocks.Record())
            {
                Expect.Call(personRepository.Load(dto.PersonId)).Return(person);
				Expect.Call(scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), null)).Return(dic);
                LastCall.IgnoreArguments(); //tested in another test
                Expect.Call(dic[person]).Return(range);
                Expect.Call(range.ScheduledDay(date)).Return(part);
                Expect.Call(part.SignificantPart()).Return(SchedulePartView.Absence);
                Expect.Call(part.Person).Return(person).Repeat.Any();
								Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(date, TimeZoneInfo.Utc));
                part.Clear<IPersonAssignment>();
                part.Clear<IStudentAvailabilityDay>();
                part.DeleteAbsence(false);
                Expect.Call(assignmentAssembler.DtosToDomainEntities(dto.PersonAssignmentCollection)).Return(assResult);
                Expect.Call(absenceAssembler.DtosToDomainEntities(dto.PersonAbsenceCollection)).Return(new[] { absResult });
                
                part.Add(absResult);

                Expect.Call(scenarioRepository.Current()).Return(null).Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                Assert.AreSame(part, target.DtoToDomainEntity(dto));
            }
        }

        [Test]
        public void VerifyDoToDtoWithNullValue()
        {
            Assert.IsNotNull(target.DomainEntityToDto(null));
        }

        [Test]
        public void VerifyDoToDtoWithProjection()
        {
            IScheduleDay part = mocks.StrictMock<IScheduleDay>();
            IVisualLayerCollection visualLayerCollection =
                VisualLayerCollectionFactory.CreateForWorkShift(new Person(), TimeSpan.FromHours(8), TimeSpan.FromHours(18));
            visualLayerCollection.ForEach(l => l.Payload.SetId(Guid.NewGuid()));
            PrepareSchedulePartForDoToDto(part,visualLayerCollection);

            IEnumerable<IPersistableScheduleData> restriction = new List<IPersistableScheduleData>().ToArray();
            Expect.Call(part.PersonAbsenceCollection()).Return(
                new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>()));
            Expect.Call(part.PersonAssignment()).Return(null);
            Expect.Call(part.PersistableScheduleDataCollection()).Return(restriction).Repeat.Twice();
            
            Expect.Call(absenceAssembler.DomainEntitiesToDtos(new List<IPersonAbsence>())).Return(
                new List<PersonAbsenceDto>());

            IPersonMeeting personMeeting = CreatePersonMeeting();
            PersonMeetingDto personMeetingDto = new PersonMeetingDto();
            personMeetingDto.Id = Guid.NewGuid();
            Expect.Call(part.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting }));
            Expect.Call(personMeetingAssembler.DomainEntitiesToDtos(new List<IPersonMeeting> { personMeeting })).Return(
                new List<PersonMeetingDto> { personMeetingDto });
			Expect.Call(part.ScheduleTag()).Return(NullScheduleTag.Instance);
			Expect.Call(_tagAssembler.DomainEntityToDto(NullScheduleTag.Instance)).Return(null);
            
            mocks.ReplayAll();
            SchedulePartDto schedulePartDto = target.DomainEntityToDto(part);
            Assert.AreEqual(DateTime.MinValue.Add(visualLayerCollection.ContractTime()), schedulePartDto.ContractTime);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoWithPersonAssignment()
        {
            IPersonAssignment assResult = mocks.StrictMock<IPersonAssignment>();
            IScheduleDay part = mocks.StrictMock<IScheduleDay>();
            PrepareSchedulePartForDoToDto(part);
            IEnumerable<IPersistableScheduleData> restrictions = new List<IPersistableScheduleData>().ToArray();
            PersonAssignmentDto personAssignmentDto = new PersonAssignmentDto();

            Expect.Call(part.PersonAbsenceCollection()).Return(
                new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>()));
            Expect.Call(part.PersonAssignment()).Return(assResult);
            Expect.Call(assignmentAssembler.DomainEntityToDto(assResult)).Return(personAssignmentDto);
            Expect.Call(absenceAssembler.DomainEntitiesToDtos(new List<IPersonAbsence>())).Return(
                new List<PersonAbsenceDto>());

            IPersonMeeting personMeeting = CreatePersonMeeting();
            PersonMeetingDto personMeetingDto = new PersonMeetingDto();
            personMeetingDto.Id = Guid.NewGuid();
            Expect.Call(part.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting }));
            Expect.Call(personMeetingAssembler.DomainEntitiesToDtos(new List<IPersonMeeting> { personMeeting })).Return(
                new List<PersonMeetingDto> { personMeetingDto });
            Expect.Call(part.PersistableScheduleDataCollection()).Return(restrictions).Repeat.Twice();
			Expect.Call(part.ScheduleTag()).Return(NullScheduleTag.Instance);
			Expect.Call(_tagAssembler.DomainEntityToDto(NullScheduleTag.Instance)).Return(null);
            mocks.ReplayAll();
            SchedulePartDto schedulePartDto = target.DomainEntityToDto(part);
            Assert.AreEqual(1, schedulePartDto.PersonAssignmentCollection.Count);
            Assert.AreEqual(personAssignmentDto,schedulePartDto.PersonAssignmentCollection.First());
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoWithPersonAbsence()
        {
            IPersonAbsence personAbsence = mocks.StrictMock<IPersonAbsence>();
            IScheduleDay part = mocks.StrictMock<IScheduleDay>();
            PrepareSchedulePartForDoToDto(part);
            IEnumerable<IPersistableScheduleData> restrictions = new List<IPersistableScheduleData>().ToArray();
            PersonAbsenceDto personAbsenceDto = new PersonAbsenceDto();

            Expect.Call(part.PersonAbsenceCollection()).Return(
                new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>{personAbsence}));
            Expect.Call(part.PersonAssignment()).Return(null);
            Expect.Call(absenceAssembler.DomainEntitiesToDtos(new List<IPersonAbsence>{personAbsence})).Return(
                new List<PersonAbsenceDto>{personAbsenceDto});

            IPersonMeeting personMeeting = CreatePersonMeeting();
            PersonMeetingDto personMeetingDto = new PersonMeetingDto();
            personMeetingDto.Id = Guid.NewGuid();
            Expect.Call(part.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting }));
            Expect.Call(personMeetingAssembler.DomainEntitiesToDtos(new List<IPersonMeeting> { personMeeting })).Return(
                new List<PersonMeetingDto> { personMeetingDto });
            Expect.Call(part.PersistableScheduleDataCollection()).Return(restrictions).Repeat.Twice();
	        Expect.Call(part.ScheduleTag()).Return(NullScheduleTag.Instance);
			Expect.Call(_tagAssembler.DomainEntityToDto(NullScheduleTag.Instance)).Return(null);
            mocks.ReplayAll();
            SchedulePartDto schedulePartDto = target.DomainEntityToDto(part);
            Assert.AreEqual(1, schedulePartDto.PersonAbsenceCollection.Count);
            Assert.AreEqual(personAbsenceDto, schedulePartDto.PersonAbsenceCollection.First());
            mocks.VerifyAll();
        }


        private static IPersonMeeting CreatePersonMeeting()
        {
            DateTime dateTime = new DateTime(1999, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            DateTimePeriod period = new DateTimePeriod(dateTime, dateTime.AddHours(1));
            IPerson aPerson = PersonFactory.CreatePerson();
            IMeeting mainMeeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description",
                    ActivityFactory.CreateActivity("activity"), ScenarioFactory.CreateScenarioAggregate());
            IPersonMeeting personMeeting = new PersonMeeting(mainMeeting, new MeetingPerson(aPerson, true), period);
            personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(aPerson, true));
            return personMeeting;
        }

        private void PrepareSchedulePartForDoToDto(IScheduleDay part)
        {
            PrepareSchedulePartForDoToDto(part, new VisualLayerCollection(person, new List<IVisualLayer>(), new ProjectionPayloadMerger()));
        }

        private void PrepareSchedulePartForDoToDto(IScheduleDay part, IVisualLayerCollection visualLayerCollection)
        {
            IProjectionService projectionService = mocks.StrictMock<IProjectionService>();
        	Expect.Call(sdkProjectionServiceFactory.CreateProjectionService(part, string.Empty, target.TimeZone)).Return(projectionService);
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection);
            Expect.Call(part.Person).Return(person).Repeat.Any();
            Expect.Call(part.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2008, 7, 25), (TimeZoneInfo.Utc))).Repeat.Any();
        }

        [Test]
        public void VerifyDoToDtoWithPersonDayOff()
        {
            var personDayOff = mocks.StrictMock<IPersonAssignment>();
            IScheduleDay part = mocks.StrictMock<IScheduleDay>();
            PrepareSchedulePartForDoToDto(part);
            IEnumerable<IPersistableScheduleData> resstrictions = new List<IPersistableScheduleData>().ToArray();
            PersonDayOffDto personDayOffDto = new PersonDayOffDto();

            Expect.Call(part.PersonAbsenceCollection()).Return(
                new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>()));
	        Expect.Call(part.PersonAssignment()).Return(personDayOff);
            Expect.Call(absenceAssembler.DomainEntitiesToDtos(new List<IPersonAbsence>())).Return(
                new List<PersonAbsenceDto>());
            Expect.Call(dayOffAssembler.DomainEntityToDto(personDayOff)).Return(personDayOffDto);

            IPersonMeeting personMeeting = CreatePersonMeeting();
            PersonMeetingDto personMeetingDto = new PersonMeetingDto();
            personMeetingDto.Id = Guid.NewGuid();
            Expect.Call(part.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting }));
            Expect.Call(personMeetingAssembler.DomainEntitiesToDtos(new List<IPersonMeeting> { personMeeting })).Return(
                new List<PersonMeetingDto> { personMeetingDto });
            Expect.Call(part.PersistableScheduleDataCollection()).Return(resstrictions).Repeat.Twice();
			Expect.Call(part.ScheduleTag()).Return(NullScheduleTag.Instance);
			Expect.Call(_tagAssembler.DomainEntityToDto(NullScheduleTag.Instance)).Return(null);
            mocks.ReplayAll();
            SchedulePartDto schedulePartDto = target.DomainEntityToDto(part);
            Assert.AreEqual(personDayOffDto, schedulePartDto.PersonDayOff);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyDtoWithRestrictions()
        {
            var personDayOff = mocks.StrictMock<IPersonAssignment>();
            IScheduleDay part = mocks.StrictMock<IScheduleDay>();
            PrepareSchedulePartForDoToDto(part);
            PreferenceRestriction preference = new PreferenceRestriction();
            IPersistableScheduleData day = new PreferenceDay(person, new DateOnly(2009, 2, 2), preference);
            IEnumerable<IPersistableScheduleData> prefs = new List<IPersistableScheduleData> { day }.ToArray();
            
            PersonDayOffDto personDayOffDto = new PersonDayOffDto();
            PreferenceRestrictionDto preferenceRestrictionDto = new PreferenceRestrictionDto();
            Expect.Call(part.PersonAbsenceCollection()).Return(
                new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>()));
	        Expect.Call(part.PersonAssignment()).Return(personDayOff);
            Expect.Call(assignmentAssembler.DomainEntityToDto(personDayOff)).Return(new PersonAssignmentDto());
            Expect.Call(absenceAssembler.DomainEntitiesToDtos(new List<IPersonAbsence>())).Return(
                new List<PersonAbsenceDto>());
            Expect.Call(dayOffAssembler.DomainEntityToDto(personDayOff)).Return(personDayOffDto);

            Expect.Call(part.PersistableScheduleDataCollection()).Return(prefs).Repeat.AtLeastOnce();

            Expect.Call(_preferenceRestrictionAssembler.DomainEntityToDto((PreferenceDay)day)).Return(
                preferenceRestrictionDto);

            Expect.Call(part.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> ()));
            Expect.Call(personMeetingAssembler.DomainEntitiesToDtos(new List<IPersonMeeting>())).Return(
                new List<PersonMeetingDto> ());
			Expect.Call(part.ScheduleTag()).Return(NullScheduleTag.Instance);
			Expect.Call(_tagAssembler.DomainEntityToDto(NullScheduleTag.Instance)).Return(null);
            mocks.ReplayAll();
            SchedulePartDto schedulePartDto = target.DomainEntityToDto(part);
            Assert.AreEqual(preferenceRestrictionDto, schedulePartDto.PreferenceRestriction);
            mocks.VerifyAll();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotExecuteFromDtoConverterWithoutPersonRepository()
        {
            target.PersonRepository = null;
            target.DtoToDomainEntity(new SchedulePartDto());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotExecuteFromDtoConverterWithoutScheduleRepository()
        {
            target.ScheduleRepository = null;
            target.DtoToDomainEntity(new SchedulePartDto());
        }

        [Test]
        public void ShouldNotUseAbsencesInSpecialProjectionExcludeAbsences()
        {
			  target = new SchedulePartAssembler(scenarioRepository, assignmentAssembler, absenceAssembler, dayOffAssembler,
									 personMeetingAssembler, _preferenceRestrictionAssembler,
									 _studentDayAssembler,
									 new ProjectedLayerAssembler(dateTimePeriodAssembler),
									 dateTimePeriodAssembler,
									 new SdkProjectionServiceFactory(),_tagAssembler);
            var activityId = Guid.NewGuid();
            var scheduleDay = CreateScheduleDay(activityId);
            
            using (mocks.Record())
            {
                Expect.Call(assignmentAssembler.DomainEntityToDto(scheduleDay.PersonAssignment())).Return(new PersonAssignmentDto()).Repeat.AtLeastOnce();
                Expect.Call(absenceAssembler.DomainEntitiesToDtos(null)).Return(new List<PersonAbsenceDto>()).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(personMeetingAssembler.DomainEntitiesToDtos(null)).Return(new List<PersonMeetingDto>()).IgnoreArguments().Repeat.AtLeastOnce();
            }
            using (mocks.Playback())
            {
                target.SpecialProjection = "excludeAbsences";
                target.TimeZone = (TimeZoneInfo.Utc);
                var dto = target.DomainEntityToDto(scheduleDay);
                Assert.AreEqual(1, dto.ProjectedLayerCollection.Count);
                Assert.AreEqual(activityId, ((List<ProjectedLayerDto>)dto.ProjectedLayerCollection)[0].PayloadId);

                target.SpecialProjection = "excludeAbsencesMidnightSplit";
                target.TimeZone = (TimeZoneInfo.Utc);
                dto = target.DomainEntityToDto(scheduleDay);
                Assert.AreEqual(1, dto.ProjectedLayerCollection.Count);
                Assert.AreEqual(activityId, ((List<ProjectedLayerDto>)dto.ProjectedLayerCollection)[0].PayloadId);
            }
        }

        private static IScheduleDay CreateScheduleDay(Guid activityId)
        {
            var date = new DateTime(2000, 2, 1, 0, 0, 0, DateTimeKind.Utc);
            var start = date.AddHours(1);
            var end = date.AddHours(22);
            var period = new DateTimePeriod(start, end);

            IActivity activity = new Activity("activity");
            activity.SetId(activityId);
            var innerDictionary = new Dictionary<IPerson, IScheduleRange>();
            var schedDictionary = new ScheduleDictionaryForTest(new Scenario("d"), new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2001, 1, 1)), innerDictionary);
            var scheduleDay = ExtractedSchedule.CreateScheduleDay(schedDictionary, new Person(), new DateOnly(2000, 2, 1));
            scheduleDay.Person.SetId(Guid.NewGuid());
            var assignment = new PersonAssignment(scheduleDay.Person, scheduleDay.Scenario, new DateOnly(2000,1,1));
	        assignment.AddMainLayer(activity, period);
					assignment.SetShiftCategory(new ShiftCategory("sdf"));
            scheduleDay.Add(assignment);

            var absence = AbsenceFactory.CreateAbsence("absence");
            absence.SetId(Guid.NewGuid());
            var personAbsence = new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, new AbsenceLayer(absence, period));
            scheduleDay.Add(personAbsence);

            return scheduleDay;
        }
    }
}
