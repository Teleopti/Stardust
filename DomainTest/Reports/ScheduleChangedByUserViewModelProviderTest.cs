using System;
using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Reports
{
	[DomainTest]
	public class ScheduleChangedByUserViewModelProviderTest
	{
		public PersonsWhoChangedSchedulesViewModelProvider Target;
		public FakeScheduleAuditTrailReport ScheduleAuditTrailReport;

		[Test]
		public void ShouldReturnModelOrderedByNameWithinGivenPeriod()
		{
			var personScott = PersonFactory.CreatePersonWithGuid("Scott", "scottson");
			var personAdam = PersonFactory.CreatePersonWithGuid("Adam", "Adamsson");
			var modifiedDate = new DateOnly(2019,02,22);
			ScheduleAuditTrailReport.AddModifiedByPerson(personScott, modifiedDate);
			ScheduleAuditTrailReport.AddModifiedByPerson(personAdam, modifiedDate.AddDays(-2));

			var result = Target.Provide(new DateOnlyPeriod(modifiedDate, modifiedDate));

			result.Count.Should().Be.EqualTo(1);
			result.Last().Id.Should().Be.EqualTo(personScott.Id);
			result.Last().Name.Should().Be.EqualTo(personScott.Name.ToString());
		}

	}
}
