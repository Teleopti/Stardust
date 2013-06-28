using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class AbsenceTypesProviderTest
	{
		[Test]
		public void ShouldReturnAbsenceTypes()
		{
			var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			var expected = new List<IAbsence> {new Absence()};
			absenceRepository.Stub(x => x.LoadRequestableAbsence()).Return(expected);

			var target = new AbsenceTypesProvider(absenceRepository);
			var result = target.GetRequestableAbsences();

			result.Should().Be.SameInstanceAs(expected);
		}
	}
}
