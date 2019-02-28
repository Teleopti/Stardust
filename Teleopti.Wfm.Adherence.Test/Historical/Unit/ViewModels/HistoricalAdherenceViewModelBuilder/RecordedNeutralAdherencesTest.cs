using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	public class RecordedNeutralAdherencesTest
	{
		public IHistoricalAdherenceViewModelBuilder Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetRecordedNeutralAdherenceForAgent()
		{
			Now.Is("2019-02-27 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-27 08:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-27 09:00", Adherence.Configuration.Adherence.In);
			var data = Target.Build(person);

			data.RecordedNeutralAdherences.Single().StartTime.Should().Be("2019-02-27T08:00:00");
			data.RecordedNeutralAdherences.Single().EndTime.Should().Be("2019-02-27T09:00:00");
		}
		
		[Test]
		public void ShouldGetMultipleRecordedNeutralAdherencesForAgent()
		{
			Now.Is("2019-02-27 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-27 08:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-27 09:00", Adherence.Configuration.Adherence.In)
				.StateChanged(person, "2019-02-27 10:00", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-27 11:00", Adherence.Configuration.Adherence.In);
			var data = Target.Build(person);

			data.RecordedNeutralAdherences.First().StartTime.Should().Be("2019-02-27T08:00:00");
			data.RecordedNeutralAdherences.First().EndTime.Should().Be("2019-02-27T09:00:00");
			data.RecordedNeutralAdherences.Last().StartTime.Should().Be("2019-02-27T10:00:00");
			data.RecordedNeutralAdherences.Last().EndTime.Should().Be("2019-02-27T11:00:00");
		}
	}
}