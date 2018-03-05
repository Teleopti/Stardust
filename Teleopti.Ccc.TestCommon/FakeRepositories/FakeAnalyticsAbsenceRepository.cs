using System;
using System.Collections.Generic;
using System.Linq;
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

		public void AddAbsence(AnalyticsAbsence analyticsAbsence)
		{
			analyticsAbsence.AbsenceId = fakeAbsences.Any() ? fakeAbsences.Max(a => a.AbsenceId) + 1 : 1;
			fakeAbsences.Add(analyticsAbsence);
		}

		public void UpdateAbsence(AnalyticsAbsence analyticsAbsence)
		{
			analyticsAbsence.AbsenceId = fakeAbsences.First(a => a.AbsenceCode == analyticsAbsence.AbsenceCode).AbsenceId;
			fakeAbsences.RemoveAll(a => a.AbsenceCode == analyticsAbsence.AbsenceCode);
			fakeAbsences.Add(analyticsAbsence);
		}

		public AnalyticsAbsence Absence(Guid absenceId)
		{
			return fakeAbsences.FirstOrDefault(a => a.AbsenceCode == absenceId);
		}
	}
}