using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Messaging;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
    [TestFixture]
    public class SendPushMessageViewModelTest
    {
        private SendPushMessageViewModel _target;
        private TesterForCommandModels _testerForCommandModels;
        private MockRepository _mocker;

        [SetUp]
        public void Setup()
        {
            _mocker = new MockRepository();
            _testerForCommandModels = new TesterForCommandModels();
            _target = new SendPushMessageViewModel();
        }


        [Test]
        public void VerifyCanSend()
        {
        
            SetupForSend();
            _target.Message = string.Empty;
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.SendMessageCommand), "Message must be set");

            SetupForSend();
            _target.Title = string.Empty;
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.SendMessageCommand), "Title must be true");
            
            SetupForSend();
            _target.SetReceivers(new List<IPerson>());
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.SendMessageCommand), "Must have at least one receiver");


            SetupForSend();
            _target.ReplyOptions.Clear();
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.SendMessageCommand),"must have at least one option");
        }

        [Test]
        public void VerifySendPersistPushMessage()
        {
            IUnitOfWork uow = _mocker.StrictMock<IUnitOfWork>();
            IRepositoryFactory repositoryfactory = _mocker.StrictMock<IRepositoryFactory>();
			var pushMessageRepository = _mocker.StrictMock<IPushMessageRepository>();
			ISendPushMessageService pushMessageService = _mocker.StrictMock<ISendPushMessageService>();
            IUnitOfWorkFactory unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            _target = new SendPushMessageViewModelForTest(repositoryfactory, unitOfWorkFactory);
            _target.Message = "not cleared";
            _target.Title = "not cleared";
            ((SendPushMessageViewModelForTest) _target).SendService = pushMessageService;

            using (_mocker.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(repositoryfactory.CreatePushMessageRepository(uow)).Return(pushMessageRepository);
                Expect.Call(() => pushMessageService.SendConversation(null)).IgnoreArguments();
                Expect.Call(uow.PersistAll()).Return(null);
                Expect.Call(uow.Dispose).Repeat.Once();
            }
            using (_mocker.Playback())
            {
                _testerForCommandModels.ExecuteCommandModel(_target.SendMessageCommand);

                //Make sure the gui is cleared:
                Assert.AreEqual(SendPushMessageViewModel.MessageProperty.DefaultMetadata.DefaultValue, _target.Message);
                Assert.AreEqual(SendPushMessageViewModel.TitleProperty.DefaultMetadata.DefaultValue, _target.Message);
            }
        }

        [Test]
        public void VerifySendPushMessageService()
        {
            SetupForSend();
            
            ISendPushMessageService service = _target.CreateSendPushMessageService();
			Assert.AreEqual(_target.Message, service.PushMessage.GetMessage(new NoFormatting()));
			Assert.AreEqual(_target.Title, service.PushMessage.GetTitle(new NoFormatting()));
            Assert.AreEqual(_target.Receivers, service.Receivers);
            Assert.AreEqual(_target.ReplyOptions, service.PushMessage.ReplyOptions);
            Assert.IsTrue(service.PushMessage.AllowDialogueReply);

            SetupForSend();
            _target.AllowDialogueReply = false;
            Assert.IsFalse(_target.CreateSendPushMessageService().PushMessage.AllowDialogueReply);
        }

        [Test]
        public void VerifyCanSetReceivers()
        {
            IList<IPerson> receivers = new List<IPerson>();
            IPerson person1 = PersonFactory.CreatePerson("P1");
            receivers.Add(person1);
            _target.SetReceivers(receivers);
            Assert.That(_target.Receivers.Contains(person1));
            receivers.Clear();
            _target.SetReceivers(receivers);
            Assert.That(_target.Receivers.IsEmpty());
        }

        [Test]
        public void VerifyAddReplyOptionCommand()
        {

            int defaultValue = _target.ReplyOptions.Count;
            Assert.IsTrue(_target.ReplyOptions.Contains(UserTexts.Resources.Ok), "Default is OK");
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.AddReplyOptionCommand));
            Assert.AreEqual(_target.AddReplyOptionCommand.Text, UserTexts.Resources.Add);
            string option = "new option";
            Assert.AreEqual(defaultValue, _target.ReplyOptions.Count);
            _target.ReplyOptionToAdd = option;
            _testerForCommandModels.ExecuteCommandModel(_target.AddReplyOptionCommand);
            Assert.IsTrue(_target.ReplyOptions.Contains(option));
            Assert.AreEqual(string.Empty,_target.ReplyOptionToAdd);//checked that text is cleared..
            _target.ReplyOptionToAdd = option;
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.AddReplyOptionCommand),"Canot add same");
        }

        [Test]
        public void VerifyRemoveOption()
        {
            string opt1 = "o1";
            string opt2 = "o2";
         
         
            ICollectionView view = CollectionViewSource.GetDefaultView(_target.ReplyOptions);
            _target.ReplyOptions.Clear();
            Assert.IsFalse(_testerForCommandModels.CanExecute(_target.RemoveReplyOptionCommand),"Cannot delete when not selected");

            _target.ReplyOptions.Add(opt1);
            _target.ReplyOptions.Add(opt2);

            CollectionListener<string> listener = new CollectionListener<string>(_target.ReplyOptions);

            view.MoveCurrentTo(opt2);  //Select the second element:
            Assert.IsTrue(_testerForCommandModels.CanExecute(_target.RemoveReplyOptionCommand), "Cannot delete when not selected");
            _testerForCommandModels.ExecuteCommandModel(_target.RemoveReplyOptionCommand);
            Assert.IsTrue(listener.RemovedItems.Contains(opt2));
        }

        [Test]
        public void VerifyCreateYesNoOption()
        {
            Assert.AreEqual(UserTexts.Resources.YesNoAnswer,_target.YesNoReplyCommand.Text);
            _testerForCommandModels.ExecuteCommandModel(_target.YesNoReplyCommand);
            Assert.AreEqual(2,_target.ReplyOptions.Count);
            Assert.IsTrue(_target.ReplyOptions.Contains(UserTexts.Resources.Yes));
            Assert.IsTrue(_target.ReplyOptions.Contains(UserTexts.Resources.No));
        }

        [Test]
        public void VerifyCreateOkReplyOption()
        {
            Assert.AreEqual(UserTexts.Resources.Ok, _target.OkReplyCommand.Text);
            _testerForCommandModels.ExecuteCommandModel(_target.OkReplyCommand);
            Assert.AreEqual(1, _target.ReplyOptions.Count);
            Assert.IsTrue(_target.ReplyOptions[0].Contains(UserTexts.Resources.Ok));
        }

        [Test]
        public void VerifyAllowDialogueReply()
        {
            Assert.IsTrue(_target.AllowDialogueReply, "should be true default");
        }

        #region helpers
     
        private class SendPushMessageViewModelForTest : SendPushMessageViewModel
        {
            public ISendPushMessageService SendService { get; set; }

            public SendPushMessageViewModelForTest(IRepositoryFactory repositoryfactory, IUnitOfWorkFactory unitOfWorkFactory)
                : base(repositoryfactory,unitOfWorkFactory)
            {
                
            }

            public override ISendPushMessageService CreateSendPushMessageService()
            {
                return SendService;
            }
        }

        /// <summary>
        /// Setups for CanSend=True 
        /// </summary>
        private void SetupForSend()
        {
           
            _target = new SendPushMessageViewModel();
            _target.Title = "not empty";
            _target.Message = "not empty";
            _target.ReplyOptions.Add("at least one option");
            _target.SetReceivers(new List<IPerson> { PersonFactory.CreatePerson() });
            Assert.IsTrue(_testerForCommandModels.CanExecute(_target.SendMessageCommand), "Could not initiate sendcommand, SendCommand.CanExecute==false");
        }
        #endregion
    }
}
