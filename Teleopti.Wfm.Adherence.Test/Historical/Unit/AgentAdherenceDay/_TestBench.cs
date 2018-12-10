using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Historical;
using Teleopti.Wfm.Adherence.Historical.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Historical.Infrastructure;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.AgentAdherenceDay
{
	[DomainTest]
	[Ignore("Useful for testing with customer data")]
	public class TestBench
	{
		public FakeRtaEventStore Store;
		public IAgentAdherenceDayLoader Loader;
		public IJsonEventDeserializer Deserializer;
		public RtaEventStoreTypeIdMapper TypeMapper;

		[Test]
		public void ShouldWork()
		{
			var events = readFile();
			Store.Add(events, DeadLockVictim.No, RtaEventStoreVersion.StoreVersion);
			var personId = (events.First() as dynamic).PersonId;
			var date = events.OfType<PersonShiftStartEvent>().First().BelongsToDate.Value;

			Loader.Load(personId, date);
		}

		private IEnumerable<IEvent> readFile() =>
		(
			from l in fileContents().Split('\n')
			let columns = l.Split('\t')
			let typeId = columns[0]
			let json = columns[1]
			let eventType = TypeMapper.TypeForTypeId(typeId)
			let @event = Deserializer.DeserializeEvent(json, eventType) as IEvent
			select @event
		).ToArray();

		private static string fileContents()
		{
			var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Historical\\Unit\\AgentAdherenceDay\\_TestBench.data");
			return File.ReadAllText(path);
		}
	}
}