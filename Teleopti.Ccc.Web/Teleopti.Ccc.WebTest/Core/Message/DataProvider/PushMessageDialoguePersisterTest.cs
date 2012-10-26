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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Message.DataProvider
{
	[TestFixture]
	public class PushMessageDialoguePersisterTest
	{
		[Test]
		public void ShouldPersistMessage()
		{
			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new PushMessageDialoguePersister(pushMessageDialogueRepository, mapper);

			var pushMessage = new PushMessage(new []{"OK"});

			var pushMessageDialogue = new PushMessageDialogue(pushMessage, new Person());
			var id = new Guid();
			pushMessageDialogue.SetId(id);
			var mappedViewModel = new MessageViewModel { MessageId = pushMessageDialogue.Id.ToString(), IsRead = true};

			pushMessageDialogueRepository.Stub(x => x.Get(id)).Return(pushMessageDialogue);
			mapper.Stub(x => x.Map<IPushMessageDialogue, MessageViewModel>(pushMessageDialogue)).Return(mappedViewModel);

			var viewModel = target.PersistMessage(new ConfirmMessageViewModel() { Id = id });

			viewModel.MessageId.Should().Be.EqualTo(mappedViewModel.MessageId);
            viewModel.IsRead.Should().Be.True();
		}

		[Test]
		public void CanAddDialogueMessage()
		{			
			var sender = new Person();
			var dialogueId = Guid.NewGuid();
			var pushMessage = new PushMessage(new[] { "OK" }) { Sender = sender };
			var pushMessageDialogue = new PushMessageDialogue(pushMessage, sender);
			pushMessageDialogue.SetId(dialogueId);
			var pushMessageDialogueRepository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			pushMessageDialogueRepository.Stub(x => x.Get(dialogueId)).Return(pushMessageDialogue);
			var target = new PushMessageDialoguePersister(pushMessageDialogueRepository, SetupMapper());
			string newMessage="new message!!";
			var viewModel = target.PersistMessage(new ConfirmMessageViewModel() { Id = dialogueId, Reply = newMessage });

			Assert.That(viewModel.DialogueMessages.First().Text,Is.EqualTo(newMessage));
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
