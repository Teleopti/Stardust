using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class RecordedAdherencesTest
	{
		public Domain.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetRecordedHistoricalDataForAgent()
		{
			Now.Is("2018-01-10 15:00");
			var person = Guid.NewGuid();
			Database
				.WithAdherenceOut(person, "2018-01-10 08:05")
				.WithAdherenceIn(person, "2018-01-10 08:15");

			var data = Target.Build(person);

			data.RecordedOutOfAdherences.Single().StartTime.Should().Be("2018-01-10T08:05:00");
			data.RecordedOutOfAdherences.Single().EndTime.Should().Be("2018-01-10T08:15:00");
		}

		[Test]
		public void ShouldGetRecordedHistoricalDataForAgentAfterApproval()
		{
			Now.Is("2018-02-13 15:00");
			var person = Guid.NewGuid();
			Database
				.WithAdherenceOut(person, "2018-02-13 08:05")
				.WithAdherenceIn(person, "2018-02-13 08:15")
				.WithApprovedPeriod(person, "2018-02-13 08:05", "2018-02-13 08:15");

			var data = Target.Build(person);

			data.RecordedOutOfAdherences.Single().StartTime.Should().Be("2018-02-13T08:05:00");
			data.RecordedOutOfAdherences.Single().EndTime.Should().Be("2018-02-13T08:15:00");
		}
	}
}