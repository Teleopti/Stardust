using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
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
        private DenyRequestCommandHandler _target;
        private DenyRequestCommandDto _denyRequestCommandDto;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _personRequestRepository = _mock.StrictMock<IPersonRequestRepository>();
            _authorization = _mock.StrictMock<IPersonRequestCheckAuthorization>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = _mock.DynamicMock<ICurrentUnitOfWorkFactory>();
            _target = new DenyRequestCommandHandler(_personRequestRepository,_authorization,_currentUnitOfWorkFactory);
            _denyRequestCommandDto = new DenyRequestCommandDto {PersonRequestId = Guid.NewGuid()};
        }

        [Test]
        public void ShouldDeniedRequestSuccessfully()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var request = _mock.StrictMock<IPersonRequest>();
            
            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRequestRepository.Get(_denyRequestCommandDto.PersonRequestId)).Return(request);
                Expect.Call(() => request.Deny(null, "Test", _authorization)).IgnoreArguments();
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_denyRequestCommandDto);
            }
        }

        [Test]
        [ExpectedException(typeof(FaultException))]
        public void ShouldThrowExceptionIfRequestIsInvalid()
        {
            var unitOfWork = _mock.StrictMock<IUnitOfWork>();
            var request = _mock.StrictMock<IPersonRequest>();

            using (_mock.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_personRequestRepository.Get(_denyRequestCommandDto.PersonRequestId)).Return(request);
                Expect.Call(() => request.Deny(null, "Test", _authorization)).IgnoreArguments().Throw(new InvalidRequestStateTransitionException());
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(unitOfWork.Dispose);
            }
            using (_mock.Playback())
            {
                _target.Handle(_denyRequestCommandDto);
            }
        }
    }
}
