using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class DateTransformer : IEtlTransformer<DayDate>
	{
		private readonly DateTime _insertDateTime;

		public DateTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		private DateTransformer()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<DayDate> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (DayDate date in rootList)
			{
				CreateDataRow(date, table);
			}
		}

		private void CreateDataRow(DayDate date, DataTable table)
		{
			DataRow dataRow = table.NewRow();

			dataRow["date_date"] = date.DateDate;
			dataRow["year"] = date.Year;
			dataRow["year_month"] = date.YearMonth;
			dataRow["month"] = date.Month;
			dataRow["month_name"] = date.MonthName;
			dataRow["month_resource_key"] = date.MonthResourceKey;
			dataRow["day_in_month"] = date.DayInMonth;
			dataRow["weekday_number"] = date.WeekdayNumber;
			dataRow["weekday_name"] = date.WeekdayName;
			dataRow["weekday_resource_key"] = date.WeekdayResourceKey;
			dataRow["week_number"] = date.WeekNumber;
			dataRow["year_week"] = date.YearWeek;
			dataRow["quarter"] = date.Quarter;
			dataRow["insert_date"] = _insertDateTime;

			table.Rows.Add(dataRow);
		}

		public static IList<DayDate> CreateDateList(DateTimePeriod period)
		{
			IList<DayDate> dateList = new List<DayDate>();
			DateTime startDate = period.StartDateTime.Date;

			while (startDate <= period.EndDateTime.Date)
			{
				dateList.Add(new DayDate(startDate, CultureInfo.CurrentCulture));
				startDate = startDate.AddDays(1);
			}
			return dateList;
		}
	}
}
