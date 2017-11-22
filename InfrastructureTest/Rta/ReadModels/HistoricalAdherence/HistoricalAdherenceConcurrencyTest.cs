using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.HistoricalAdherence
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	[Explicit]
	[Category("LongRunning")]
	public class HistoricalAdherenceConcurrencyTest
	{
		public HistoricalAdherenceUpdater Updater;
		public HistoricalAdherenceMaintainer Maintainer;
		public MutableNow Now;

		[Test]
		public void ShouldNotDeadlockBetweenUpdaterAndMaintainer()
		{
			var run = new ConcurrencyRunner();
			var persons = Enumerable.Range(0, 1000)
				.Select(x => new Guid($"81a130d2-502f-4cf1-a376-" + x.ToString("000000000000")));
			var days = "2017-11-22".Date().DateRange(30);
			var runs =
				from d in days
				let times = Enumerable.Range(0, 60).Select(m => d.Date.AddMinutes(m * 10))
				let events = (
					from t in times
					from p in persons
					select new PersonInAdherenceEvent
					{
						PersonId = p,
						Timestamp = t
					}).ToArray()
				select new
				{
					startOfDay = d.Date,
					events
				};

			runs.ForEach(r =>
			{
				Now.Is(r.startOfDay);
				run.InParallel(() => { Updater.Handle(r.events); });
				run.InParallel(() => { Maintainer.Handle(new TenantDayTickEvent()); });
				run.Wait();
			});
		}
	}
}