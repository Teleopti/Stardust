using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class PersonRequestAssemblerTest
    {
        private PersonRequestAssembler _target;
        private MockRepository _mocks;
        private IAssembler<IRequest, TextRequestDto> _textRequestAssembler;
        private IAssembler<IAbsenceRequest, AbsenceRequestDto> _absenceRequestAssembler;
        private IAssembler<IShiftTradeRequest,ShiftTradeRequestDto> _shiftTradeRequestAssembler;
        private IAssembler<IShiftTradeSwapDetail, ShiftTradeSwapDetailDto> _shiftTradeSwapDetailAssembler;
        private IPerson _person;
        private DateOnly _date;
        private IPersonRequestRepository _personRequestRepository;
        private IAbsence _absence;
        private IAssembler<IPerson, PersonDto> _personAssembler;
        private IBatchShiftTradeRequestStatusChecker _batchStatusChecker;

        [SetUp]
        public void Setup()
        {
            _person = PersonFactory.CreatePerson();
            _person.SetId(Guid.NewGuid());
            _date = new DateOnly(2009, 9, 3);
            _absence = AbsenceFactory.CreateAbsence("Sjuk");

            _mocks = new MockRepository();
            _textRequestAssembler = _mocks.StrictMock<IAssembler<IRequest, TextRequestDto>>();
            _absenceRequestAssembler = _mocks.StrictMock<IAssembler<IAbsenceRequest, AbsenceRequestDto>>();
            _shiftTradeRequestAssembler = _mocks.StrictMock<IAssembler<IShiftTradeRequest,ShiftTradeRequestDto>>();
            _batchStatusChecker = _mocks.StrictMock<IBatchShiftTradeRequestStatusChecker>();
            _shiftTradeSwapDetailAssembler = _mocks.StrictMock<IAssembler<IShiftTradeSwapDetail, ShiftTradeSwapDetailDto>>();
            _personAssembler = _mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
            _personRequestRepository = _mocks.StrictMock<IPersonRequestRepository>();
            _target = new PersonRequestAssembler(_textRequestAssembler, _absenceRequestAssembler, _shiftTradeRequestAssembler, _shiftTradeSwapDetailAssembler, _personAssembler, _batchStatusChecker);
            _target.PersonRequestRepository = _personRequestRepository;
        }

        [Test]
        public void VerifyDtoToDoTextRequestWithReload()
        {
            Guid id = Guid.NewGuid();
            PersonDto personDto = new PersonDto { Id = _person.Id };
            TextRequestDto textRequestDto = new TextRequestDto();
            TextRequest textRequest = new TextRequest(new DateTimePeriod());
            PersonRequestDto personRequestDto = new PersonRequestDto
                {
                    CreatedDate = _date.Date,
                    Message = "test",
                    Id = id,
                    Person = personDto,
                    Request = textRequestDto,
                    RequestedDate = _date.Date,
                    RequestedDateLocal = _date.Date,
                    RequestStatus = RequestStatusDto.Pending,
                    Subject = "subject",
                    UpdatedOn = _date.Date
                };

            IPersonRequest personRequest = new PersonRequest(_person, textRequest);
            personRequest.SetId(id);

            Expect.Call(_personRequestRepository.Load(id)).Return(personRequest);
            Expect.Call(_textRequestAssembler.DtoToDomainEntity((TextRequestDto)personRequestDto.Request)).Return(textRequest);

            _mocks.ReplayAll();
            personRequest = _target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDtoToDoTextRequest()
        {
            PersonDto personDto = new PersonDto { Id = _person.Id };
            TextRequestDto textRequestDto = new TextRequestDto();
            TextRequest textRequest = new TextRequest(new DateTimePeriod());
            PersonRequestDto personRequestDto = new PersonRequestDto
                {
                    CreatedDate = _date.Date,
                    Message = "test",
                    Person = personDto,
                    Request = textRequestDto,
                    RequestedDate = _date.Date,
                    RequestedDateLocal = _date.Date,
                    RequestStatus = RequestStatusDto.Pending,
                    Subject = "subject",
                    UpdatedOn = _date.Date
                };

            Expect.Call(_personAssembler.DtoToDomainEntity(personDto)).Return(_person);
            Expect.Call(_textRequestAssembler.DtoToDomainEntity((TextRequestDto)personRequestDto.Request)).Return(
                textRequest);

            _mocks.ReplayAll();
            IPersonRequest personRequest = _target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is TextRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDtoToDoAbsenceRequest()
        {
            PersonDto personDto = new PersonDto { Id = _person.Id };
            AbsenceRequestDto absenceRequestDto = new AbsenceRequestDto();
            IAbsenceRequest absenceRequest = _mocks.StrictMock<IAbsenceRequest>();
            PersonRequestDto personRequestDto = new PersonRequestDto
            {
                CreatedDate = _date.Date,
                Message = "test",
                Person = personDto,
                Request = absenceRequestDto,
                RequestedDate = _date.Date,
                RequestedDateLocal = _date.Date,
                RequestStatus = RequestStatusDto.Pending,
                Subject = "subject",
                UpdatedOn = _date.Date
            };

            Expect.Call(_personAssembler.DtoToDomainEntity(personDto)).Return(_person);
            Expect.Call(_absenceRequestAssembler.DtoToDomainEntity((AbsenceRequestDto)personRequestDto.Request)).Return(
                absenceRequest);
            absenceRequest.SetParent(null);
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            IPersonRequest personRequest = _target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is IAbsenceRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDtoToDoShiftTradeRequest()
        {
            PersonDto personDto = new PersonDto { Id = _person.Id };
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();

            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
            shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);

            IShiftTradeSwapDetail shiftTradeSwapDetail = _mocks.StrictMock<IShiftTradeSwapDetail>();
            IShiftTradeRequest shiftTradeRequest = _mocks.StrictMock<IShiftTradeRequest>();
            PersonRequestDto personRequestDto = new PersonRequestDto
            {
                CreatedDate = _date.Date,
                Message = "test",
                Person = personDto,
                Request = shiftTradeRequestDto,
                RequestedDate = _date.Date,
                RequestedDateLocal = _date.Date,
                RequestStatus = RequestStatusDto.Pending,
                Subject = "subject",
                UpdatedOn = _date.Date
            };

            Expect.Call(_personAssembler.DtoToDomainEntity(personDto)).Return(_person);
            Expect.Call(_shiftTradeSwapDetailAssembler.DtoToDomainEntity(shiftTradeSwapDetailDto)).Return(
                shiftTradeSwapDetail);
            Expect.Call(_shiftTradeRequestAssembler.DtoToDomainEntity((ShiftTradeRequestDto)personRequestDto.Request)).Return(
                shiftTradeRequest);
            shiftTradeRequest.ClearShiftTradeSwapDetails();
            shiftTradeRequest.AddShiftTradeSwapDetail(null);
            LastCall.IgnoreArguments();
            shiftTradeRequest.SetParent(null);
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();
            IPersonRequest personRequest = _target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is IShiftTradeRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDtoToDoShiftTradeRequestWithReload()
        {
            PersonDto personDto = new PersonDto { Id = _person.Id };
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();

            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto { Id = Guid.NewGuid() };
            shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);

            IShiftTradeSwapDetail shiftTradeSwapDetail = _mocks.StrictMock<IShiftTradeSwapDetail>();
            IShiftTradeRequest shiftTradeRequest = _mocks.StrictMock<IShiftTradeRequest>();
            PersonRequestDto personRequestDto = new PersonRequestDto
            {
                Id = Guid.NewGuid(),
                CreatedDate = _date.Date,
                Message = "test",
                Person = personDto,
                Request = shiftTradeRequestDto,
                RequestedDate = _date.Date,
                RequestedDateLocal = _date.Date,
                RequestStatus = RequestStatusDto.Pending,
                Subject = "subject",
                UpdatedOn = _date.Date
            };

            IPersonRequest personRequest = new PersonRequest(_person, shiftTradeRequest);
            personRequest.SetId(personRequestDto.Id);

            Expect.Call(_personRequestRepository.Load(personRequestDto.Id.GetValueOrDefault())).Return(personRequest);
            Expect.Call(_shiftTradeSwapDetailAssembler.DtoToDomainEntity(shiftTradeSwapDetailDto)).Return(shiftTradeSwapDetail);
            Expect.Call(_shiftTradeRequestAssembler.DtoToDomainEntity((ShiftTradeRequestDto)personRequestDto.Request)).Return(shiftTradeRequest);
            shiftTradeRequest.TextForNotification = "test";
            shiftTradeRequest.ClearShiftTradeSwapDetails();
            shiftTradeRequest.AddShiftTradeSwapDetail(null);
            LastCall.IgnoreArguments();

            _mocks.ReplayAll();

            personRequest = _target.DtoToDomainEntity(personRequestDto);
            Assert.AreEqual(personRequestDto.Id, personRequest.Id);
            Assert.AreEqual(personRequestDto.Message, personRequest.GetMessage(new NormalizeText()));
            Assert.AreEqual(personRequestDto.Person.Id, personRequest.Person.Id);
            Assert.IsTrue(personRequest.Request is IShiftTradeRequest);
            Assert.AreEqual(personRequestDto.Subject, personRequest.GetSubject(new NormalizeText()));
            Assert.AreEqual(string.Empty, personRequest.DenyReason);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoTextRequest()
        {
            TextRequestDto textRequestDto = new TextRequestDto();
            TextRequest textRequest = new TextRequest(new DateTimePeriod());
            IPersonRequest personRequest = new PersonRequest(_person, textRequest);

            Expect.Call(_textRequestAssembler.DomainEntityToDto(textRequest)).Return(
                textRequestDto);
            Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
            {
                Id = _person.Id,
                Name = _person.Name.ToString()
            }).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            PersonRequestDto personRequestDtoInReturn = _target.DomainEntityToDto(personRequest);
            Assert.AreEqual(textRequestDto, personRequestDtoInReturn.Request);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoAbsenceRequest()
        {
            AbsenceRequestDto absenceRequestDto = new AbsenceRequestDto();
            AbsenceRequest absenceRequest = new AbsenceRequest(_absence, new DateTimePeriod());
            IPersonRequest personRequest = new PersonRequest(_person, absenceRequest);

            Expect.Call(_absenceRequestAssembler.DomainEntityToDto(absenceRequest)).Return(
                absenceRequestDto);
            Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
            {
                Id = _person.Id,
                Name = _person.Name.ToString()
            });

            _mocks.ReplayAll();
            PersonRequestDto personRequestDtoInReturn = _target.DomainEntityToDto(personRequest);
            Assert.AreEqual(absenceRequestDto, personRequestDtoInReturn.Request);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoShiftTradeRequest()
        {
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();
            IShiftTradeSwapDetail shiftTradeSwapDetail = new ShiftTradeSwapDetail(_person, _person, _date, _date);
            ShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
            IPersonRequest personRequest = new PersonRequest(_person, shiftTradeRequest);

            Expect.Call(_shiftTradeRequestAssembler.DomainEntityToDto(shiftTradeRequest)).Return(
                shiftTradeRequestDto);
            Expect.Call(_shiftTradeSwapDetailAssembler.DomainEntityToDto(shiftTradeSwapDetail)).Return(
                shiftTradeSwapDetailDto);
            Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
                                                                                {
                                                                                    Id = _person.Id,
                                                                                    Name = _person.Name.ToString()
                                                                                }).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            PersonRequestDto personRequestDtoInReturn = _target.DomainEntityToDto(personRequest);
            Assert.AreEqual(shiftTradeRequestDto, personRequestDtoInReturn.Request);
            Assert.AreEqual(shiftTradeSwapDetailDto, ((ShiftTradeRequestDto)personRequestDtoInReturn.Request).ShiftTradeSwapDetails[0]);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoShiftTradeRequestCannotDelete()
        {
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();
            shiftTradeRequestDto.ShiftTradeStatus = ShiftTradeStatusDto.OkByBothParts;
            IShiftTradeSwapDetail shiftTradeSwapDetail = new ShiftTradeSwapDetail(_person, _person, _date, _date);
            ShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
            IPersonRequest personRequest = new PersonRequest(_person, shiftTradeRequest);

            Expect.Call(_shiftTradeRequestAssembler.DomainEntityToDto(shiftTradeRequest)).Return(
                shiftTradeRequestDto);
            Expect.Call(_shiftTradeSwapDetailAssembler.DomainEntityToDto(shiftTradeSwapDetail)).Return(
                shiftTradeSwapDetailDto);
            Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
            {
                Id = _person.Id,
                Name = _person.Name.ToString()
            }).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            PersonRequestDto personRequestDtoInReturn = _target.DomainEntityToDto(personRequest);
            Assert.AreEqual(shiftTradeRequestDto, personRequestDtoInReturn.Request);
            Assert.AreEqual(shiftTradeSwapDetailDto, ((ShiftTradeRequestDto)personRequestDtoInReturn.Request).ShiftTradeSwapDetails[0]);
            Assert.IsFalse(personRequestDtoInReturn.CanDelete);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoShiftTradeRequestCanDelete()
        {
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();
            shiftTradeRequestDto.ShiftTradeStatus = ShiftTradeStatusDto.OkByMe;
            IShiftTradeSwapDetail shiftTradeSwapDetail = new ShiftTradeSwapDetail(_person, _person, _date, _date);
            ShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
            IPersonRequest personRequest = new PersonRequest(_person, shiftTradeRequest);

            Expect.Call(_shiftTradeRequestAssembler.DomainEntityToDto(shiftTradeRequest)).Return(
                shiftTradeRequestDto);
            Expect.Call(_shiftTradeSwapDetailAssembler.DomainEntityToDto(shiftTradeSwapDetail)).Return(
                shiftTradeSwapDetailDto);
            Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
            {
                Id = _person.Id,
                Name = _person.Name.ToString()
            }).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            PersonRequestDto personRequestDtoInReturn = _target.DomainEntityToDto(personRequest);
            Assert.AreEqual(shiftTradeRequestDto, personRequestDtoInReturn.Request);
            Assert.AreEqual(shiftTradeSwapDetailDto, ((ShiftTradeRequestDto)personRequestDtoInReturn.Request).ShiftTradeSwapDetails[0]);
            Assert.IsTrue(personRequestDtoInReturn.CanDelete);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDoToDtoShiftTradeRequestCanDeleteReferred()
        {
            ShiftTradeSwapDetailDto shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
            ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();
            shiftTradeRequestDto.ShiftTradeStatus = ShiftTradeStatusDto.Referred;
            IShiftTradeSwapDetail shiftTradeSwapDetail = new ShiftTradeSwapDetail(_person, _person, _date, _date);
            ShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
            IPersonRequest personRequest = new PersonRequest(_person, shiftTradeRequest);

            Expect.Call(_shiftTradeRequestAssembler.DomainEntityToDto(shiftTradeRequest)).Return(
                shiftTradeRequestDto);
            Expect.Call(_shiftTradeSwapDetailAssembler.DomainEntityToDto(shiftTradeSwapDetail)).Return(
                shiftTradeSwapDetailDto);
            Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
            {
                Id = _person.Id,
                Name = _person.Name.ToString()
            }).Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            PersonRequestDto personRequestDtoInReturn = _target.DomainEntityToDto(personRequest);
            Assert.AreEqual(shiftTradeRequestDto, personRequestDtoInReturn.Request);
            Assert.AreEqual(shiftTradeSwapDetailDto, ((ShiftTradeRequestDto)personRequestDtoInReturn.Request).ShiftTradeSwapDetails[0]);
            Assert.IsTrue(personRequestDtoInReturn.CanDelete);
            _mocks.VerifyAll();
        }

		[Test]
		public void VerifyDoToDtoSwallowAutodeny()
		{
			AbsenceRequestDto absenceRequestDto = new AbsenceRequestDto();
			AbsenceRequest absenceRequest = new AbsenceRequest(_absence, new DateTimePeriod());
			IPersonRequest personRequest = new PersonRequest(_person, absenceRequest);
			personRequest.Deny(_person, string.Empty, new PersonRequestAuthorizationCheckerForTest());
			Assert.IsTrue(personRequest.IsAutoDenied);

			Expect.Call(_absenceRequestAssembler.DomainEntityToDto(absenceRequest)).Return(
				absenceRequestDto);
			Expect.Call(_personAssembler.DomainEntityToDto(_person)).Return(new PersonDto
			{
				Id = _person.Id,
				Name = _person.Name.ToString()
			});

			_mocks.ReplayAll();
			PersonRequestDto personRequestDtoInReturn = _target.DomainEntityToDto(personRequest);
			Assert.AreEqual(RequestStatusDto.Denied, personRequestDtoInReturn.RequestStatus);
			_mocks.VerifyAll();
		}


    }
}