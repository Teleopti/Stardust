using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class PushMessageDialogueRepositoryTest : RepositoryTest<IPushMessageDialogue>
    {
        private PushMessage _pushMessage;
        private string _option;
        private IPerson _person;
        private string _messageText1 = "m1";
        
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _option = "option";
            IList<string> options = new List<string>() { _option };
            _pushMessage = new PushMessage(options) { Message = "message", Title = "title" };
            _person = PersonFactory.CreatePerson("sdfgs");

            PersistAndRemoveFromUnitOfWork(_person);
            PersistAndRemoveFromUnitOfWork(_pushMessage);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IPushMessageDialogue CreateAggregateWithCorrectBusinessUnit()
        {
          
            PushMessageDialogue dialogue = new PushMessageDialogue(_pushMessage,_person);
            dialogue.DialogueReply(_messageText1,_person);
            dialogue.SetReply(_option);
            return dialogue;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IPushMessageDialogue loadedAggregateFromDatabase)
        {
            IPushMessageDialogue pushMessageDialogue = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(pushMessageDialogue.PushMessage, loadedAggregateFromDatabase.PushMessage);
            Assert.AreEqual(pushMessageDialogue.Receiver, loadedAggregateFromDatabase.Receiver);
            Assert.AreEqual(_messageText1,loadedAggregateFromDatabase.DialogueMessages[0].Text);
            Assert.AreEqual(_person,loadedAggregateFromDatabase.DialogueMessages[0].Sender);
            Assert.AreEqual(_option,loadedAggregateFromDatabase.GetReply(new NoFormatting()));
        }

        /// <summary>
        /// Tests the repository.
        /// </summary>
        /// <param name="currentUnitOfWork">The unit of work.</param>
        /// <returns></returns>
        protected override Repository<IPushMessageDialogue> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return PushMessageDialogueRepository.DONT_USE_CTOR(currentUnitOfWork);
        }

      

        [Test]
		public void VerifyCanGetAllAndDeleteMessageDialoguesBasedOnConversation()
        {
            #region setup & persist
            IPushMessage anotherPushMessage = new PushMessage();
            IPushMessageDialogue pushMessageDialogue1 = new PushMessageDialogue(_pushMessage,_person);
            IPushMessageDialogue pushMessageDialogue2 = new PushMessageDialogue(_pushMessage,_person);
            IPushMessageDialogue pushMessageDialogueWithAnotherPushMessage = new PushMessageDialogue(anotherPushMessage, _person);
            pushMessageDialogue1.SetReply(_option);

		    PersistAndRemoveFromUnitOfWork(anotherPushMessage);
		    PersistAndRemoveFromUnitOfWork(pushMessageDialogue1);
            PersistAndRemoveFromUnitOfWork(pushMessageDialogue2);
            PersistAndRemoveFromUnitOfWork(pushMessageDialogueWithAnotherPushMessage);
		    IPushMessageDialogueRepository repository = new RepositoryFactory().CreatePushMessageDialogueRepository(UnitOfWork);
            #endregion

            IList<IPushMessageDialogue> allDialoguesBelongingToConversation = repository.Find(_pushMessage);
            IList<IPushMessageDialogue> allDialoguesBelongingToAnotherConversation = repository.Find(anotherPushMessage);
            IList<IPushMessageDialogue> allDialoguesUnrepliedForPerson =
                repository.FindAllPersonMessagesNotRepliedTo(_person);

            Assert.AreEqual(2, allDialoguesUnrepliedForPerson.Count);
		    Assert.AreEqual(2, allDialoguesBelongingToConversation.Count);
            Assert.AreEqual(1, allDialoguesBelongingToAnotherConversation.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(allDialoguesBelongingToConversation[0].DialogueMessages));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(allDialoguesBelongingToConversation[0].Receiver));

            //Delete:
            repository.Remove(_pushMessage);
            Session.Flush();
            Assert.AreEqual(0, repository.Find(_pushMessage).Count);
            Assert.AreEqual(1, repository.Find(anotherPushMessage).Count);
            repository.Remove(anotherPushMessage);
            Session.Flush();
            Assert.AreEqual(0, repository.Find(anotherPushMessage).Count);
		}

        [Test]
        public void ShouldGetCorrectCountOfUnreadMessages()
        {
            IPerson receiver = PersonFactory.CreatePerson("vsd");
            IPushMessage message = new PushMessage { Sender = _person, Title = "title", Message = "message" };
            IPushMessageDialogue dialogue = new PushMessageDialogue(message, receiver);

            PersistAndRemoveFromUnitOfWork(message);
            PersistAndRemoveFromUnitOfWork(receiver);
            PersistAndRemoveFromUnitOfWork(dialogue);

            IPushMessageDialogueRepository repository = new RepositoryFactory().CreatePushMessageDialogueRepository(UnitOfWork);
            repository.CountUnread(receiver).Should().Be.EqualTo(1);
        }

        [Test]
        public void ShouldGetUnreadMessages()
        {
            IPerson receiver = PersonFactory.CreatePerson("vsd");
            IPushMessage message = new PushMessage { Sender = _person, Title = "title", Message = "message" };
            IPushMessageDialogue dialogue = new PushMessageDialogue(message, receiver);

            PersistAndRemoveFromUnitOfWork(message);
            PersistAndRemoveFromUnitOfWork(receiver);
            PersistAndRemoveFromUnitOfWork(dialogue);

            IPushMessageDialogueRepository repository = new RepositoryFactory().CreatePushMessageDialogueRepository(UnitOfWork);
            var unreadMessages = repository.FindUnreadMessages(new Paging(), receiver);
            unreadMessages.First().PushMessage.GetTitle(new NoFormatting()).Should().Be.EqualTo(
                message.GetTitle(new NoFormatting()));
        }

        [Test]
        public void ShouldGetUnreadMessagesInExpectedOrder()
        {
            IPerson receiver = PersonFactory.CreatePerson("vsd");

            IPushMessage messageOrderFirst = new PushMessage { Sender = _person, Title = "title first", Message = "first message" };
            IPushMessageDialogue dialogueOrderFirst = new PushMessageDialogue(messageOrderFirst, receiver);

            IPushMessage messageOrderLast = new PushMessage { Sender = _person, Title = "title last", Message = "last message" };
            IPushMessageDialogue dialogueOrderLast = new PushMessageDialogue(messageOrderLast, receiver);

            PersistAndRemoveFromUnitOfWork(receiver);
            PersistAndRemoveFromUnitOfWork(messageOrderLast);
            PersistAndRemoveFromUnitOfWork(dialogueOrderLast);
            PersistAndRemoveFromUnitOfWork(messageOrderFirst);
            PersistAndRemoveFromUnitOfWork(dialogueOrderFirst);

            SetUpdatedOnForMessageDialogue(dialogueOrderLast, -1);

            IPushMessageDialogueRepository repository = new RepositoryFactory().CreatePushMessageDialogueRepository(UnitOfWork);
            var unreadMessages = repository.FindUnreadMessages(new Paging(), receiver);
            unreadMessages.First().PushMessage.GetTitle(new NoFormatting()).Should().Be.EqualTo(
                messageOrderFirst.GetTitle(new NoFormatting()));
            unreadMessages.Last().PushMessage.GetTitle(new NoFormatting()).Should().Be.EqualTo(
                messageOrderLast.GetTitle(new NoFormatting()));
        }


        [Test]
        public void ShouldGetUnreadMessagesWithPaging()
        {
            IPerson receiver = PersonFactory.CreatePerson("vsd");

            IPushMessage message1 = new PushMessage { Sender = _person, Title = "title first", Message = "first message" };
            IPushMessageDialogue dialogue1 = new PushMessageDialogue(message1, receiver);

            IPushMessage message2 = new PushMessage { Sender = _person, Title = "title last", Message = "last message" };
            IPushMessageDialogue dialogue2 = new PushMessageDialogue(message2, receiver);

            IPushMessage message3 = new PushMessage { Sender = _person, Title = "title first", Message = "first message" };
            IPushMessageDialogue dialogue3 = new PushMessageDialogue(message3, receiver);

            IPushMessage message4 = new PushMessage { Sender = _person, Title = "title last", Message = "last message" };
            IPushMessageDialogue dialogue4 = new PushMessageDialogue(message4, receiver);

            var paging = new Paging { Skip = 1, Take = 3 };

            PersistAndRemoveFromUnitOfWork(receiver);
            PersistAndRemoveFromUnitOfWork(message1);
            PersistAndRemoveFromUnitOfWork(dialogue1);
            SetUpdatedOnForMessageDialogue(dialogue1, -1);
            PersistAndRemoveFromUnitOfWork(message2);
            PersistAndRemoveFromUnitOfWork(dialogue2);
            SetUpdatedOnForMessageDialogue(dialogue2, -2);
            PersistAndRemoveFromUnitOfWork(message3);
            PersistAndRemoveFromUnitOfWork(dialogue3);
            SetUpdatedOnForMessageDialogue(dialogue3, -3);
            PersistAndRemoveFromUnitOfWork(message4);
            PersistAndRemoveFromUnitOfWork(dialogue4);
            SetUpdatedOnForMessageDialogue(dialogue4, -4);

            IPushMessageDialogueRepository repository = new RepositoryFactory().CreatePushMessageDialogueRepository(UnitOfWork);
            var unreadMessages = repository.FindUnreadMessages(paging, receiver);

            unreadMessages.Should().Have.SameSequenceAs(new[] { dialogue2, dialogue3, dialogue4 });

            unreadMessages = repository.FindUnreadMessages(new Paging(), receiver);

            unreadMessages.Should().Have.SameSequenceAs(new[] { dialogue1, dialogue2, dialogue3, dialogue4 });
        }

        private void SetUpdatedOnForMessageDialogue(IPushMessageDialogue messageDialogue, int minutes)
        {
            Session.CreateSQLQuery("UPDATE dbo.PushMessageDialogue SET UpdatedOn = DATEADD(mi,:Minutes,UpdatedOn) WHERE Id=:Id;").SetGuid(
                "Id", messageDialogue.Id.GetValueOrDefault()).SetInt32("Minutes", minutes).ExecuteUpdate();
        }

    }
}
