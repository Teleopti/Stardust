using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common.Messaging
{
    [TestFixture]
    public class CreatePushMessageDialoguesServiceTest
    {
        private IPushMessage _pushMessage;
        private ICreatePushMessageDialoguesService _target;
        private IPerson _receiver1;
        private IPerson _receiver2;
        private IList<IPerson> _receivers;
        private ISendPushMessageReceipt _receipt;

        [SetUp]
        public void Setup()
        {
            _target = new CreatePushMessageDialoguesService();
            _pushMessage = new PushMessage();
            _receiver1 = PersonFactory.CreatePerson("Person1");
            _receiver2 = PersonFactory.CreatePerson("Person2");
            _receivers = new List<IPerson>() { _receiver1, _receiver2 };
            _receipt = _target.Create(_pushMessage, _receivers);

        }

        [Test]
        public void VerifyPushMessageReceipt()
        {
            Assert.AreEqual(_receipt.CreatedPushMessage, _pushMessage);
        }

        [Test]
        public void VerifyCreatesOneDialogueForEachReceiver()
        {
            Assert.IsTrue(_receipt.CreatedDialogues.Count == 2);
            Assert.IsTrue(_receipt.CreatedDialogues.Count(d => d.Receiver == _receiver1) == 1);
            Assert.IsTrue(_receipt.CreatedDialogues.Count(d => d.Receiver == _receiver2) == 1);
        }

        [Test]
        public void VerifyReceiptContainsAllAddedRoots()
        {
            Assert.IsTrue(_receipt.AddedRoots().OfType<IPushMessage>().ToList().Count == 1);
            Assert.IsTrue(_receipt.AddedRoots().OfType<IPushMessageDialogue>().ToList().Count == _receivers.Count);
        }

    }
}