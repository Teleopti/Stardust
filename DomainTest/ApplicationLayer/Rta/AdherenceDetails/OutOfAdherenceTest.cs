using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.AdherenceDetails
{
	[AdherenceTest]
	[TestFixture]
	public class OutOfAdherenceTest : IRegisterInContainer
	{
		public FakeAdherenceDetailsReadModelPersister Persister;
		public AdherenceDetailsReadModelUpdater Target;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterType<AdherenceDetailsReadModelUpdater>().AsSelf();
		}

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenStateChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 9:00".Utc(),
				InAdherence = true
			});

			Persister.Details.Single().TimeOutOfAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutAdherenceWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				InAdherence = false
			});

			Persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromHours(1));
		}

		[Test]
		public void ShouldPersistTimeOutOfAdherenceWhenInAdherenceBeforeActivityChanges()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:00".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 9:00".Utc(),
				InAdherence = false
			});

			Persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(5));
		}

		[Test]
		public void ShouldSummarizeAllOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			Target.Handle(new PersonActivityStartEvent
			{
				PersonId = personId,
				StartTime = "2014-11-17 8:00".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:10".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:20".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:25".Utc(),
				InAdherence = true
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:45".Utc(),
				InAdherence = false
			});
			Target.Handle(new PersonStateChangedEvent
			{
				PersonId = personId,
				Timestamp = "2014-11-17 8:55".Utc(),
				InAdherence = false
			});

			Persister.Details.First().TimeOutOfAdherence.Should().Be(TimeSpan.FromMinutes(10 + 5 + 10));
		}
	}
}