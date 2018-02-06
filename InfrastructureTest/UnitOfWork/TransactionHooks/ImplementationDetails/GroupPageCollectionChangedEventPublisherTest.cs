using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture]
	public class GroupPageCollectionChangedEventPublisherTest
	{
		private ITransactionHook _target;
		private MockRepository _mocks;
		private IEventPopulatingPublisher _eventPopulatingPublisher;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_eventPopulatingPublisher = _mocks.DynamicMock<IEventPopulatingPublisher>();

			_target = new GroupPageCollectionChangedEventPublisher(_eventPopulatingPublisher);
		}

        [Test]
        public void ShouldSaveRebuildReadModelForGroupPageToQueue()
        {
            var page = new GroupPage("Page");
            var ids = new Guid[0];
            var message = new GroupPageCollectionChangedEvent();
            message.SetGroupPageIdCollection(ids);

            var roots = new IRootChangeInfo[1];
            roots[0] = new RootChangeInfo(page, DomainUpdateType.Update);

            using (_mocks.Record())
            {
                Expect.Call(() => _eventPopulatingPublisher.Publish(message)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
				_target.AfterCompletion(roots);
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
				_target.AfterCompletion(new IRootChangeInfo[] { new RootChangeInfo(scenario, DomainUpdateType.Insert) });
			}
		}
	}
}