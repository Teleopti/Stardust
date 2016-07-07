﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.AgentState
{
	[TestFixture]
	[ReadModelUpdaterTest]
	public class DeletedPersonTest
	{
		public AgentStateReadModelCleaner Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldRemoveReadModelWhenPersonIsDeleted()
		{
			var personId = Guid.NewGuid();
			Persister.Persist(new AgentStateReadModel {PersonId = personId});

			Target.Handle(new PersonDeletedEvent {PersonId = personId});

			Persister.Models.Should().Be.Empty();
		}
	}
}