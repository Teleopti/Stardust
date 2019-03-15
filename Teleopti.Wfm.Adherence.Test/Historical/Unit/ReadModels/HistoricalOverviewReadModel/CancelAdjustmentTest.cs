using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ReadModels.HistoricalOverviewReadModel
{
	[DomainTest]
	public class CancelAdjustmentTest
	{
		public FakeHistoricalOverviewReadModelPersister ReadModels;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldSynchronizeCanceledAdjustmentForPreviousDay()
		{
			var person = Guid.NewGuid();
			History
				.ShiftStart(person, "2019-03-01 10:00", "2019-03-01 14:00")
				.StateChanged(person, "2019-03-01 10:00", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2019-03-01 11:00", Adherence.Configuration.Adherence.In)
				.AdjustedAdherenceToNeutral("2019-03-01 12:00", "2019-03-01 14:00")
				;

			History
				.CanceledAdjustment("2019-03-01 12:00", "2019-03-01 14:00");
			Now.Is("2019-03-08 08:00");

			var expected = ("2019-03-01 14:00".Utc() - "2019-03-01 11:00".Utc()).TotalSeconds;
			ReadModels.Read(person.AsArray())
				.Single().SecondsInAdherence
				.Should().Be(expected);
		}
	}
}