﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta.ImplementationDetailsTests.Adherence
{
	public class AdherenceAggregatorInitializorTest
	{
		[Test]
		public void ShouldCallAggregateOnInnerBasedOnCurrentDatabaseState()
		{
			var aggregator = MockRepository.GenerateMock<IAdherenceAggregator>();
			var person1 = new PersonOrganizationData {PersonId =  Guid.NewGuid()};
			var person2 = new PersonOrganizationData {PersonId =  Guid.NewGuid()};
			var state1 = new ActualAgentState();
			var state2 = new ActualAgentState();
			var loadActualAgentState = MockRepository.GenerateMock<IGetCurrentActualAgentState>();
			loadActualAgentState.Stub(x => x.GetCurrentActualAgentState(person1.PersonId)).Return(state1);
			loadActualAgentState.Stub(x => x.GetCurrentActualAgentState(person2.PersonId)).Return(state2);
			var target = new AdherenceAggregatorInitializor(aggregator, new FakePersonOrganizationProvider(new[] {person1, person2}), loadActualAgentState);

			target.Initialize();

			aggregator.AssertWasCalled(i => i.Invoke(state1));
			aggregator.AssertWasCalled(i => i.Invoke(state2));
		}

		[Test]
		public void ShouldNotCallAggregateOnInnerIfStateIsNull()
		{
			var aggregator = MockRepository.GenerateMock<IAdherenceAggregator>();
			var loadActualAgentState = MockRepository.GenerateMock<IGetCurrentActualAgentState>();
			var personOrganizationData = new PersonOrganizationData {PersonId = Guid.NewGuid()};
			var target = new AdherenceAggregatorInitializor(aggregator,
			                                                new FakePersonOrganizationProvider(new[]
				                                                {personOrganizationData}),
			                                                loadActualAgentState);
			loadActualAgentState.Stub(x => x.GetCurrentActualAgentState(personOrganizationData.PersonId)).Return(null);
			target.Initialize();

			aggregator.AssertWasNotCalled(i => i.Invoke(null));
		}
	}

	public class FakePersonOrganizationProvider : IPersonOrganizationProvider
	{
		private readonly PersonOrganizationData[] _result;

		public FakePersonOrganizationProvider(PersonOrganizationData[] result)
		{
			_result = result;
		}

		public IDictionary<Guid, PersonOrganizationData> PersonOrganizationData()
		{
			return _result.ToDictionary(data => data.PersonId);
		}
	}
}