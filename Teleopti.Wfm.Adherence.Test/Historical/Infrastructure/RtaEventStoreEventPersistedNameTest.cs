using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States.Events;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Historical.Infrastructure
{
	[TestFixture]
	[DatabaseTest]
	public class RtaEventStoreEventPersistedNameTest
	{
		public IEventPublisher Publisher;
		public IRtaEventStoreTester Events;
		public WithUnitOfWork WithUnitOfWork;
		public PersistedTypeMapper TypeMapper;

		[Test]
		public void ShouldStoreWithEventPersistedName()
		{
			var @event = new PersonStateChangedEvent();
			var eventType = @event.GetType();
			Publisher.Publish(@event);

			var actual = WithUnitOfWork.Get(uow => Events.LoadAllEventTypes());

			actual.Single().Should().Be(TypeMapper.NameForPersistence(eventType));
		}
	}
}