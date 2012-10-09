using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Message.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Message.DataProvider
{
	[TestFixture]
	public class PushMessageProviderTest
	{
		private PushMessageProvider _target;

		[Test]
		public void ShouldGetUnreadMessageCountForUser()
		{
			var repository = MockRepository.GenerateMock<IPushMessageRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			IPerson person = new Person();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			repository.Stub(x => x.CountUnread(person)).Return(2);

			_target = new PushMessageProvider(loggedOnUser, repository);

			_target.UnreadMessageCount.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldGetUnreadMessagesForUser()
		{
			var paging = new Paging();
			var repository = MockRepository.GenerateMock<IPushMessageRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			IPerson person = new Person();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			repository.Stub(x => x.FindUnreadMessage(paging, person)).Return(new Collection<IPushMessageDialogue>());

			_target = new PushMessageProvider(loggedOnUser, repository);

			_target.GetMessages(paging).Count.Should().Be.EqualTo(0);
		}
	}
}
