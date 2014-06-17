using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class ThreadingIssuesTest
	{
		[Test, Explicit("not yet fixed")]
		public void LotsOfTeamMessagesShouldNotThrow()
		{
			const int numberOfMessages = 500;
			var teamId = Guid.NewGuid();
			var broker = new MessageSenderExposingNotifications();
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();
			organizationForPerson.Stub(x => x.GetOrganization(Guid.Empty)).IgnoreArguments()
				.Return(new PersonOrganizationData { TeamId = teamId });
			var target = new AdherenceAggregator(broker, organizationForPerson);

			//gegga för nu
			var personGuids = new List<Guid>();
			for (var loop = 0; loop < numberOfMessages; loop++)
			{
				personGuids.Add(Guid.NewGuid());
			}

			thisShouldGoAwayWhenWeRefactor(target, personGuids, organizationForPerson);

			/////

			var taskar = new List<Task>();
			for (var loop = 0; loop < numberOfMessages; loop++)
			{
				var loopVarde = loop;
				taskar.Add(Task.Factory.StartNew(() =>
				{
					var agentId = personGuids[loopVarde];
					target.Invoke(new ActualAgentState { StaffingEffect = 0, PersonId = agentId });
					target.Invoke(new ActualAgentState { StaffingEffect = 1, PersonId = agentId });
				}));
			}
			Task.WaitAll(taskar.ToArray());

			broker.AllNotifications.Select(x => x.GetOriginal<TeamAdherenceMessage>().OutOfAdherence)
				.OrderByDescending(x => x).First().Should().Be.EqualTo(numberOfMessages);
		}

		private static void thisShouldGoAwayWhenWeRefactor(IActualAgentStateHasBeenSent target, IEnumerable<Guid> personGuids, IOrganizationForPerson organizationForPerson)
		{
			var actualAgentStateDefault = new ActualAgentState();
			var personOrgDataColl = new List<PersonOrganizationData>();
			personGuids.ForEach(id =>
			{
				var personOrgData = organizationForPerson.GetOrganization(id);
				var personData = new PersonOrganizationData
				{
					PersonId = id,
					TeamId = personOrgData.TeamId,
					SiteId = personOrgData.SiteId
				};
				personOrgDataColl.Add(personData);
			});

			var loadAgentState = MockRepository.GenerateStub<ILoadActualAgentState>();
			loadAgentState.Stub(x => x.LoadOldState(Guid.Empty)).IgnoreArguments().Return(actualAgentStateDefault);
			var personOrgProvider = MockRepository.GenerateStub<IPersonOrganizationProvider>();
			personOrgProvider.Stub(x => x.LoadAll()).Return(personOrgDataColl);
			var init = new AdherenceAggregatorInitializor(target, personOrgProvider, loadAgentState);
			init.Initialize();
		}
	}
}