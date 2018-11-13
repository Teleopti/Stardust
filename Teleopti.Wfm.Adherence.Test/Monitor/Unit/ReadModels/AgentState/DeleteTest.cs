﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;

namespace Teleopti.Wfm.Adherence.Test.ApplicationLayer.ReadModels.AgentState
{
	[TestFixture]
	[DomainTest]
	public class DeleteTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldUpsertNoAssociationWhenDeleting()
		{
			var personId = Guid.NewGuid();

			Target.Handle(new PersonAssociationChangedEvent { PersonId = personId });

			Persister.Models.Single().PersonId.Should().Be(personId);
		}

	}
}