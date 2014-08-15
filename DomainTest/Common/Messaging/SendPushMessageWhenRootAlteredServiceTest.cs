using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Common.Messaging
{
    [TestFixture]
    public class SendPushMessageWhenRootAlteredServiceTest
    {
        private SendPushMessageWhenRootAlteredService _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _target = new SendPushMessageWhenRootAlteredService();
            _mocks = new MockRepository();
        }

        [Test]
        public void VerifySelectsAllItemsThatImplementsPushMessageWhenRootAltered()
        {
            var normalRoot = _mocks.StrictMock<IAggregateRoot>();
            var rootThatShouldNotify = _mocks.StrictMultiMock<IAggregateRoot>(typeof(IPushMessageWhenRootAltered));
            var messgeWhenAlteredRoot = (IPushMessageWhenRootAltered)rootThatShouldNotify;
            var rootChangedInfo1 = _mocks.StrictMock<IRootChangeInfo>();
            var rootChangedInfo2 = _mocks.StrictMock<IRootChangeInfo>();
            var rootChangedInfo3 = _mocks.StrictMock<IRootChangeInfo>();
            IList<IRootChangeInfo> changedRoots = new List<IRootChangeInfo> { rootChangedInfo1, rootChangedInfo2, rootChangedInfo3 };

            using (_mocks.Record())
            {
                Expect.Call(rootChangedInfo1.Root).Return(normalRoot);
                Expect.Call(rootChangedInfo2.Root).Return(normalRoot);
                Expect.Call(rootChangedInfo3.Root).Return(rootThatShouldNotify);
            }

            using (_mocks.Playback())
            {
                _target.AddAlteredRoots(changedRoots);
                Assert.IsTrue(
                    _target.SendPushMessagesWhenRootAltered.Contains(messgeWhenAlteredRoot));
                Assert.IsTrue(_target.SendPushMessagesWhenRootAltered.Count == 1);
            }
        }

        [Test]
        public void VerifySendPushMessagesCombinesRootsFromMultiplePushMessages()
        {
            var rootThatShouldNotify = _mocks.StrictMultiMock<IAggregateRoot>(typeof(IPushMessageWhenRootAltered));
            var messgeWhenAlteredRoot = (IPushMessageWhenRootAltered)rootThatShouldNotify;
            var rootChangedInfo1 = _mocks.StrictMock<IRootChangeInfo>();
			var repository = _mocks.StrictMock<IPushMessagePersister>();
            var pushMessageService = _mocks.StrictMock<ISendPushMessageService>();
            var pushMessageReceipt = _mocks.StrictMock<ISendPushMessageReceipt>();

            IList<IRootChangeInfo> changedRoots = new List<IRootChangeInfo> { rootChangedInfo1 };

            using (_mocks.Record())
            {
                Expect.Call(rootChangedInfo1.Root).Return(rootThatShouldNotify);
                Expect.Call(messgeWhenAlteredRoot.ShouldSendPushMessageWhenAltered()).
                    Return(true);
                Expect.Call(messgeWhenAlteredRoot.PushMessageWhenAlteredInformation()).
                    Return(pushMessageService);
                Expect.Call(pushMessageService.SendConversationWithReceipt(repository)).Return(pushMessageReceipt);
                Expect.Call(pushMessageReceipt.AddedRoots()).Return(new[] {rootThatShouldNotify});
            }

            using (_mocks.Playback())
            {
                _target.SendPushMessagesWhenRootAltered.Add(messgeWhenAlteredRoot);
                IList<IAggregateRoot> addedRoots = _target.SendPushMessages(changedRoots, repository);
                addedRoots.Count.Should().Be.EqualTo(1);
            }
        }
    }
}
