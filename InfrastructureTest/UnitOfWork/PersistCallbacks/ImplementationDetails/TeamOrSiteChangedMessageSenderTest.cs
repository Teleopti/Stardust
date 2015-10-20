using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
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
	public class TeamOrSiteChangedMessageSenderTest
	{
		private IPersistCallback _target;
		private MockRepository _mocks;
		private IEventPopulatingPublisher _serviceBusSender;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_serviceBusSender = _mocks.DynamicMock<IEventPopulatingPublisher>();
			_target = new PersonCollectionChangedEventPublisherForTeamOrSite(_serviceBusSender, new SpecificBusinessUnit(BusinessUnitFactory.CreateWithId("fakeBu")));
		}

		[Test]
		public void ShouldSaveRebuildReadModelForTeamToQueue()
		{
			var team = new Team();
			var message = new PersonCollectionChangedEvent();

			var roots = new IRootChangeInfo[] {new RootChangeInfo(team, DomainUpdateType.Update)};

			using (_mocks.Record())
			{
				Expect.Call(() => _serviceBusSender.Publish(message))
				      .Constraints(new Rhino.Mocks.Constraints.PredicateConstraint<IEvent[]>(m => ((PersonCollectionChangedEvent)m.First()).SerializedPeople == Guid.Empty.ToString()));
			}
			using (_mocks.Playback())
			{
				_target.AfterFlush(roots);
			}
		}

		[Test]
		public void ShouldSaveRebuildReadModelForSiteToQueue()
		{
			var site = new Site("My Site");
			var message = new PersonCollectionChangedEvent();

			var roots = new IRootChangeInfo[] { new RootChangeInfo(site, DomainUpdateType.Update) };

			using (_mocks.Record())
			{
				Expect.Call(() => _serviceBusSender.Publish(message))
				      .Constraints(new Rhino.Mocks.Constraints.PredicateConstraint<IEvent[]>(m => ((PersonCollectionChangedEvent)m.First()).SerializedPeople == Guid.Empty.ToString()));
			}
			using (_mocks.Playback())
			{
				_target.AfterFlush(roots);
			}
		}
	}
}