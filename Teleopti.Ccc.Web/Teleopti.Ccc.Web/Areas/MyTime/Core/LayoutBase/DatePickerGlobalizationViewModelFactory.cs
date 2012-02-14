using System.Globalization;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase
{
	public class DatePickerGlobalizationViewModelFactory : IDatePickerGlobalizationViewModelFactory
	{
		public DatePickerGlobalizationViewModel CreateDatePickerGlobalizationViewModel()
		{
			var currentCulture = CultureInfo.CurrentCulture;
			var dateTimeFormatInfo = currentCulture.DateTimeFormat;

			return new DatePickerGlobalizationViewModel
			       	{
			       		CloseText = UserTexts.Resources.Close,
			       		PreviousText = UserTexts.Resources.ShowPrevious,
			       		NextText = UserTexts.Resources.ShowNext,
			       		DayNames = dateTimeFormatInfo.DayNames,
			       		DayNamesMin = dateTimeFormatInfo.ShortestDayNames,
			       		DayNamesShort = dateTimeFormatInfo.AbbreviatedDayNames,
			       		MonthNames = dateTimeFormatInfo.MonthNames,
			       		MonthNamesShort = dateTimeFormatInfo.AbbreviatedMonthNames,
			       		IsRtl = currentCulture.TextInfo.IsRightToLeft,
			       		FirstDay = (int) dateTimeFormatInfo.FirstDayOfWeek,
						DateFormat = DatePatternConverter.TojQueryPattern(dateTimeFormatInfo.ShortDatePattern),
						ShowMonthAfterYear = false,
						ShowWeek = false,
						WeekHeader = "",
						YearSuffix = "",
						CurrentText = "",
			       	};
		}
	}
}