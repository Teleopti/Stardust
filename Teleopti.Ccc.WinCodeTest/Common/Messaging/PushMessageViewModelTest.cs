using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.WinCode.Common.Messaging;
using Teleopti.Ccc.WinCodeTest.Helpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Messaging
{
    [TestFixture]
    public class PushMessageViewModelTest
    {
        private IPushMessage _pushMessage;
        private string _title;
        private string _message;
        private PushMessageViewModel _target;

        [SetUp]
        public void Setup()
        {
            _pushMessage = new PushMessage();
            _title = "title";
            _message = "message";
            _pushMessage.Title = _title;
            _pushMessage.Message = _message;
            _target = new PushMessageViewModel(_pushMessage);
        }
	
        [Test]
        public void VerifyProperties()
        {
         
            Assert.AreEqual(_title,_target.Title);
            Assert.AreEqual(_message,_target.Message);
        }

        [Test]
        public void VerifyChangesToViewModelAreDirectedToTheModelDirectly()
        {
            string newTitle = "newTitle";
            string newMessage = "newMessage";
            _target.Title = newTitle;
            _target.Message = newMessage;
            Assert.AreEqual(newTitle,_target.PushMessage.Title);
            Assert.AreEqual(newMessage, _target.PushMessage.Message);
        }

        [Test]
        public void VerifyNotifyPropertyChanged()
        {

            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.Title = _target.Title;
            _target.Message = _target.Message;
            
            Assert.IsFalse(listener.HasFired("Message"));
            Assert.IsFalse(listener.HasFired("Title"));
            Assert.IsFalse(listener.HasFired("CanSend"));
            
            _target.Title = _target.Title+"new";
            Assert.IsTrue(listener.HasFired("Title"));
            listener.Clear();
            _target.Message = _target.Message + "new";
            Assert.IsTrue(listener.HasFired("Message"));
            
        }

        [Test]
        public void VerifyCanSend()
        {
            _target.Message = "message";
            _target.Title = "title";
            Assert.IsTrue(_target.CanSend);
            _target.Title = string.Empty;
            Assert.IsFalse(_target.CanSend);
            _target.Message = string.Empty;
            _target.Title = "title";
            Assert.IsFalse(_target.CanSend);
        }

        [Test]
        public void VerifySendCreatesConversationAndPropertyChangeFires()
        {
            PropertyChangedListener listener = new PropertyChangedListener();
            listener.ListenTo(_target);
            _target.CreateNewConversation();
            Assert.AreNotSame(_pushMessage,_target.PushMessage);
            Assert.IsTrue(listener.HasFired("PushMessage"));
            Assert.IsTrue(listener.HasFired("Title"));
            Assert.IsTrue(listener.HasFired("Message"));
        }
    }
}
