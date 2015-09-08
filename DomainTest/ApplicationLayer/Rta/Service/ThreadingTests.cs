using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class ThreadingTests
	{
		public MutableNow Now;
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public IAgentStateReadModelReader AgentStateReadModelReader;

		[Test]
		public async void ShouldNotSendDuplicateEvents()
		{
			Database
				.WithDefaultStateGroup()
				.WithStateCode("AUX1");
			var tasks = new List<Task>();
			100.Times(i =>
			{
				var personId = Guid.NewGuid();
				Database
					.WithUser(i.ToString(), personId)
					.WithSchedule(personId, Guid.NewGuid(), "2015-05-19 08:00", "2015-05-19 09:00");
			});
			Now.Is("2015-05-19 08:00");

			100.Times(i =>
			{
				tasks.Add(Task.Factory.StartNew(() =>
					Target.SaveState(new ExternalUserStateForTest
					{
						UserCode = i.ToString(),
						StateCode = "AUX1"
					})));
				tasks.Add(Task.Factory.StartNew(() =>
					Target.SaveState(new ExternalUserStateForTest
					{
						UserCode = i.ToString(),
						StateCode = "AUX1"
					})));
				tasks.Add(Task.Factory.StartNew(() =>
					Target.SaveState(new ExternalUserStateForTest
					{
						UserCode = i.ToString(),
						StateCode = "AUX1"
					})));
			});

			await Task.WhenAll(tasks.ToArray());

			AgentStateReadModelReader.GetActualAgentStates().Should().Have.Count.EqualTo(100);
			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Should().Have.Count.EqualTo(100);
		}
	}
}