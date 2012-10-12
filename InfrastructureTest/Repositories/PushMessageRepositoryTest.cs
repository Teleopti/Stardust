using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

    [TestFixture]
    [Category("LongRunning")]
    public class PushMessageRepositoryTest : RepositoryTest<IPushMessage>
    {
        private IPerson _sender;
        private readonly IList<string> _replyOptions = new List<string> {"option1", "option2"};
        private PushMessage _pushMessage;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _sender = PersonFactory.CreatePerson("sender");
            PersistAndRemoveFromUnitOfWork(_sender);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IPushMessage CreateAggregateWithCorrectBusinessUnit()
        {
            return new PushMessage(_replyOptions)
                       {
                           Message = "message",
                           Title = "title",
                           Sender = _sender,
                           AllowDialogueReply = false,
                           TranslateMessage = true};
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IPushMessage loadedAggregateFromDatabase)
        {
            IPushMessage pushMessage = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(pushMessage.GetMessage(new NoFormatting()),loadedAggregateFromDatabase.GetMessage(new NoFormatting()));
            Assert.AreEqual(pushMessage.GetTitle(new NoFormatting()),loadedAggregateFromDatabase.GetTitle(new NoFormatting()));
            Assert.AreEqual(pushMessage.Sender,loadedAggregateFromDatabase.Sender);
            Assert.AreEqual(pushMessage.ReplyOptions,loadedAggregateFromDatabase.ReplyOptions);
            Assert.AreEqual(pushMessage.AllowDialogueReply, loadedAggregateFromDatabase.AllowDialogueReply);
            Assert.AreEqual(pushMessage.TranslateMessage, loadedAggregateFromDatabase.TranslateMessage);
        }

        /// <summary>
        /// Tests the repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <returns></returns>
        protected override Repository<IPushMessage> TestRepository(IUnitOfWork unitOfWork)
        {
            return new PushMessageRepository(unitOfWork);
        }
        
        [Test]
        public void ShouldCreateWithFactory()
        {
            Assert.NotNull(new PushMessageRepository(UnitOfWorkFactory.Current,new PushMessageDialogueRepository(UnitOfWorkFactory.Current)));
        }

        [Test]
        public void VerifySenderReturnsCreatedByIfNull()
        {
            PushMessage pushMessage = new PushMessage();
            PersistAndRemoveFromUnitOfWork(pushMessage);
            Assert.AreEqual(pushMessage.Sender,pushMessage.CreatedBy);
        }

        [Test]
        public void VerifyRemovesConversationDialoguesConnectedToTheConversation()
        {
           
            IPushMessageDialogueRepository dialogueRepository = Mocks.StrictMock<IPushMessageDialogueRepository>();
            IPushMessageRepository rep = new PushMessageRepository(UnitOfWork, dialogueRepository);
            _pushMessage = new PushMessage();
            PersistAndRemoveFromUnitOfWork(_pushMessage);

            using (Mocks.Record())
            {
                dialogueRepository.Remove(_pushMessage);
            }
            using (Mocks.Playback())
            {
                rep.Remove(_pushMessage);
            }
        }

        [Test]
        public void VerifyUsesServiceToCreateDialoguesAndAddThemToDialogueRepository()
        {
            MockRepository _mocks = new MockRepository();
            IPushMessageDialogueRepository dialogueRepository = _mocks.StrictMock<IPushMessageDialogueRepository>();
            IPushMessageRepository rep = new PushMessageRepository(UnitOfWork, dialogueRepository);
            _pushMessage = new PushMessage();
            IList<IPerson> receivers = new List<IPerson>();
            ISendPushMessageReceipt receipt = _mocks.StrictMock<ISendPushMessageReceipt>();
            ICreatePushMessageDialoguesService service = _mocks.StrictMock<ICreatePushMessageDialoguesService>();
            
            IPushMessageDialogue createdDialogue = new PushMessageDialogue(_pushMessage,PersonFactory.CreatePerson("Receiver"));

            using(_mocks.Record())
            {
                Expect.Call(service.Create(_pushMessage, receivers)).Return(receipt);
                Expect.Call(receipt.CreatedDialogues).Return(new List<IPushMessageDialogue> {createdDialogue});
                Expect.Call(()=>dialogueRepository.Add(createdDialogue)); //Verifies that the created dialogue has been added to repository 
            }
            using(_mocks.Playback())
            {
                rep.Add(_pushMessage, receivers, service);
            }
        }

        [Test]
        public void VerifyFindByPerson()
        {
            IPerson anotherPerson = PersonFactory.CreatePerson("vsd");
        
            IPushMessage pushMessage1 = new PushMessage { Title = "title", Message = "message" };
            IPushMessage pushMessage2 = new PushMessage { Sender = _sender, Title = "title", Message = "message" };

            PersistAndRemoveFromUnitOfWork(_sender);
            PersistAndRemoveFromUnitOfWork(pushMessage1);
            PersistAndRemoveFromUnitOfWork(pushMessage2);
            PersistAndRemoveFromUnitOfWork(anotherPerson);

            IPushMessageRepository repository = new RepositoryFactory().CreatePushMessageRepository(UnitOfWork);

            IPerson createdBy = pushMessage1.CreatedBy;

            Assert.AreEqual(0, repository.Find(anotherPerson,new PagingDetail{Take = 10}).Count);
			Assert.AreEqual(1, repository.Find(createdBy, new PagingDetail { Take= 10 }).Count, "If sender is null, it should get CreatedBy");
			Assert.AreEqual(1, repository.Find(_sender, new PagingDetail { Take= 10 }).Count);

            IPushMessage pushMessage3 = new PushMessage { Sender = createdBy, Title = "title", Message = "message" };
            PersistAndRemoveFromUnitOfWork(pushMessage3);

			Assert.AreEqual(2, repository.Find(createdBy, new PagingDetail { Take= 10 }).Count, "Fetch both with createdby and sender");
        }

       
        [Test]
        public void VerifyFindDoesNotReadOnePushMessageForEveryReplyOption()
        {
            string replyOption1="one";
            string replyOption2="two";
            IPushMessage pushMessage2 = new PushMessage { Sender = _sender, Title = "title", Message = "message" };
            pushMessage2.ReplyOptions.Add(replyOption1);
            pushMessage2.ReplyOptions.Add(replyOption2);

            PersistAndRemoveFromUnitOfWork(_sender);
            PersistAndRemoveFromUnitOfWork(pushMessage2);
           
            IPushMessageRepository repository = new RepositoryFactory().CreatePushMessageRepository(UnitOfWork);
			Assert.AreEqual(1, repository.Find(_sender, new PagingDetail { Take = 10 }).Count);
        }

		[Test]
		public void ShouldGetCorrectCountOfUnreadMessages()
		{
			IPerson receiver = PersonFactory.CreatePerson("vsd");
			IPushMessage message = new PushMessage { Sender = _sender, Title = "title", Message = "message" };
			IPushMessageDialogue dialogue = new PushMessageDialogue(message, receiver);

			PersistAndRemoveFromUnitOfWork(_sender);
			PersistAndRemoveFromUnitOfWork(message);
			PersistAndRemoveFromUnitOfWork(receiver);
			PersistAndRemoveFromUnitOfWork(dialogue);

			IPushMessageRepository repository = new RepositoryFactory().CreatePushMessageRepository(UnitOfWork);
			repository.CountUnread(receiver).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetUnreadMessages()
		{
			IPerson receiver = PersonFactory.CreatePerson("vsd");
			IPushMessage message = new PushMessage { Sender = _sender, Title = "title", Message = "message" };
			IPushMessageDialogue dialogue = new PushMessageDialogue(message, receiver);

			PersistAndRemoveFromUnitOfWork(_sender);
			PersistAndRemoveFromUnitOfWork(message);
			PersistAndRemoveFromUnitOfWork(receiver);
			PersistAndRemoveFromUnitOfWork(dialogue);

			IPushMessageRepository repository = new RepositoryFactory().CreatePushMessageRepository(UnitOfWork);
			var unreadMessages = repository.FindUnreadMessages(null, receiver);
			unreadMessages.First().PushMessage.GetTitle(new NoFormatting()).Should().Be.EqualTo(
				message.GetTitle(new NoFormatting()));
		}

		[Test]
		public void ShouldGetUnreadMessagesInExpectedOrder()
		{
			IPerson receiver = PersonFactory.CreatePerson("vsd");

			IPushMessage messageOrderFirst = new PushMessage { Sender = _sender, Title = "title first", Message = "first message" };
			IPushMessageDialogue dialogueOrderFirst = new PushMessageDialogue(messageOrderFirst, receiver);

			IPushMessage messageOrderLast = new PushMessage { Sender = _sender, Title = "title last", Message = "last message" };
			IPushMessageDialogue dialogueOrderLast = new PushMessageDialogue(messageOrderLast, receiver);

			PersistAndRemoveFromUnitOfWork(_sender);
			PersistAndRemoveFromUnitOfWork(receiver);
			PersistAndRemoveFromUnitOfWork(messageOrderLast);
			PersistAndRemoveFromUnitOfWork(dialogueOrderLast);
			PersistAndRemoveFromUnitOfWork(messageOrderFirst);
			PersistAndRemoveFromUnitOfWork(dialogueOrderFirst);

			SetUpdatedOnForMessageDialogue(dialogueOrderLast, -1);

			IPushMessageRepository repository = new RepositoryFactory().CreatePushMessageRepository(UnitOfWork);
			var unreadMessages = repository.FindUnreadMessages(null, receiver);
			unreadMessages.First().PushMessage.GetTitle(new NoFormatting()).Should().Be.EqualTo(
				messageOrderFirst.GetTitle(new NoFormatting()));
			unreadMessages.Last().PushMessage.GetTitle(new NoFormatting()).Should().Be.EqualTo(
				messageOrderLast.GetTitle(new NoFormatting()));
		}


		[Test]
		public void ShouldGetUnreadMessagesWithPaging()
		{
			IPerson receiver = PersonFactory.CreatePerson("vsd");

			IPushMessage message1 = new PushMessage { Sender = _sender, Title = "title first", Message = "first message" };
			IPushMessageDialogue dialogue1 = new PushMessageDialogue(message1, receiver);

			IPushMessage message2 = new PushMessage { Sender = _sender, Title = "title last", Message = "last message" };
			IPushMessageDialogue dialogue2 = new PushMessageDialogue(message2, receiver);

			IPushMessage message3 = new PushMessage { Sender = _sender, Title = "title first", Message = "first message" };
			IPushMessageDialogue dialogue3 = new PushMessageDialogue(message3, receiver);

			IPushMessage message4 = new PushMessage { Sender = _sender, Title = "title last", Message = "last message" };
			IPushMessageDialogue dialogue4 = new PushMessageDialogue(message4, receiver);

			var paging = new Paging{Skip = 1, Take = 3};

			PersistAndRemoveFromUnitOfWork(_sender);
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

			IPushMessageRepository repository = new RepositoryFactory().CreatePushMessageRepository(UnitOfWork);
			var unreadMessages = repository.FindUnreadMessages(paging, receiver);

			unreadMessages.Should().Have.SameSequenceAs(new[] {dialogue2, dialogue3, dialogue4});

			unreadMessages = repository.FindUnreadMessages(null, receiver);

			unreadMessages.Should().Have.SameSequenceAs(new[] { dialogue1 ,dialogue2, dialogue3, dialogue4 });
		}

		private void SetUpdatedOnForMessageDialogue(IPushMessageDialogue messageDialogue, int minutes)
		{
			Session.CreateSQLQuery("UPDATE dbo.PushMessageDialogue SET UpdatedOn = DATEADD(mi,:Minutes,UpdatedOn) WHERE Id=:Id;").SetGuid(
				"Id", messageDialogue.Id.GetValueOrDefault()).SetInt32("Minutes", minutes).ExecuteUpdate();
		}
    }
}
