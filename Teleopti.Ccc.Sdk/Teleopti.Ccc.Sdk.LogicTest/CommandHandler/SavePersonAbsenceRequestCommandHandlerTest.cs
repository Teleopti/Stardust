using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SavePersonAbsenceRequestCommandHandlerTest
    {
        private IPersistPersonRequest _persistPersonRequest;
        private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private IPersonRequestRepository _personRequestRepository;
        private IEventPublisher _eventPublisher;
        private MockRepository _mock;
        private SavePersonAbsenceRequestCommandHandler _target;
        private readonly DateTimePeriodDto _periodDto = new DateTimePeriodDto()
        {
            UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
        };

        private AbsenceRequestDto _requestDto;
        private PersonRequestDto _personRequestDto;
        private SavePersonAbsenceRequestCommandDto _savePersonAbsenceRequestCommandDto;
        private IPerson _person;
        private IPersonRequest _personRequest;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _persistPersonRequest = _mock.StrictMock<IPersistPersonRequest>();
            _unitOfWorkFactory = _mock.StrictMock<ICurrentUnitOfWorkFactory>();
            _personRequestRepository = _mock.StrictMock<IPersonRequestRepository>();
			_eventPublisher = _mock.StrictMock<IEventPublisher>();
            _target = new SavePersonAbsenceRequestCommandHandler(_persistPersonRequest,_unitOfWorkFactory,_personRequestRepository,_eventPublisher);
            _requestDto = new AbsenceRequestDto { Id = Guid.NewGuid(), Period = _periodDto };

            _personRequestDto = new PersonRequestDto
            {
                Request = _requestDto,
                RequestStatus = RequestStatusDto.Approved,
                RequestedDate = new DateTime(),
                RequestedDateLocal = new DateTime(),
                Subject = "test subject"
            };

            _savePersonAbsenceRequestCommandDto = new SavePersonAbsenceRequestCommandDto { PersonRequestDto = _personRequestDto };

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());

            _personRequest = new PersonRequest(_person);
            _personRequest.SetId(Guid.NewGuid());
        }

        [Test]
        public void ShouldSavePersonAbsenceRequest()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_persistPersonRequest.Persist(_savePersonAbsenceRequestCommandDto.PersonRequestDto,
                                                          unitOfWork, null)).IgnoreArguments().Return(_personRequest);
                Expect.Call(()=>_eventPublisher.Publish(new NewAbsenceRequestCreatedEvent())).IgnoreArguments();
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_savePersonAbsenceRequestCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfOtherThanAbsenceRequest()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            _savePersonAbsenceRequestCommandDto.PersonRequestDto.Request = new TextRequestDto();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_savePersonAbsenceRequestCommandDto);
            }
        }

    }
}
