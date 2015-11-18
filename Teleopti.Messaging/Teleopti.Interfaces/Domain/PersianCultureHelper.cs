using System;
using System.Globalization;
using System.Reflection;

namespace Teleopti.Interfaces.Domain
{
	public static class PersianCultureHelper
	{
		private static readonly FieldInfo CultureInfoReadOnly = typeof(CultureInfo).GetField("m_isReadOnly", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo CultureInfoCalendar = typeof(CultureInfo).GetField("calendar", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo CultureDataField = typeof(CultureInfo).GetField("m_cultureData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo DateTimeFormatInfoReadOnly = typeof(DateTimeFormatInfo).GetField("m_isReadOnly", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo DateTimeFormatInfoCalendar = typeof(DateTimeFormatInfo).GetField("calendar", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
	

		/// <summary> 
		/// Fixes the DateTimeFormatInfo for Persian resources (months and week day names), and optionally sets the calendar to PersianCalendar. 
		/// </summary> 
		/// <param name="info">The DateTimeFormatInfo to be fixed.</param> 
		/// <param name="usePersianCalendar">If set, the calendar will be set to PersianCalendar.</param> 
		/// <returns>The fixed DateTimeFormatInfo.</returns> 
		public static DateTimeFormatInfo FixPersianDateTimeFormat(DateTimeFormatInfo info, bool usePersianCalendar)
		{
			if (info == null)
				info = new DateTimeFormatInfo();
			info.Calendar = new HijriCalendar();

			var readOnly = (bool)DateTimeFormatInfoReadOnly.GetValue(info);
			if (readOnly)
			{
				DateTimeFormatInfoReadOnly.SetValue(info, false);
			}
			if (usePersianCalendar)
			{
				DateTimeFormatInfoCalendar.SetValue(info, new PersianCalendar());
			}
			if (info.Calendar.GetType() == typeof(PersianCalendar))
			{
				translateCalendarResources(info);
			}
			if (readOnly)
			{
				DateTimeFormatInfoReadOnly.SetValue(info, true);
			}
			return info;
		}

		private static void translateCalendarResources(DateTimeFormatInfo info)
		{

			info.AbbreviatedDayNames = new string[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
			info.ShortestDayNames = new string[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
			info.DayNames = new string[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
			info.AbbreviatedMonthNames = new string[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
			info.AbbreviatedMonthGenitiveNames = new string[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
			info.MonthNames = new string[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
			info.MonthGenitiveNames = new string[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
			info.FirstDayOfWeek = DayOfWeek.Saturday;
			info.FullDateTimePattern = "yyyy MMMM dddd, dd HH:mm:ss";
			info.LongDatePattern = "yyyy MMMM dddd, dd";
			info.ShortDatePattern = "yyyy/MM/dd";
			info.ShortTimePattern = "HH:mm";
		}

		/// <summary> 
		/// Fixes CultureInfo for Persian resoures (months and day names) and also PersianCalendar. 
		/// </summary> 
		/// <param name="culture">The CultureInfo instace to be fixed. If NULL, a new instance will be created and returned.</param> 
		/// <returns>A new instance of fixed Persian CultureInfo.</returns> 
		public static CultureInfo FixPersianCulture(this CultureInfo culture)
		{

			if (culture == null)
				culture = new CultureInfo("fa-IR", false);
			if (culture.LCID != 1065)
				return culture;

			fixOptionalCalendars(culture, 4);
			culture = createNewCultureInfoAndEnsureNotReadOnly();

			culture.DateTimeFormat = FixPersianDateTimeFormat(culture.DateTimeFormat, true);
			CultureInfoCalendar.SetValue(culture, new PersianCalendar());

			makeCultureReadOnly(culture);

			return culture;
		}

		public static CultureInfo FixPersianCulture(this CultureInfo culture, bool onlyTranslateResources)
		{
			return onlyTranslateResources ? fixPersianCultureResourcesOnly(culture) : FixPersianCulture(culture);
		}

		private static CultureInfo createNewCultureInfoAndEnsureNotReadOnly()
		{
			var culture = new CultureInfo ("fa-IR", false);

			var readOnly = (bool) CultureInfoReadOnly.GetValue (culture);
			if (readOnly)
			{
				CultureInfoReadOnly.SetValue (culture, false);
			}
			return culture;
		}

		private static CultureInfo fixPersianCultureResourcesOnly (CultureInfo culture)
		{
			if (culture == null)
				culture = new CultureInfo ("fa-IR", false);
			if (culture.LCID != 1065)
				return culture;

			culture = createNewCultureInfoAndEnsureNotReadOnly();

			translateCalendarResources (culture.DateTimeFormat);

			makeCultureReadOnly(culture);

			return culture;
		}

		private static void makeCultureReadOnly (CultureInfo culture)
		{
			CultureInfoReadOnly.SetValue (culture, true);
		}

		private static void fixOptionalCalendars(CultureInfo culture, int calendarIndex)
		{
			Object cultureData = CultureDataField.GetValue(culture);

			PropertyInfo calendarIDProp = cultureData.GetType().GetProperty("CalendarIds",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var calendars = (int[])calendarIDProp.GetValue(cultureData, null);
			if (calendarIndex >= 0 && calendarIndex < calendars.Length)
				calendars[calendarIndex] = 0x16;
		}
	}
}