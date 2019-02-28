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
	public class AdherencesTest
	{
		public IHistoricalAdherenceViewModelBuilder Target;
		public FakeRtaHistory History;
		public MutableNow Now;

		[Test]
		public void ShouldGetHistoricalOutOfAdherencesDataForAgent()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2016-10-10 08:05", Adherence.Configuration.Adherence.Out)
				.StateChanged(person, "2016-10-10 08:15", Adherence.Configuration.Adherence.In);

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-10T08:05:00");
			data.OutOfAdherences.Single().EndTime.Should().Be("2016-10-10T08:15:00");
		}
		
		[Test]
		public void ShouldGetHistoricalNeutralAdherencesDataForAgent()
		{
			Now.Is("2019-02-28 15:00");
			var person = Guid.NewGuid();
			History
				.StateChanged(person, "2019-02-28 08:05", Adherence.Configuration.Adherence.Neutral)
				.StateChanged(person, "2019-02-28 08:15", Adherence.Configuration.Adherence.In);

			var data = Target.Build(person);

			data.NeutralAdherences.Single().StartTime.Should().Be("2019-02-28T08:05:00");
			data.NeutralAdherences.Single().EndTime.Should().Be("2019-02-28T08:15:00");
		}
	}
}