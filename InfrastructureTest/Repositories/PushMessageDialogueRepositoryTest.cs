using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{


    /// <summary>
    /// Tests AlarmTypeRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
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
        /// <param name="unitOfWork">The unit of work.</param>
        /// <returns></returns>
        protected override Repository<IPushMessageDialogue> TestRepository(IUnitOfWork unitOfWork)
        {
            return new PushMessageDialogueRepository(unitOfWork);
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

        

    }
}
