using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	public class AbsenceTypesProviderTest
	{
		[Test]
		public void ShouldReturnAbsenceTypes()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();
			var expected = new List<IAbsence> {new Absence()};
			absenceRepository.Stub(x => x.LoadRequestableAbsence()).Return(expected);

			var target = new AbsenceTypesProvider(absenceRepository, loggedOnUser);
			var result = target.GetRequestableAbsences();

			result.Should().Be.SameInstanceAs(expected);
		}

		[Test]
		public void ShouldReturnReportableAbsenceTypes()
		{
			var expected = new List<IAbsence> { new Absence() };

			var wfcs = MockRepository.GenerateMock<IWorkflowControlSet>();
			wfcs.Stub(x => x.AllowedAbsencesForReport).Return(expected);
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser().WorkflowControlSet).Return(wfcs);

			var absenceRepository = MockRepository.GenerateMock<IAbsenceRepository>();

			var target = new AbsenceTypesProvider(absenceRepository, loggedOnUser);
			var result = target.GetReportableAbsences();

			result.Should().Be.SameInstanceAs(expected);
		}
	}
}
