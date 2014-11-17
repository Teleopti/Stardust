using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture, Ignore]
	public class AdherenceDetailsReadModelUpdaterTest
	{
		[Test]
		public void ShouldPersist()
		{
			var persister = new FakeAdherenceDetailsReadModelPersister();
			var target = new AdherenceDetailsReadModelUpdater(persister);

			target.Handle(new PersonActivityStartEvent
			{
				PersonId = Guid.NewGuid(),
				Name = "Phone",
				StartTime = "2014-11-17 8:00".Utc()
			});

			persister.Persisted.Single().Name.Should().Be("Phone");
		}
	}

	public class AdherenceDetailsReadModelUpdater : 
		IHandleEvent<PersonActivityStartEvent>,
		IHandleEvent<PersonStateChangedEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonOutOfAdherenceEvent>
	{
		public AdherenceDetailsReadModelUpdater(IAdherenceDetailsReadModelPersister persister)
		{
		}

		public void Handle(PersonActivityStartEvent @event)
		{
		}

		public void Handle(PersonStateChangedEvent @event)
		{
		}

		public void Handle(PersonInAdherenceEvent @event)
		{
		}

		public void Handle(PersonOutOfAdherenceEvent @event)
		{
		}
	}

	public class FakeAdherenceDetailsReadModelPersister : IAdherenceDetailsReadModelPersister
	{
		public IEnumerable<AdherenceDetailsReadModel> Persisted { get; set; }
	}

	public class AdherenceDetailsReadModel
	{
		public string Name { get; set; }
	}

	public interface IAdherenceDetailsReadModelPersister
	{
	}
}