using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class SavePersonAbsenceRequestCommandHandlerTest
    {
        private IPersistPersonRequest _persistPersonRequest;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IPersonRequestRepository _personRequestRepository;
        private IServiceBusSender _serviceBusSender;
        private MockRepository _mock;
        private SavePersonAbsenceRequestCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _persistPersonRequest = _mock.StrictMock<IPersistPersonRequest>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _personRequestRepository = _mock.StrictMock<IPersonRequestRepository>();
            _serviceBusSender = _mock.StrictMock<IServiceBusSender>();
            _target = new SavePersonAbsenceRequestCommandHandler(_persistPersonRequest,_unitOfWorkFactory,_personRequestRepository,_serviceBusSender);
        }

        [Test]
        public void ShouldSavePersonAbsenceRequest()
        {
            var periodDto = new DateTimePeriodDto()
            {
                UtcStartTime = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UtcEndTime = new DateTime(2012, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            };

            var requestDto = new AbsenceRequestDto {Id = Guid.NewGuid(), Period = periodDto};

            var personRequestDto = new PersonRequestDto
                                       {
                                           Request = requestDto,
                                           RequestStatus = RequestStatusDto.Approved,
                                           RequestedDate = new DateTime(),
                                           RequestedDateLocal = new DateTime(),
                                           Subject = "test subject"
                                       };

            var savePersonAbsenceRequestCommandDto = new SavePersonAbsenceRequestCommandDto
                                                         {PersonRequestDto = personRequestDto};

            var person = PersonFactory.CreatePerson("test");
            person.SetId(Guid.NewGuid());

            var personRequest = new PersonRequest(person);
            personRequest.SetId(Guid.NewGuid());

            using (_mock.Record())
            {
                var unitOfWork = _mock.StrictMock<IUnitOfWork>();
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_serviceBusSender.EnsureBus()).Return(true);
                Expect.Call(_persistPersonRequest.Persist(savePersonAbsenceRequestCommandDto.PersonRequestDto,
                                                          unitOfWork, null)).IgnoreArguments().Return(personRequest);
                Expect.Call(()=>_serviceBusSender.NotifyServiceBus(new NewAbsenceRequestCreated())).IgnoreArguments();
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(savePersonAbsenceRequestCommandDto);
            }
        }
    }
}
