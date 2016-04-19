using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsFactSchedulePreference
	{
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public int PersonId { get; set; }
		public int ScenarioId { get; set; }
		public int PreferenceTypeId { get; set; }
		public int ShiftCategoryId { get; set; }
		public int DayOffId { get; set; }
		public int PreferencesRequested { get; set; }
		public int PreferencesFulfilled { get; set; }
		public int PreferencesUnfulfilled { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
		public DateTime InsertDate { get; set; }
		public DateTime UpdateDate { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
		public int MustHaves { get; set; }
		public int AbsenceId { get; set; }
	}
}