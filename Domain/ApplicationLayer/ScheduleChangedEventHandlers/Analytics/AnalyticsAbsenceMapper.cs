using System;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsAbsenceMapper
	{
		private readonly IAnalyticsAbsenceRepository _analyticsAbsenceRepository;

		public AnalyticsAbsenceMapper(IAnalyticsAbsenceRepository analyticsAbsenceRepository)
		{
			_analyticsAbsenceRepository = analyticsAbsenceRepository;
		}
		
		public virtual AnalyticsAbsence Map(Guid absenceCode)
		{
			return _analyticsAbsenceRepository.Absence(absenceCode);
		}
	}
}