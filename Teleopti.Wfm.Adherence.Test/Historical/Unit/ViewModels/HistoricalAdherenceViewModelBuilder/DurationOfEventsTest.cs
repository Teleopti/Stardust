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
	public class DurationOfEventsTest
	{
		public IHistoricalAdherenceViewModelBuilder Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetDuration()
		{
			Now.Is("2018-07-09 17:00");
			var personId = Guid.NewGuid();
			History
				.StateChanged(personId, "2018-07-09 10:00")
				.StateChanged(personId, "2018-07-09 11:00")
				;

			var results = Target.Build(personId).Changes;

			results.First().Duration.Should().Be("01:00:00");
			results.Last().Duration.Should().Be(null);
		}
	}
}