using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
    [TestFixture]
    public class DenyRequestCommandHandlerTest
    {
        private MockRepository _mock;
        private IPersonRequestRepository _personRequestRepository;
        private IPersonRequestCheckAuthorization _authorization;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IMessageBrokerEnablerFactory _messageBrokerEnablerFactory;
        private DenyRequestCommandHandler _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personRequestRepository = _mock.StrictMock<IPersonRequestRepository>();
            _authorization = _mock.StrictMock<IPersonRequestCheckAuthorization>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _messageBrokerEnablerFactory = _mock.StrictMock<IMessageBrokerEnablerFactory>();
            _target = new DenyRequestCommandHandler(_personRequestRepository,_authorization,_unitOfWorkFactory,_messageBrokerEnablerFactory);
        }

        [Test]
        public void ShouldDeniedRequestSuccessfully()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var request = _mock.StrictMock<IPersonRequest>();
            var denyRequestCommandDto = new DenyRequestCommandDto();
            denyRequestCommandDto.PersonRequestId = Guid.NewGuid();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_personRequestRepository.Get(denyRequestCommandDto.PersonRequestId)).Return(request);
                Expect.Call(() => request.Deny(null, "Test", _authorization)).IgnoreArguments();
                Expect.Call(() => _messageBrokerEnablerFactory.NewMessageBrokerEnabler());
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);

            }
            using (_mock.Playback())
            {
                _target.Handle(denyRequestCommandDto);
            }
        }
    }
}
