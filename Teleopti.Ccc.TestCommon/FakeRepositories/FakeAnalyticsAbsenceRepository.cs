using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsAbsenceRepository : IAnalyticsAbsenceRepository
	{
		private readonly List<AnalyticsAbsence> fakeAbsences;

		public FakeAnalyticsAbsenceRepository()
		{
			fakeAbsences = new List<AnalyticsAbsence>();
		}

		public FakeAnalyticsAbsenceRepository(List<AnalyticsAbsence> absences)
		{
			fakeAbsences = absences;
		}
		public IList<AnalyticsAbsence> Absences()
		{
			return fakeAbsences;
		}
	}
}