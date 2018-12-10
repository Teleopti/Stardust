using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class TimeZoneTransformer
	{
		private readonly DateTime _insertDateTime;

		private TimeZoneTransformer() { }

		public TimeZoneTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void TransformDim(IList<TimeZoneDim> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (TimeZoneDim timeZoneDim in rootList)
			{
				AddRowToDimensionDataTable(timeZoneDim, table);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void TransformBridge(IList<TimeZoneBridge> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (TimeZoneBridge timeZoneBridge in rootList)
			{
				AddRowToBridgeDataTable(timeZoneBridge, table);
			}
		}

		private void AddRowToDimensionDataTable(TimeZoneDim timeZoneDim, DataTable table)
		{
			DataRow row = table.NewRow();

			row["time_zone_code"] = timeZoneDim.TimeZoneCode;
			row["time_zone_name"] = timeZoneDim.TimeZoneName;
			row["default_zone"] = timeZoneDim.IsDefaultTimeZone;
			row["utc_conversion"] = timeZoneDim.UtcConversion;
			row["utc_conversion_dst"] = timeZoneDim.UtcConversionDst;
			row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
			row["insert_date"] = _insertDateTime;
			row["update_date"] = _insertDateTime;
			row["utc_in_use"] = timeZoneDim.IsUtcInUse;

			table.Rows.Add(row);
		}

		private void AddRowToBridgeDataTable(TimeZoneBridge timeZoneBridge, DataTable table)
		{
			DataRow row = table.NewRow();

			row["date"] = timeZoneBridge.Date;
			row["interval_id"] = timeZoneBridge.IntervalId;
			row["time_zone_code"] = timeZoneBridge.TimeZoneCode;
			row["local_date"] = timeZoneBridge.LocalDate;
			row["local_interval_id"] = timeZoneBridge.LocalIntervalId;
			row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
			row["insert_date"] = _insertDateTime;
			row["update_date"] = _insertDateTime;

			table.Rows.Add(row);
		}
	}
}