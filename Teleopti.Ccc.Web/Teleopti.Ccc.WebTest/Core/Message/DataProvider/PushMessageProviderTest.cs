using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.Message.DataProvider
{
	[TestFixture]
	public class PushMessageProviderTest
	{
		private PushMessageProvider _target;
		private IPushMessageDialogueRepository _repository;
		private ILoggedOnUser _loggedOnUser;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_repository = MockRepository.GenerateMock<IPushMessageDialogueRepository>();
			_loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			_person = new Person();
			_loggedOnUser.Stub(x => x.CurrentUser()).Return(_person);
		}

		[Test]
		public void ShouldGetUnreadMessageCountForUser()
		{
			_repository.Stub(x => x.CountUnread(_person)).Return(2);

			_target = new PushMessageProvider(_loggedOnUser, _repository);

			_target.UnreadMessageCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldGetUnreadMessagesForUser()
		{
			var paging = new Paging();
			
			_repository.Stub(x => x.FindUnreadMessages(paging, _person)).Return(new Collection<IPushMessageDialogue>());

			_target = new PushMessageProvider(_loggedOnUser, _repository);

			_target.GetMessages(paging).Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetAMessage()
		{
			var pushMessageDialogue = MockRepository.GenerateMock<IPushMessageDialogue>();
			_repository.Stub(x => x.Get(Guid.Empty)).Return(pushMessageDialogue);

			_target = new PushMessageProvider(_loggedOnUser, _repository);

			var result = _target.GetMessage(Guid.Empty);

			result.Should().Be.SameInstanceAs(pushMessageDialogue);
		}
	}
}
