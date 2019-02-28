﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState.Reader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class EmptyCollectionsTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;
		public ICurrentBusinessUnit CurrentBusinessUnit;

		[Test]
		public void ShouldLoadAllWhenEmptyCollections()
		{
			var personId = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = CurrentBusinessUnit.CurrentId(),
				PersonId = personId
			});

			var result = Target.Read(new AgentStateFilter
			{
				TeamIds = Enumerable.Empty<Guid>(),
				SiteIds = Enumerable.Empty<Guid>(),
				SkillIds = Enumerable.Empty<Guid>(),
				ExcludedStateIds = Enumerable.Empty<Guid?>()
			});

			result.Single().PersonId.Should().Be(personId);
		}
	}
}