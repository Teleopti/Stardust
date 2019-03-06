using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{

    [TestFixture]
    [Category("BucketB")]
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
        /// <param name="currentUnitOfWork">The unit of work.</param>
        /// <returns></returns>
        protected override Repository<IPushMessage> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return PushMessageRepository.DONT_USE_CTOR(currentUnitOfWork);
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
			var rep = new PushMessagePersister(PushMessageRepository.DONT_USE_CTOR(UnitOfWork), dialogueRepository, null);
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
            _pushMessage = new PushMessage();
            IList<IPerson> receivers = new List<IPerson>();
            ISendPushMessageReceipt receipt = _mocks.StrictMock<ISendPushMessageReceipt>();
            ICreatePushMessageDialoguesService service = _mocks.StrictMock<ICreatePushMessageDialoguesService>();
			var rep = new PushMessagePersister(PushMessageRepository.DONT_USE_CTOR(UnitOfWork), dialogueRepository, service);
            
            IPushMessageDialogue createdDialogue = new PushMessageDialogue(_pushMessage,PersonFactory.CreatePerson("Receiver"));

            using(_mocks.Record())
            {
                Expect.Call(service.Create(_pushMessage, receivers)).Return(receipt);
                Expect.Call(receipt.CreatedDialogues).Return(new List<IPushMessageDialogue> {createdDialogue});
                Expect.Call(()=>dialogueRepository.Add(createdDialogue)); //Verifies that the created dialogue has been added to repository 
            }
            using(_mocks.Playback())
            {
                rep.Add(_pushMessage, receivers);
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
		public void ShouldNotUpdateNonChanged_Bug33806()
	    {
		    var pushMessage = new PushMessage();
			PersistAndRemoveFromUnitOfWork(pushMessage);
			var versionBeforeRead = pushMessage.Version;

		    var pushMessageRead = Session.Get<PushMessage>(pushMessage.Id.Value);
			PersistAndRemoveFromUnitOfWork(pushMessageRead);

			pushMessageRead.Version.Should().Be.EqualTo(versionBeforeRead);
			UnitOfWork.IsDirty().Should().Be.False();
	    }
    }
}
