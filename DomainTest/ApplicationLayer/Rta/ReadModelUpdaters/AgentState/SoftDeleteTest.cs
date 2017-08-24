using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Events.ExternalLogon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[DomainTest]
	public class SoftDeleteTest
	{
		public AgentStateReadModelMaintainer Target;
		public MutableNow Now;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldSetDeletedWhenPersonIsDeleted()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonDeletedEvent { PersonId = personId });

			Persister.Models.Single().IsDeleted.Should().Be(true);
		}

		[Test]
		public void ShouldInsertDeleted()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonDeletedEvent { PersonId = personId });

			Persister.Models.Single().IsDeleted.Should().Be(true);
		}
		
		[Test]
		public void ShouldSetDeletedIfPersonIsTerminated()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Models.Single().IsDeleted.Should().Be(true);
		}

		[Test]
		public void ShouldInsertDeletedIfPersonIsTerminated()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null
			});

			Persister.Models.Single().IsDeleted.Should().Be(true);
		}

		[Test]
		public void ShouldSetDeletedIfPersonIsNotConnectedToExternalLogon()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				ExternalLogons = null
			});

			Persister.Models.Single().IsDeleted.Should().Be(true);
		}
		
		[Test]
		public void ShouldInsertDeletedIfPersonIsNotConnectedToExternalLogon()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				ExternalLogons = null
			});

			Persister.Models.Single().IsDeleted.Should().Be(true);
		}

		[Test]
		public void ShouldKeepReadModelIfPersonIsInATeam()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = personId });

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				ExternalLogons = new[] { new ExternalLogon() }
			});

			Persister.Models.Single().PersonId.Should().Be(personId);
			Persister.Models.Single().IsDeleted.Should().Be(false);
		}

		[Test]
		public void ShouldUnDeleteReadModel()
		{
			var personId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel {PersonId = personId});
			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = null,
				Timestamp = "2016-10-04 08:00".Utc()
			});

			Target.Handle(new PersonAssociationChangedEvent
			{
				PersonId = personId,
				TeamId = Guid.NewGuid(),
				Timestamp = "2016-10-04 08:10".Utc(),
				ExternalLogons = new[] {new ExternalLogon()}
			});

			Persister.Load(personId).IsDeleted.Should().Be.False();
		}
	}
}