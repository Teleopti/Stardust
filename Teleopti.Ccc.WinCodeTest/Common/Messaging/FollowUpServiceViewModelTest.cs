using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
    [TestFixture]
    public class FollowUpServiceViewModelTest
    {
        private FollowUpServiceViewModel _target;
        private IPushMessageRepository _repository;
        private IRepositoryFactory _repositoryFactory;
        private IPerson _person;
        private MockRepository _mocker;
        private TesterForCommandModels _testerForCommandModels;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _repository = _mocker.StrictMock<IPushMessageRepository>();
            _repositoryFactory = _mocker.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            _person = PersonFactory.CreatePerson();
            _target = new FollowUpServiceViewModel(_person, _repositoryFactory, _unitOfWorkFactory);
            _testerForCommandModels = new TesterForCommandModels();
        }

        [Test]
        public void VerifyLoadCommand()
        {
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();
            Assert.AreEqual(UserTexts.Resources.Load,_target.Load.Text);
            Assert.IsTrue(_testerForCommandModels.CanExecute(_target.Load), "make sure we can execute the command");
            using (_mocker.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreatePushMessageRepository(uow)).Return(_repository);
                Expect.Call(_repository.Find(_person,_target.PagingDetail)).Return(new List<IPushMessage> { new PushMessage(), new PushMessage() });
                Expect.Call(uow.Dispose);
            }
            using (_mocker.Playback())
            {
                _testerForCommandModels.ExecuteCommandModel(_target.Load);
                Assert.AreEqual(2, _target.PushMessages.Count, "the LoadCommand should have created a viewmodel for each conversation");
                Assert.AreEqual(_target,_target.PushMessages[0].Observables[0],"Target should have been added as observer");

                _target.Notify((FollowUpPushMessageViewModel)_target.PushMessages[0]);
                Assert.AreEqual(1, _target.PushMessages.Count, "The Notify should have removed it from the list");
            }
        }

        [Test]
        public void VerifyFilterIsConnectedToPushMessages()
        {
            Assert.AreEqual(_target.PushMessageFilter.TargetCollection, _target.PushMessages);
        }
    }
}
