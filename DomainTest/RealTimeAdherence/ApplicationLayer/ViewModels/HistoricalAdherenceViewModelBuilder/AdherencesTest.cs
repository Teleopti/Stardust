using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class AdherencesTest
	{
		public Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetHistoricalDataForAgent()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			Database
				.WithAdherenceOut(person, "2016-10-10 08:05")
				.WithAdherenceIn(person, "2016-10-10 08:15");

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-10T08:05:00");
			data.OutOfAdherences.Single().EndTime.Should().Be("2016-10-10T08:15:00");
		}
	}
}