using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule
{
	public class ScheduleExcelExportData
	{
		public string SelectedGroups;
		public DateTime DateFrom;
		public DateTime DateTo;
		public string Scenario;
		public string[] OptionalColumns;
		public string Timezone;
		public PersonRow[] PersonRows;
	}

	public class ScheduleDaySummary
	{
		public DateOnly Date;
		public string Summary;

	}

	public class PersonRow
	{
		public string Name;
		public string EmploymentNumber;
		public string[] OptionalColumns;
		public string SiteNTeam;
		public ScheduleDaySummary[] ScheduleDaySummarys;
	}

	public class ProcessExportResult
	{
		public string FailReason;
		public byte[] Data;
	}
	public class ExportScheduleForm
	{
		public DateOnly StartDate;
		public DateOnly EndDate;
		public Guid ScenarioId;
		public string TimezoneId;
		public SearchGroupIdsData SelectedGroups;
		public Guid[] OptionalColumnIds;
	}
}