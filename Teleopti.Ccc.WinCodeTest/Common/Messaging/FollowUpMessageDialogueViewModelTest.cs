using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
    [TestFixture]
    public class FollowUpMessageDialogueViewModelTest
    {
        private FollowUpMessageDialogueViewModel _target;
        private IPushMessageDialogue _dialogue;
        private IPushMessage _pushMessage;
        private IPerson _sender;
        private IPerson _receiver;
        private TesterForCommandModels _testerForCommandModels;
        private MockRepository _mocker;
        private string _dialogueMessage;
    	private IRepositoryFactory _repositoryFactory;
    	private IUnitOfWorkFactory _unitOfWorkFactory;

    	[SetUp]
        public void Setup()
        {
            _dialogueMessage = "_dialogueMessage";
            _sender = PersonFactory.CreatePerson();
            _mocker = new MockRepository();

			_repositoryFactory = _mocker.StrictMock<IRepositoryFactory>();
			_unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();

            _receiver = PersonFactory.CreatePerson("tjoho");
            _pushMessage = new PushMessage {Sender = _sender};
            _pushMessage.SetId(Guid.NewGuid());
            _dialogue = new PushMessageDialogue(_pushMessage,_receiver);
            _dialogue.DialogueReply(_dialogueMessage, _sender);
            _target = new FollowUpMessageDialogueViewModel(_dialogue,_repositoryFactory,_unitOfWorkFactory);
            
            _testerForCommandModels = new TesterForCommandModels();
        }

        [Test]
        public void VerifyConstructorAndProperties()
        {
            Assert.AreEqual(_receiver,_target.Receiver);
            Assert.AreEqual(_dialogue.IsReplied,_target.IsReplied);
			Assert.AreEqual(_dialogue.GetReply(new NoFormatting()), _target.GetReply(new NoFormatting()));
        }

        [Test]
        public void VerifySendReplyCanExecute()
        {
            Assert.IsFalse(_target.AllowDialogueReply);
            _target.AllowDialogueReply = true;
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.SendReply));
            _target.ReplyText = "test";
            Assert.IsTrue(_testerForCommandModels.CanExecute(_target.SendReply));

            _target.AllowDialogueReply = false;
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.SendReply));

            //Text
            Assert.AreEqual(UserTexts.Resources.Send,_target.SendReply.Text);
        }

        [Test]
        public void VerifySendReplyOnExecute()
        {
            string newText = "newText";

            IPushMessageDialogueRepository repository = _mocker.StrictMock<IPushMessageDialogueRepository>();
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();

            _target = new FollowUpMessageDialogueViewModel(_dialogue,_repositoryFactory,_unitOfWorkFactory);
            int dialogueMessagesFromStart = _target.Messages.Count;
            _target.ReplyText = newText;
            using(_mocker.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreatePushMessageDialogueRepository(uow)).Return(repository);
                Expect.Call(() => uow.Reassociate(_pushMessage)); //We need to reassociate to get all the properties we need (sender) instead of putting it in the repository
                Expect.Call(() => repository.Add(_dialogue));
                Expect.Call(uow.PersistAll()).Return(new List<IRootChangeInfo>()).Repeat.AtLeastOnce(); //make sure the changes are persisted
                uow.Dispose(); //And disposed
            }
            using(_mocker.Playback())
            {
                _testerForCommandModels.ExecuteCommandModel(_target.SendReply);
                var newMessage = _target.Messages.First(m => m.Text == newText);
                Assert.AreEqual(dialogueMessagesFromStart+1, _target.Messages.Count);
                Assert.AreEqual(_pushMessage.Sender, newMessage.Sender);
               Assert.AreEqual(string.Empty,_target.ReplyText,"text should have been cleared");
            }
        }

        [Test]
        public void VerifyDeleteCommand()
        {
           Assert.IsTrue(_testerForCommandModels.CanExecute(_target.DeleteDialogue));
           Assert.AreEqual(UserTexts.Resources.Delete,_target.DeleteDialogue.Text);
        }

        [Test]
        public void VerifyDeleteCommandOnExecute()
        {
            IPushMessageDialogueRepository repository = _mocker.StrictMock<IPushMessageDialogueRepository>();
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();

            using (_mocker.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(_repositoryFactory.CreatePushMessageDialogueRepository(uow)).Return(repository);
                Expect.Call(() => repository.Remove(_dialogue));
                Expect.Call(uow.PersistAll()).Return(new List<IRootChangeInfo>()).Repeat.AtLeastOnce(); //make sure the changes are persisted
                uow.Dispose(); //And disposed
            }
            using (_mocker.Playback())
            {
				_target = new FollowUpMessageDialogueViewModel(_dialogue, _repositoryFactory, _unitOfWorkFactory);
                _testerForCommandModels.ExecuteCommandModel(_target.DeleteDialogue);
            }
        }
    }
}
