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
	public class AdjustedToNeutralAdherencesTest
	{
		public IHistoricalAdherenceViewModelBuilder Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetAdjustedToNeutralAdherencesHistoricalDataForAgent()
		{
			Now.Is("2019-02-25 15:00");
			var person = Guid.NewGuid();
			History
				.AdjustedAdherenceToNeutral("2019-02-25 08:00", "2019-02-25 09:00");
			var data = Target.Build(person);

			data.AdjustedToNeutralAdherences.Single().StartTime.Should().Be("2019-02-25T08:00:00");
			data.AdjustedToNeutralAdherences.Single().EndTime.Should().Be("2019-02-25T09:00:00");
		}
		
		[Test]
		public void ShouldGetMultipleAdjustedToNeutralAdherencesHistoricalDataForAgent()
		{
			Now.Is("2019-02-25 15:00");
			var person = Guid.NewGuid();
			History
				.AdjustedAdherenceToNeutral("2019-02-25 08:00", "2019-02-25 09:00")
				.AdjustedAdherenceToNeutral("2019-02-25 09:00", "2019-02-25 10:00");
			var data = Target.Build(person);

			data.AdjustedToNeutralAdherences.First().StartTime.Should().Be("2019-02-25T08:00:00");
			data.AdjustedToNeutralAdherences.First().EndTime.Should().Be("2019-02-25T09:00:00");
			data.AdjustedToNeutralAdherences.Last().StartTime.Should().Be("2019-02-25T09:00:00");
			data.AdjustedToNeutralAdherences.Last().EndTime.Should().Be("2019-02-25T10:00:00");
		}
	}
}