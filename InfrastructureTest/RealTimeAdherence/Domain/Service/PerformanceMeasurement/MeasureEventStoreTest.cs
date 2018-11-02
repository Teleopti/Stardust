using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using NHibernate.Loader;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Domain.AgentAdherenceDay;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.Domain.Service.PerformanceMeasurement
{
	[TestFixture]
	[Explicit]
	[Category("LongRunning")]
	[DatabaseTest]
	[Toggle(Toggles.RTA_ReviewHistoricalAdherence_Domain_74770)]
	[Toggle(Toggles.RTA_SpeedUpHistoricalAdherence_RemoveLastBefore_78306)]
	[Toggle(Toggles.RTA_SpeedUpHistoricalAdherence_EventStoreUpgrader_78485)]
	[Toggle(Toggles.RTA_SpeedUpHistoricalAdherence_RemoveScheduleDependency_78485)]
	[Toggle(Toggles.RTA_ReviewHistoricalAdherence_74770)]
	public class MeasureEventStoreTest
	{
		public ConcurrencyRunner Run;
		public IAgentAdherenceDayLoader Loader;
		public IEventPublisher Publisher;
		public Database Database;
		public WithUnitOfWork Uow;

		[Test]
		public void MeasureReading()
		{
			Database
				.WithAgent()
				.WithActivity("phone")
				.WithAssignment("2018-10-18")
				.WithAssignedActivity("phone", "2018-10-18 10:00", "2018-10-18 18:00");

			var personId = Database.CurrentPersonId();
			var done = false;
			var eventTime = "2018-10-18 10:00".Utc();
			var eventDate = "2018-10-18".Date();
			var personsIds = Enumerable.Range(0, 99).Select(x => Guid.NewGuid())
				.Concat(personId.AsArray())
				.ToArray();
			var eventSource = Enumerable.Range(0, 100).Select(i => new {number = i, personId = personsIds[i]});
			
			Run.InParallel(() =>
			{
				while (!done)
				{
					eventTime = eventTime.AddSeconds(1);
					var events = (
						from s in eventSource
						select new PersonStateChangedEvent
						{
							PersonId = s.personId,
							Timestamp = eventTime.AddMilliseconds(s.number),
							BelongsToDate = eventDate
						})
						.Randomize()
						.ToArray();
					Publisher.Publish(events);
				}
			});

			var stopwatch = new Stopwatch();
			stopwatch.Start();
			Run.InParallel(() =>
			{
				5000.Times(x =>
					Uow.Do(() => Loader.Load(personId, "2018-10-18".Date()))
				);
				done = true;
			});
			Run.Wait();
			stopwatch.Stop();

			Console.WriteLine($"Loaded in {stopwatch.Elapsed}");
		}
	}
}