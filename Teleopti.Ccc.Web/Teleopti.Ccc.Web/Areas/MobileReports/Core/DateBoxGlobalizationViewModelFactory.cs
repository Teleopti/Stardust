using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public class DateBoxGlobalizationViewModelFactory : IDateBoxGlobalizationViewModelFactory
	{
		private readonly IUserCulture _userCulture;

		public DateBoxGlobalizationViewModelFactory(IUserCulture userCulture)
		{
			_userCulture = userCulture;
		}

		#region IDateBoxGlobalizationViewModelFactory Members

		public DateBoxGlobalizationViewModel CreateDateBoxGlobalizationViewModel()
		{
			var currentCulture = _userCulture.GetCulture();
			var dateTimeFormatInfo = currentCulture.DateTimeFormat;

			return new DateBoxGlobalizationViewModel
			       	{
			       		CalTodayButtonLabel = Resources.Today,
			       		SetDateButtonLabel = Resources.SelectDates,
			       		Tooltip = Resources.SelectDates,
			       		DaysOfWeek = dateTimeFormatInfo.DayNames,
			       		DaysOfWeekShort = dateTimeFormatInfo.AbbreviatedDayNames,
			       		MonthsOfYear = dateTimeFormatInfo.MonthNames,
			       		MonthsOfYearShort = dateTimeFormatInfo.AbbreviatedMonthNames,
			       		IsRtl = currentCulture.TextInfo.IsRightToLeft,
			       		DateFieldOrder =
			       			ToDateFieldOrderArray(dateTimeFormatInfo.ShortDatePattern, dateTimeFormatInfo.DateSeparator, currentCulture),
			       		HeaderFormat = dateTimeFormatInfo.LongDatePattern,
			       		DateFormat =
			       			AdjustDateFormat(dateTimeFormatInfo.ShortDatePattern, dateTimeFormatInfo.DateSeparator),
							CalStartDay = (int)dateTimeFormatInfo.FirstDayOfWeek,
						TitleDateDialogLabel = Resources.SelectDates,
						NextMonth = Resources.DoubleArrowAdd,
						PreviousMonth = Resources.DoubleArrowRemove,
						UseArabicIndic = currentCulture.TextInfo.IsRightToLeft,
			       	};
		}

		#endregion

		private string AdjustDateFormat(string shortDatePattern, string dateSeparator)
		{
			string[] strings = shortDatePattern.Split(new[] {dateSeparator}, StringSplitOptions.RemoveEmptyEntries);
			return string.Join(dateSeparator, strings.Select(s =>
			                                                 	{
			                                                 		if ("d".Equals(s)) return "%d";
			                                                 		if ("dd".Equals(s)) return "%d";
			                                                 		if ("DD".Equals(s)) return "%d";
			                                                 		if ("M".Equals(s)) return "%m";
			                                                 		if ("MM".Equals(s)) return "%m";
			                                                 		if ("yy".Equals(s)) return "%Y";
			                                                 		if ("yyyy".Equals(s)) return "%Y";
			                                                 		return s;
			                                                 	}).ToArray());
		}

		private static string[] ToDateFieldOrderArray(string shortDatePattern, string dateSeparator,
		                                              CultureInfo currentCulture)
		{
			return
				shortDatePattern.Split(new[] {dateSeparator}, StringSplitOptions.RemoveEmptyEntries).Select(
					d => d[0].ToString(currentCulture).ToLower(currentCulture)).ToArray();
		}
	}
}