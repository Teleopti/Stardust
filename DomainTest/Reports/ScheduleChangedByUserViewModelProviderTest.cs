using NUnit.Framework;
using SharpTestsEx;
using System.Linq;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Reports
{
	[DomainTest]
	public class ScheduleChangedByUserViewModelProviderTest
	{
		public ScheduleChangedByUserViewModelProvider Target;
		public FakeScheduleAuditTrailReport ScheduleAuditTrailReport;

		[Test]
		public void ShouldReturnModelOrderedByName()
		{
			var personScott = PersonFactory.CreatePersonWithGuid("Scott", "scottson");
			var personAdam = PersonFactory.CreatePersonWithGuid("Adam", "Adamsson");
			ScheduleAuditTrailReport.AddModifiedByPerson(personScott);
			ScheduleAuditTrailReport.AddModifiedByPerson(personAdam);

			var result = Target.Provide();

			result.Count.Should().Be.EqualTo(2);
			result.First().Id.Should().Be.EqualTo(personAdam.Id);
			result.First().Name.Should().Be.EqualTo(personAdam.Name.ToString());
			result.Last().Id.Should().Be.EqualTo(personScott.Id);
			result.Last().Name.Should().Be.EqualTo(personScott.Name.ToString());
		}
	}
}
