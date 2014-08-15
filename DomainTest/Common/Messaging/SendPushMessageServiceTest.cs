using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common.Messaging
{
    [TestFixture]
    public class SendPushMessageServiceTest
    {
        private IPerson _receiver1;
        private IPerson _receiver2;
        private IPerson _sender;
        private IPushMessage _pushMessage;
        private SendPushMessageService _sendPushMessage;
        private MockRepository _mocker;
		private IPushMessagePersister _persister;
        private IPushMessageDialogueRepository _dialogueRepository;
        private string _option1 = "option1";
        private string _option2 = "option2";
	    private IPushMessageRepository _repository;

	    [SetUp]
        public void Setup()
        {
            _sender = new Person();
            _receiver1 = new Person();
            _receiver2 = new Person();
            _pushMessage = new PushMessage();
            _sendPushMessage = new SendPushMessageService(_pushMessage);
            _mocker = new MockRepository();
			_persister = _mocker.StrictMock<IPushMessagePersister>();
			_repository = _mocker.StrictMock<IPushMessageRepository>();
			_dialogueRepository = _mocker.StrictMock<IPushMessageDialogueRepository>();
        }

        [Test]
        public void VerifySenderReceiver()
        {
            IList<IPerson> people = new List<IPerson> { _receiver1, _receiver2 };
            _sendPushMessage.To(_receiver1).To(_receiver1).To(_receiver2).From(_sender);
            Assert.AreEqual(_sender, _pushMessage.Sender);
            Assert.Contains(_receiver1, _sendPushMessage.Receivers);
            Assert.Contains(_receiver2, _sendPushMessage.Receivers);
            Assert.AreEqual(1, _sendPushMessage.Receivers.Count(p => p == _receiver1), "Only unique recievers");
            _sendPushMessage = new SendPushMessageService(_pushMessage);
            _sendPushMessage.To(people);
            Assert.Contains(_receiver1, _sendPushMessage.Receivers);
            Assert.Contains(_receiver2, _sendPushMessage.Receivers);
            //Verify clear
            _sendPushMessage.ClearReceivers();
            Assert.IsEmpty( _sendPushMessage.Receivers);
        }

        [Test]
        public void VerifyCreateConversation()
        {
            using (_mocker.Record())
            {
                Expect.Call(() => _persister.Add(_pushMessage,new List<IPerson>())).IgnoreArguments();
            }
            using (_mocker.Playback())
            {
                SendPushMessageService.CreateConversation("Title", "Message", false).From(_sender).To(_receiver1).SendConversation(_persister);
            }
        }

        [Test]
        public void VerifySendGetsCalled()
        {
            SendPushMessageService sendPushMessage = new SendPushMessageService(_pushMessage);

            //Todo check the conversation and receiver of the new conversationdialogue....
            PushMessageDialogue dialogue = new PushMessageDialogue(_pushMessage,_receiver1);
			
            using(_mocker.Record())
            {
				Expect.Call(()=>_repository.Add(_pushMessage));
                Expect.Call(()=>_dialogueRepository.Add(dialogue)).IgnoreArguments(); //do abetter check here! The only thing thats interesting is that conversation and receiver is correct
            }
            using(_mocker.Playback())
            {
				sendPushMessage.From(_sender).To(_receiver1).SendConversation(_repository, _dialogueRepository);

            }
        }

        [Test]
        public void VerifyOptions()
        {
            IList<string> options = new List<string> {_option2};
            ISendPushMessageService service = SendPushMessageService.CreateConversation("title", "message", true).AddReplyOption(_option1).AddReplyOption(options).TranslateMessage();
            Assert.IsTrue(service.PushMessage.ReplyOptions.Contains(_option1));
            Assert.IsTrue(service.PushMessage.ReplyOptions.Contains(_option2));
            Assert.AreEqual(service.PushMessage.ReplyOptions.Count,2);
            Assert.IsTrue(service.PushMessage.TranslateMessage);
        }
    }
}
