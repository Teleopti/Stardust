using System.Runtime.Serialization;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout
{
	[DataContract]
	public class DateBoxGlobalizationViewModel
	{
		[DataMember(Name = "calTodayButtonLabel")]
		public string CalTodayButtonLabel { get; set; }

		[DataMember(Name = "daysOfWeek")]
		public string[] DaysOfWeek { get; set; }

		[DataMember(Name = "daysOfWeekShort")]
		public string[] DaysOfWeekShort { get; set; }

		[DataMember(Name = "monthsOfYear")]
		public string[] MonthsOfYear { get; set; }

		[DataMember(Name = "monthsOfYearShort")]
		public string[] MonthsOfYearShort { get; set; }

		[DataMember(Name = "timeFormat")]
		public string TimeFormat { get; set; }

		[DataMember(Name = "dateFieldOrder")]
		public string[] DateFieldOrder { get; set; }

		[DataMember(Name = "headerFormat")]
		public string HeaderFormat { get; set; }

		[DataMember(Name = "dateFormat")]
		public string DateFormat { get; set; }

		[DataMember(Name = "calStartDay")]
		public int CalStartDay { get; set; }


		[DataMember(Name = "isRTL")]
		public bool IsRtl { get; set; }

		[DataMember(Name = "setDateButtonLabel")]
		public string SetDateButtonLabel { get; set; }

		[DataMember(Name = "tooltip")]
		public string Tooltip { get; set; }
	}
}