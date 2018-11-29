using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class IntervalTransformer : IEtlTransformer<Interval>
	{
		private readonly DateTime _insertDateTime;

		private IntervalTransformer() { }

		public IntervalTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<Interval> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (Interval interval in rootList)
			{
				DataRow row = table.NewRow();

				row["interval_id"] = interval.Id;
				row["interval_name"] = interval.IntervalName;
				row["halfhour_name"] = interval.HalfHourName;
				row["hour_name"] = interval.HourName;
				row["interval_start"] = interval.Period.StartDateTime;
				row["interval_end"] = interval.Period.EndDateTime;
				row["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				row["insert_date"] = _insertDateTime;
				row["update_date"] = _insertDateTime;

				table.Rows.Add(row);
			}
		}
	}
}
