using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks.ImplementationDetails
{
	[TestFixture]
	public class GroupPageChangedMessageSenderTest
	{
		private IPersistCallback _target;
		private MockRepository _mocks;
		private IMessagePopulatingServiceBusSender _serviceBusSender;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_serviceBusSender = _mocks.DynamicMock<IMessagePopulatingServiceBusSender>();

			_target = new GroupPageChangedBusMessageSender(_serviceBusSender);
		}

        [Test]
        public void ShouldSaveRebuildReadModelForGroupPageToQueue()
        {
            var page = new GroupPage("Page");
            var ids = new Guid[] { };
            var message = new GroupPageChangedMessage();
            message.SetGroupPageIdCollection(ids);

            var roots = new IRootChangeInfo[1];
            roots[0] = new RootChangeInfo(page, DomainUpdateType.Update);

            using (_mocks.Record())
            {
                Expect.Call(() => _serviceBusSender.Send(message, false)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
				_target.AfterFlush(roots);
            }
        }


		[Test]
		public void ShouldNotRebuildReadModelForScenario()
		{
			var scenario = _mocks.DynamicMock<IScenario>();

			using (_mocks.Record())
			{
			}
			using (_mocks.Playback())
			{
				_target.AfterFlush(new IRootChangeInfo[] { new RootChangeInfo(scenario, DomainUpdateType.Insert) });
			}
		}
	}
}