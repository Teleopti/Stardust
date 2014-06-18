using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;
using IMessageSender = Teleopti.Interfaces.MessageBroker.Client.IMessageSender;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	/// If this/these tests "randomly" fails, it _is_ a bug.
	/// To recreate, set a higher "numberOfIterations" to a higher value
	public class ThreadingIssuesTest
	{
		private const int numberOfIterations = 100;

		[Test]
		public void LotsOfTeamMessagesShouldNotThrow()
		{
			var teamId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var broker = new assertBrokerMessageSender(numberOfIterations);
			var organizationForPerson = MockRepository.GenerateMock<IOrganizationForPerson>();

			var target = new AdherenceAggregator(broker, organizationForPerson);


			var personGuids = new List<Guid>();
			for (var loop = 0; loop < numberOfIterations; loop++)
			{
				var personId = Guid.NewGuid();

				personGuids.Add(personId);
				organizationForPerson.Stub(x => x.GetOrganization(personId)).Return(new PersonOrganizationData
				{
					TeamId = teamId, 
					PersonId = personId,
					SiteId = siteId
				});
			}

			var taskar = new List<Task>();
			for (var loop = 0; loop < numberOfIterations; loop++)
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

			broker.HasMatchedAdherence.Should().Be.True();
		}

		private class assertBrokerMessageSender : IMessageSender
		{
			private readonly int _outofAdherence;

			public assertBrokerMessageSender(int outofAdherence)
			{
				_outofAdherence = outofAdherence;
			}

			public bool IsAlive { get; private set; }

			public bool HasMatchedAdherence { get; private set; }

			public void StartBrokerService(bool useLongPolling = false)
			{
			}

			public void SendNotification(Notification notification)
			{
				var outOfAdherenceValue = notification.GetOriginal<TeamAdherenceMessage>().OutOfAdherence;
				if (outOfAdherenceValue > _outofAdherence)
					throw new Exception("Too high outofadherence");
				if (Math.Abs(outOfAdherenceValue - _outofAdherence) < 0.1)
					HasMatchedAdherence = true;

			}
		}
	}
}