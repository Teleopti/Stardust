using System;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.Message.DataProvider
{
	[TestFixture]
	public class PushMessageDialoguePersisterTest
	{
	
		[Test]
		public void ShouldLoadDialogueFromRepositoryWithTheIdFromTheConfirmMessageViewModel()
		{
			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			var target = new PushMessageDialoguePersister(pushMessageDialogueRepository, SetupMapper(), null);
			var pushMessageDialogue = new PushMessageDialogue(new PushMessage(), new Person());
			var id = new Guid();
			pushMessageDialogue.SetId(id);

			pushMessageDialogueRepository.Expect(x => x.Get(id))
				.Return(pushMessageDialogue)
				.Repeat.Once();

			target.PersistMessage(new ConfirmMessageViewModel() { Id = id });
		}

		[Test]
		public void ShouldConfirmIsRead()
		{
			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			var target = new PushMessageDialoguePersister(pushMessageDialogueRepository, SetupMapper(), null);

			var pushMessage = new PushMessage(new []{"OK"});

			var pushMessageDialogue = new PushMessageDialogue(pushMessage, new Person());
			var id = new Guid();
			pushMessageDialogue.SetId(id);
		
			pushMessageDialogueRepository.Stub(x => x.Get(id)).Return(pushMessageDialogue);

			var viewModel = target.PersistMessage(new ConfirmMessageViewModel() { Id = id });

         viewModel.IsRead.Should().Be.True();
		}

		[Test]
		public void ReplyingToMessageShouldAddThatReplyToTheConversationOnTheDialogue()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var user = new Person();
			user.SetId(Guid.NewGuid());
			var sender = new Person();
			loggedOnUser.Stub(l => l.CurrentUser()).Return(user);
			var dialogueId = Guid.NewGuid();
			var pushMessage = new PushMessage(new[] { "OK" }) { Sender = sender };
			var pushMessageDialogue = new PushMessageDialogue(pushMessage, sender);
			pushMessageDialogue.SetId(dialogueId);
			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			pushMessageDialogueRepository.Stub(x => x.Get(dialogueId)).Return(pushMessageDialogue);
			var target = new PushMessageDialoguePersister(pushMessageDialogueRepository, SetupMapper(), loggedOnUser);
			string newMessage="new message!!";
			var viewModel = target.PersistMessage(new ConfirmMessageViewModel() { Id = dialogueId, Reply = newMessage });

			Assert.That(viewModel.DialogueMessages.First().Text,Is.EqualTo(newMessage));
		}

		[Test]
		public void ReplyingToMessageShouldSetLoggedOnUserAsSenderToThatReply()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var user = new Person();
			user.SetId(Guid.NewGuid());			
			var sender = new Person();
			loggedOnUser.Expect(l => l.CurrentUser()).Return(user);
			var dialogueId = Guid.NewGuid();
			var pushMessage = new PushMessage(new[] { "OK" }) { Sender = sender };
			var confirmMessage = new ConfirmMessageViewModel() { Id = dialogueId, Reply = "the reply" };
			var pushMessageDialogue = new PushMessageDialogue(pushMessage, sender);

			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			pushMessageDialogueRepository.Stub(x => x.Get(dialogueId)).Return(pushMessageDialogue);
			var target = new PushMessageDialoguePersister(pushMessageDialogueRepository, SetupMapper(), loggedOnUser);

			var result = target.PersistMessage(confirmMessage);
			Assert.That(result.DialogueMessages.First(m => m.Text.Equals("the reply")).SenderId, Is.EqualTo(user.Id));

		}

		private static IMappingEngine SetupMapper()
		{
			Mapper.Initialize(c => c.AddProfile(new MessageViewModelMappingProfile(
				() => new UserTimeZone(null)
				)));

			return Mapper.Engine;
		}
	}
}
