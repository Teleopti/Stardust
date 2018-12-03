using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.LogicTest.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;



namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonRequestAssemblerTest
    {
        [Test]
        public void VerifyDtoToDoTextRequestWithReload()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);

	        var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
	        var dateTimePeriodAssembler = new DateTimePeriodAssembler();
	        var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
	        var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
	        var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
	        var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person);
	        var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());

			TextRequest textRequest = new TextRequest(new DateTimePeriod());
			IPersonRequest personRequest = new PersonRequest(person, textRequest).WithId();
			personRequestRepository.Add(personRequest);

			PersonDto personDto = new PersonDto { Id = person.Id };
            TextRequestDto textRequestDto = new TextRequestDto {Period = new DateTimePeriodDto()};
            PersonRequestDto personRequestDto = new PersonRequestDto
                {
                    CreatedDate = date.Date,
                    Message = "test",
                    Id = personRequest.Id.GetValueOrDefault(),
                    Person = personDto,
                    Request = textRequestDto,
                    RequestedDate = date.Date,
                    RequestedDateLocal = date.Date,
                    RequestStatus = RequestStatusDto.Pending,
                    Subject = "subject",
                    UpdatedOn = date.Date
                };

            personRequest = target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
        }

        [Test]
        public void VerifyDtoToDoTextRequest()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
	        var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person);
	        var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
			var personDto = new PersonDto { Id = person.Id };
            var textRequestDto = new TextRequestDto {Period = new DateTimePeriodDto()};
            var personRequestDto = new PersonRequestDto
                {
                    CreatedDate = date.Date,
                    Message = "test",
                    Person = personDto,
                    Request = textRequestDto,
                    RequestedDate = date.Date,
                    RequestedDateLocal = date.Date,
                    RequestStatus = RequestStatusDto.Pending,
                    Subject = "subject",
                    UpdatedOn = date.Date
                };
			
            var personRequest = target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is TextRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
        }

        [Test]
        public void VerifyDtoToDoAbsenceRequest()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);
			var absence = AbsenceFactory.CreateAbsence("Sjuk").WithId();

	        var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
	        var dateTimePeriodAssembler = new DateTimePeriodAssembler();
	        var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
	        var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);
	        var absenceAssembler = new AbsenceAssembler(absenceRepository);
	        var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
	        var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person);
	        var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();

			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			var personDto = new PersonDto { Id = person.Id };
            var absenceRequestDto = new AbsenceRequestDto {Period = new DateTimePeriodDto(),Absence = new AbsenceDto {Id = absence.Id.Value} };
            var personRequestDto = new PersonRequestDto
            {
                CreatedDate = date.Date,
                Message = "test",
                Person = personDto,
                Request = absenceRequestDto,
                RequestedDate = date.Date,
                RequestedDateLocal = date.Date,
                RequestStatus = RequestStatusDto.Pending,
                Subject = "subject",
                UpdatedOn = date.Date
            };
			
            IPersonRequest personRequest = target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is IAbsenceRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
        }
		
        [Test]
        public void VerifyDtoToDoShiftTradeRequest()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler {PersonAssembler = personAssembler};
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
			var personDto = new PersonDto { Id = person.Id };
            var shiftTradeRequestDto = new ShiftTradeRequestDto {Period = new DateTimePeriodDto()};

	        ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto
	        {
		        DateFrom = new DateOnlyDto {DateTime = date.Date},
		        DateTo = new DateOnlyDto {DateTime = date.Date},
		        PersonFrom = personDto,
		        PersonTo = personDto
			};
            shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);
			
            PersonRequestDto personRequestDto = new PersonRequestDto
            {
                CreatedDate = date.Date,
                Message = "test",
                Person = personDto,
                Request = shiftTradeRequestDto,
                RequestedDate = date.Date,
                RequestedDateLocal = date.Date,
                RequestStatus = RequestStatusDto.Pending,
                Subject = "subject",
                UpdatedOn = date.Date
            };
			
            IPersonRequest personRequest = target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is IShiftTradeRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
        }
		
        [Test]
        public void VerifyDtoToDoShiftTradeRequestWithReload()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(person);
			var personAssembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler { PersonAssembler = personAssembler };
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
			var existingRequest = new PersonRequest(person,new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {new ShiftTradeSwapDetail(person,person,date,date)})).WithId();
			personRequestRepository.Add(existingRequest);

			var personDto = new PersonDto { Id = person.Id };
			var shiftTradeRequestDto = new ShiftTradeRequestDto { Period = new DateTimePeriodDto() };

			ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto
			{
				DateFrom = new DateOnlyDto { DateTime = date.Date },
				DateTo = new DateOnlyDto { DateTime = date.Date },
				PersonFrom = personDto,
				PersonTo = personDto
			};
			shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);

			PersonRequestDto personRequestDto = new PersonRequestDto
			{
				Id = existingRequest.Id,
				CreatedDate = date.Date,
				Message = "test",
				Person = personDto,
				Request = shiftTradeRequestDto,
				RequestedDate = date.Date,
				RequestedDateLocal = date.Date,
				RequestStatus = RequestStatusDto.Pending,
				Subject = "subject",
				UpdatedOn = date.Date
			};

			var personRequest = target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is IShiftTradeRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
        }
		
        [Test]
        public void VerifyDoToDtoTextRequest()
        {
			var person = PersonFactory.CreatePerson().WithId();

	        var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
	        var dateTimePeriodAssembler = new DateTimePeriodAssembler();
	        var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
	        var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
	        var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
			var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
            var textRequest = new TextRequest(new DateTimePeriod());
            var personRequest = new PersonRequest(person, textRequest);
	        personRequest.TrySetMessage("ledig?");
			
            var personRequestDtoInReturn = target.DomainEntityToDto(personRequest);
            Assert.IsInstanceOf<TextRequestDto>(personRequestDtoInReturn.Request);
	        personRequestDtoInReturn.Message.Should().Be.EqualTo("ledig?");
        }
		
        [Test]
        public void VerifyDoToDtoAbsenceRequest()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var absence = AbsenceFactory.CreateAbsence("Sjuk").WithId();

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
	        var dateTimePeriodAssembler = new DateTimePeriodAssembler();
	        var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
	        var absenceRepository = new FakeAbsenceRepository();
			absenceRepository.Add(absence);
	        var absenceAssembler = new AbsenceAssembler(absenceRepository);
	        var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
			var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod());
            var personRequest = new PersonRequest(person, absenceRequest);
			
            var personRequestDtoInReturn = target.DomainEntityToDto(personRequest);
            Assert.IsInstanceOf<AbsenceRequestDto>(personRequestDtoInReturn.Request);
        }
		
        [Test]
        public void VerifyDoToDtoShiftTradeRequest()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);
			
			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
	        var dateTimePeriodAssembler = new DateTimePeriodAssembler();
	        var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
	        var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
	        var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
	        var shiftCategoryRepository = new FakeShiftCategoryRepository();
	        var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
	        var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
	        shiftTradeSwapDetailAssembler.PersonAssembler = personAssembler;
	        shiftTradeSwapDetailAssembler.SchedulePartAssembler = new SchedulePartAssembler(
		        new PersonAssignmentAssembler(shiftCategoryRepository,
			        new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
				        activityAssembler),
			        new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
				        new DateTimePeriodAssembler(), activityAssembler),
			        new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
				        new FakeMultiplicatorDefinitionSetRepository())),
		        new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler),
		        new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler),
		        new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler),
		        new ProjectedLayerAssembler(dateTimePeriodAssembler), dateTimePeriodAssembler,
		        new SdkProjectionServiceFactory(), new ScheduleTagAssembler(new FakeScheduleTagRepository()));

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(person, person, date, date);
            var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
            var personRequest = new PersonRequest(person, shiftTradeRequest);
			
            PersonRequestDto personRequestDtoInReturn = target.DomainEntityToDto(personRequest);
            Assert.IsInstanceOf<ShiftTradeRequestDto>(personRequestDtoInReturn.Request);
        }
		
        [Test]
        public void VerifyDoToDtoShiftTradeRequestCannotDelete()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
			shiftTradeSwapDetailAssembler.PersonAssembler = personAssembler;
			shiftTradeSwapDetailAssembler.SchedulePartAssembler = new SchedulePartAssembler(
				new PersonAssignmentAssembler(shiftCategoryRepository,
					new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
						activityAssembler),
					new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
						new DateTimePeriodAssembler(), activityAssembler),
					new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
						new FakeMultiplicatorDefinitionSetRepository())),
				new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler),
				new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler),
				new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler),
				new ProjectedLayerAssembler(dateTimePeriodAssembler), dateTimePeriodAssembler,
				new SdkProjectionServiceFactory(), new ScheduleTagAssembler(new FakeScheduleTagRepository()));

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(person, person, date, date);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(person, shiftTradeRequest);
			shiftTradeRequest.Accept(PersonFactory.CreatePerson(),new EmptyShiftTradeRequestSetChecksum(), new PersonRequestAuthorizationCheckerForTest());
			
            var personRequestDtoInReturn = target.DomainEntityToDto(personRequest);
            Assert.IsFalse(personRequestDtoInReturn.CanDelete);
        }
		
        [Test]
        public void VerifyDoToDtoShiftTradeRequestCanDelete()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
			shiftTradeSwapDetailAssembler.PersonAssembler = personAssembler;
			shiftTradeSwapDetailAssembler.SchedulePartAssembler = new SchedulePartAssembler(
				new PersonAssignmentAssembler(shiftCategoryRepository,
					new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
						activityAssembler),
					new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
						new DateTimePeriodAssembler(), activityAssembler),
					new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
						new FakeMultiplicatorDefinitionSetRepository())),
				new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler),
				new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler),
				new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler),
				new ProjectedLayerAssembler(dateTimePeriodAssembler), dateTimePeriodAssembler,
				new SdkProjectionServiceFactory(), new ScheduleTagAssembler(new FakeScheduleTagRepository()));

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(person, person, date, date);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(person, shiftTradeRequest);
			shiftTradeRequest.Accept(person,new EmptyShiftTradeRequestSetChecksum(), new PersonRequestAuthorizationCheckerForTest());
			
            var personRequestDtoInReturn = target.DomainEntityToDto(personRequest);
            Assert.IsTrue(personRequestDtoInReturn.CanDelete);
        }
		
        [Test]
        public void VerifyDoToDtoShiftTradeRequestCanDeleteReferred()
        {
			var person = PersonFactory.CreatePerson().WithId();
			var date = new DateOnly(2009, 9, 3);

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
			var dateTimePeriodAssembler = new DateTimePeriodAssembler();
			var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
			var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
			var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestAlwaysRefer();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
			var shiftCategoryRepository = new FakeShiftCategoryRepository();
			var activityAssembler = new ActivityAssembler(new FakeActivityRepository());
			var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(shiftCategoryRepository),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), activityAssembler,
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			
			shiftTradeSwapDetailAssembler.PersonAssembler = personAssembler;
			shiftTradeSwapDetailAssembler.SchedulePartAssembler = new SchedulePartAssembler(
				new PersonAssignmentAssembler(shiftCategoryRepository,
					new ActivityLayerAssembler<MainShiftLayer>(new MainShiftLayerConstructor(), dateTimePeriodAssembler,
						activityAssembler),
					new ActivityLayerAssembler<PersonalShiftLayer>(new PersonalShiftLayerConstructor(),
						new DateTimePeriodAssembler(), activityAssembler),
					new OvertimeLayerAssembler(dateTimePeriodAssembler, activityAssembler,
						new FakeMultiplicatorDefinitionSetRepository())),
				new PersonAbsenceAssembler(absenceAssembler, dateTimePeriodAssembler),
				new PersonDayOffAssembler(personAssembler, dateTimePeriodAssembler),
				new PersonMeetingAssembler(personAssembler, dateTimePeriodAssembler),
				new ProjectedLayerAssembler(dateTimePeriodAssembler), dateTimePeriodAssembler,
				new SdkProjectionServiceFactory(), new ScheduleTagAssembler(new FakeScheduleTagRepository()));

			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(person, person, date, date);
			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
			var personRequest = new PersonRequest(person, shiftTradeRequest);
			shiftTradeRequest.Accept(person,new EmptyShiftTradeRequestSetChecksum(), new PersonRequestAuthorizationCheckerForTest());
			
            var personRequestDtoInReturn = target.DomainEntityToDto(personRequest);
            Assert.IsTrue(personRequestDtoInReturn.CanDelete);
        }

		[Test]
		public void VerifyDoToDtoSwallowAutodeny()
		{
			var person = PersonFactory.CreatePerson().WithId();
			var absence = AbsenceFactory.CreateAbsence("Sjuk").WithId();

			var cultureProvider = new TestCultureProvider(CultureInfo.GetCultureInfo(1053));
	        var dateTimePeriodAssembler = new DateTimePeriodAssembler();
	        var textRequestAssembler = new TextRequestAssembler(cultureProvider, dateTimePeriodAssembler);
	        var absenceAssembler = new AbsenceAssembler(new FakeAbsenceRepository());
	        var absenceRequestAssembler = new AbsenceRequestAssembler(cultureProvider, absenceAssembler, dateTimePeriodAssembler);
			var batchStatusChecker = new ShiftTradeRequestStatusCheckerForTestDoesNothing();
			var shiftTradeRequestAssembler =
				new ShiftTradeRequestAssembler(cultureProvider,
					new PersonRequestAuthorizationCheckerForTest(), dateTimePeriodAssembler,
					batchStatusChecker);
			var shiftTradeSwapDetailAssembler = new ShiftTradeSwapDetailAssembler();
			var personAssembler = new PersonAssembler(new FakePersonRepositoryLegacy(),
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					absenceAssembler), new PersonAccountUpdaterDummy());
			var personRequestRepository = new FakePersonRequestRepository();
			var target = new PersonRequestAssembler(textRequestAssembler, absenceRequestAssembler, shiftTradeRequestAssembler, shiftTradeSwapDetailAssembler, personAssembler, personRequestRepository, batchStatusChecker, new FakeUserTimeZone());
			

			AbsenceRequest absenceRequest = new AbsenceRequest(absence, new DateTimePeriod());
			IPersonRequest personRequest = new PersonRequest(person, absenceRequest);
			personRequest.Deny(string.Empty, new PersonRequestAuthorizationCheckerForTest(), person);
			Assert.IsTrue(personRequest.IsAutoDenied);
			
			PersonRequestDto personRequestDtoInReturn = target.DomainEntityToDto(personRequest);
			Assert.AreEqual(RequestStatusDto.Denied, personRequestDtoInReturn.RequestStatus);
		}
	}
}