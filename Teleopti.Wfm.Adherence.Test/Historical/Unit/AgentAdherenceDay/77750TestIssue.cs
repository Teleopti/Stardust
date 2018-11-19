using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	[Ignore("WIP")]
	public class _77750TestIssue
	{
		public FakeRtaEventStore Store;
		public IAgentAdherenceDayLoader Loader;
		public IJsonEventDeserializer Deserializer;

		[Test]
		public void Should()
		{
			var events = readFile();
			Store.Add(events, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			var personId = Guid.Parse("164abe5d-ce1a-48ee-ba3a-9b5e015b2585");
			var date = "2018-11-09".Date();

			Loader.Load(personId, date);
		}

		private IEnumerable<IEvent> readFile()
		{
			var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Historical\\Unit\\AgentAdherenceDay\\77750TestIssue.txt");
			var file = File.ReadAllText(path);
			var lines = file.Split('\n');
			return
			(
				from l in lines
				let columns = l.Split('\t')
				let typeId = columns[0]
				let json = columns[1]
				let eventType = buildTypeForId()[typeId]
				let @event = Deserializer.DeserializeEvent(json, eventType) as IEvent
				select @event
			).ToArray();
		}

		private static string eventTypeId(Type type) => type.GetCustomAttribute<JsonObjectAttribute>().Id;

		private static Dictionary<string, Type> buildTypeForId()
		{
			var example = typeof(PersonStateChangedEvent);
			return example.Assembly
				.GetTypes()
				.Where(x => x.Namespace == example.Namespace)
				.Where(x => x.IsClass)
				.Where(x => typeof(IRtaStoredEvent).IsAssignableFrom(x))
				.ToDictionary(eventTypeId, x => x);
		}
	}
}