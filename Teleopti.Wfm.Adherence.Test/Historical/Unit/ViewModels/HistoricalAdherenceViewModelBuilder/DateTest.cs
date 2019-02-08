using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	public class DateTest
	{
		public Adherence.Historical.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldRenderForGivenDate()
		{
			Now.Is("2017-12-13 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2017-12-01")
				.WithAssignedActivity("2017-12-01 08:00", "2017-12-01 16:00")
				.WithAssignment(person, "2017-12-13")
				.WithAssignedActivity("2017-12-13 08:00", "2017-12-13 16:00")
				;

			var data = Target.Build(person, "2017-12-01".Date());

			data.Schedules.Single().StartTime.Should().Be.EqualTo("2017-12-01T08:00:00");
		}
	}
}