using System;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsActivity
	{
		int ActivityId { get; set; }
		Guid ActivityCode { get; set; }
		string ActivityName { get; set; }
		int DisplayColor { get; set; }
		bool InReadyTime { get; set; }
		string InReadyTimeName { get; set; }
		bool InContractTime { get; set; }
		string InContractTimeName { get; set; }
		bool InPaidTime { get; set; }
		string InPaidTimeName { get; set; }
		bool InWorkTime { get; set; }
		string InWorkTimeName { get; set; }
		int BusinessUnitId { get; set; }
		int DatasourceId { get; set; }
		DateTime DatasourceUpdateDate { get; set; }
		bool IsDeleted { get; set; }
		string DisplayColorHtml { get; set; }
	}
}
