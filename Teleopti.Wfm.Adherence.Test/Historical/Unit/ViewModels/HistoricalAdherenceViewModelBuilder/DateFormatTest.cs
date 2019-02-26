using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Historical;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	public class DateFormatTest
	{
		public IHistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		[SetCulture("fi-FI")]
		public void ShouldFormat()
		{
			Now.Is("2018-12-12 12:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person)
				.WithAssignment(person, "2018-12-12")
				.WithAssignedActivity("2018-12-12 08:00", "2018-12-12 16:00")
				;

			var data = Target.Build(person, "2018-12-12".Date());

			data.Schedules.Single().StartTime.Should().Be.EqualTo("2018-12-12T08:00:00");
		}
	}
}