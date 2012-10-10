using System;
using System.Collections.Concurrent;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Message;
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
			pushMessageDialogue.SetId(Guid.NewGuid());
			var mappedViewModel = new MessageViewModel { MessageId = pushMessageDialogue.Id.ToString(), IsRead = true};

			pushMessageDialogueRepository.Stub(x => x.Get(pushMessageDialogue.Id.Value)).Return(pushMessageDialogue);
			mapper.Stub(x => x.Map<IPushMessageDialogue, MessageViewModel>(pushMessageDialogue)).Return(mappedViewModel);

			var viewModel = target.Persist(pushMessageDialogue.Id.ToString());

			viewModel.MessageId.Should().Be.EqualTo(mappedViewModel.MessageId);
            viewModel.IsRead.Should().Be.True();
		}
	}
}
