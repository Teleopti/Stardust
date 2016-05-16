using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsAbsenceRepository : IAnalyticsAbsenceRepository
	{
		private readonly List<IAnalyticsAbsence> fakeAbsences;

		public FakeAnalyticsAbsenceRepository()
		{
			fakeAbsences = new List<IAnalyticsAbsence>();
		}

		public FakeAnalyticsAbsenceRepository(List<IAnalyticsAbsence> absences)
		{
			fakeAbsences = absences;
		}
		public IList<IAnalyticsAbsence> Absences()
		{
			return fakeAbsences;
		}
	}
}