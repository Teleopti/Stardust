﻿using System;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Message.DataProvider
{
	[TestFixture]
	public class PushMessageDialoguePersisterTest
	{
		private ILoggedOnUser _loggedOnUser;

		[Test]
		public void ShouldLoadDialogueFromRepositoryWithTheIdFromTheConfirmMessageViewModel()
		{
			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			var target = CreateTarget(pushMessageDialogueRepository);
			var pushMessageDialogue = new PushMessageDialogue(new PushMessage(), new Person());
			var id = new Guid();
			pushMessageDialogue.SetId(id);

			pushMessageDialogueRepository.Expect(x => x.Get(id))
				.Return(pushMessageDialogue)
				.Repeat.Once();

			var result = target.PersistMessage(new ConfirmMessageViewModel() { Id = id });
			Assert.That(result.MessageId, Is.EqualTo(id.ToString()));
		}

		[Test]
		public void ShouldConfirmIsRead()
		{
			var pushMessage = new PushMessage(new []{"OK"});
			var pushMessageDialogue = new PushMessageDialogue(pushMessage, new Person());
			var id = new Guid();
			pushMessageDialogue.SetId(id);

			var target = CreateTargetWithDialogueInRepository(pushMessageDialogue);
			var viewModel = target.PersistMessage(new ConfirmMessageViewModel() { Id = id });
         
			viewModel.IsRead.Should().Be.True();
		}

		[Test]
		public void ReplyingToMessageShouldAddThatReplyToTheConversationOnTheDialogue()
		{
			const string newMessage = "new message!!";
			var dialogueId = Guid.NewGuid();
			var pushMessage = new PushMessage();
			var pushMessageDialogue = new PushMessageDialogue(pushMessage, new Person());
			pushMessageDialogue.SetId(dialogueId);

			var target = CreateTargetWithDialogueInRepository(pushMessageDialogue);
			var viewModel = target.PersistMessage(new ConfirmMessageViewModel() { Id = dialogueId, Reply = newMessage });

			Assert.That(viewModel.DialogueMessages.First().Text,Is.EqualTo(newMessage));
		}

		[Test]
		public void ReplyingToMessageShouldSetLoggedOnUserAsSenderToThatReply()
		{			
			var sender = new Person();
			var dialogueId = Guid.NewGuid();
			var pushMessage = new PushMessage(new[] { "OK" }) { Sender = sender };
			var confirmMessage = new ConfirmMessageViewModel() { Id = dialogueId, Reply = "the reply" };
			var pushMessageDialogue = new PushMessageDialogue(pushMessage, sender);
			pushMessageDialogue.SetId(dialogueId);

			var target = CreateTargetWithDialogueInRepository(pushMessageDialogue);
			var result = target.PersistMessage(confirmMessage);
			
			Assert.That(result.DialogueMessages.First(m => m.Text.Equals("the reply")).SenderId, Is.EqualTo(_loggedOnUser.CurrentUser().Id));
		}

		private PushMessageDialoguePersister CreateTargetWithDialogueInRepository(IPushMessageDialogue dialogue)
		{
			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			pushMessageDialogueRepository.Stub(x => x.Get((Guid)dialogue.Id)).Return(dialogue);
			return CreateTarget(pushMessageDialogueRepository);
		}

		private PushMessageDialoguePersister CreateTarget(IPushMessageDialogueRepository repository)
		{
			var user = new Person();
			user.SetId(Guid.NewGuid());
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_loggedOnUser.Stub(l => l.CurrentUser()).Return(user);
			return new PushMessageDialoguePersister(repository, SetupMapper(), _loggedOnUser); 
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
