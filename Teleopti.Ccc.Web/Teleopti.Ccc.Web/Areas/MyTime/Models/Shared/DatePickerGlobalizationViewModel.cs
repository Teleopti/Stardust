using System.Runtime.Serialization;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Shared
{
	[DataContract]
	public class DatePickerGlobalizationViewModel
	{
		[DataMember(Name = "closeText")]
		public string CloseText { get; set; }

		[DataMember(Name = "prevText")]
		public string PreviousText { get; set; }

		[DataMember(Name = "nextText")]
		public string NextText { get; set; }

		[DataMember(Name = "monthNames")]
		public string[] MonthNames { get; set; }

		[DataMember(Name = "monthNamesShort")]
		public string[] MonthNamesShort { get; set; }

		[DataMember(Name = "dayNames")]
		public string[] DayNames { get; set; }

		[DataMember(Name = "dayNamesShort")]
		public string[] DayNamesShort { get; set; }

		[DataMember(Name = "dayNamesMin")]
		public string[] DayNamesMin { get; set; }

		[DataMember(Name = "firstDay")]
		public int FirstDay { get; set; }

		[DataMember(Name = "isRTL")]
		public bool IsRtl { get; set; }

		[DataMember(Name = "dateFormat")]
		public string DateFormat { get; set; }

		[DataMember(Name = "weekHeader")]
		public string WeekHeader { get; set; }

		[DataMember(Name = "currentText")]
		public string CurrentText { get; set; }

		[DataMember(Name = "showWeek")]
		public bool ShowWeek { get; set; }

		[DataMember(Name = "showMonthAfterYear")]
		public bool ShowMonthAfterYear { get; set; }

		[DataMember(Name = "yearSuffix")]
		public string YearSuffix { get; set; }
	}
}