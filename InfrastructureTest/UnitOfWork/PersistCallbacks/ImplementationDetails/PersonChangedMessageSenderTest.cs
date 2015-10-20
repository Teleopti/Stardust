using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks.ImplementationDetails
{
	[TestFixture]
	public class PersonChangedMessageSenderTest
	{
		private IPersistCallback _target;
		private MockRepository _mocks;
		private IEventPopulatingPublisher _serviceBusSender;
		
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_serviceBusSender = _mocks.DynamicMock<IEventPopulatingPublisher>();
			_target = new PersonCollectionChangedEventPublisher(_serviceBusSender, new SpecificBusinessUnit(BusinessUnitFactory.CreateWithId("fakeBu")));
		}

        [Test]
        public void ShouldSaveRebuildReadModelForPersonToQueue()
        {
            var person = new Person();
            var ids = new Guid[] {};
            var message = new PersonCollectionChangedEvent();
            message.SetPersonIdCollection(ids);
            
            var roots = new IRootChangeInfo[1];
            roots[0] = new RootChangeInfo(person, DomainUpdateType.Update);

            using (_mocks.Record())
            {
                Expect.Call(() => _serviceBusSender.Publish(message)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
				_target.AfterFlush(roots);
            }
        }

		[Test]
		public void ShouldNotSaveRebuildReadModelForPersonWriteProtectionToQueue()
		{
			var personWriteProtectionInfo = new PersonWriteProtectionInfo(new Person());
			var ids = new Guid[] { };
			var message = new PersonCollectionChangedEvent();
			message.SetPersonIdCollection(ids);

			var roots = new IRootChangeInfo[1];
			roots[0] = new RootChangeInfo(personWriteProtectionInfo, DomainUpdateType.Update);

			using (_mocks.Record())
			{
				Expect.Call(() => _serviceBusSender.Publish(message)).Repeat.Never();
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
