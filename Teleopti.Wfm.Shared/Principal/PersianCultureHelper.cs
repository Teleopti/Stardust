using System;
using System.Globalization;
using System.Reflection;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public static class PersianCultureHelper
	{
		private static readonly FieldInfo CultureInfoReadOnly = typeof(CultureInfo).GetField("m_isReadOnly", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo CultureInfoCalendar = typeof(CultureInfo).GetField("calendar", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo CultureDataField = typeof(CultureInfo).GetField("m_cultureData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo DateTimeFormatInfoReadOnly = typeof(DateTimeFormatInfo).GetField("m_isReadOnly", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo DateTimeFormatInfoCalendar = typeof(DateTimeFormatInfo).GetField("calendar", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
	
		public static DateTimeFormatInfo FixPersianDateTimeFormat(DateTimeFormatInfo info)
		{
			if (info == null)
				info = new DateTimeFormatInfo();
			info.Calendar = new HijriCalendar();

			var readOnly = (bool)DateTimeFormatInfoReadOnly.GetValue(info);
			if (readOnly)
			{
				DateTimeFormatInfoReadOnly.SetValue(info, false);
			}
			
			DateTimeFormatInfoCalendar.SetValue (info, new PersianCalendar());
			
			translateCalendarResources(info);
			
			if (readOnly)
			{
				DateTimeFormatInfoReadOnly.SetValue(info, true);
			}
			return info;
		}

		private static void translateCalendarResources(DateTimeFormatInfo info)
		{

			info.AbbreviatedDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
			info.ShortestDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
			info.DayNames = new[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
			info.AbbreviatedMonthNames = new[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
			info.AbbreviatedMonthGenitiveNames = new[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
			info.MonthNames = new[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
			info.MonthGenitiveNames = new[] { "فروردین", "ارديبهشت", "خرداد", "تير", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
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

			culture.DateTimeFormat = FixPersianDateTimeFormat(culture.DateTimeFormat);
			CultureInfoCalendar.SetValue(culture, new PersianCalendar());

			makeCultureReadOnly(culture);

			return culture;
		}

		public static CultureInfo FixPersianCulture(this CultureInfo culture, bool forceGregorianCalendar)
		{
			return forceGregorianCalendar ? 
				PersianCultureHelper.forceGregorianCalendar(culture) : 
				FixPersianCulture (culture);
		}

		private static CultureInfo forceGregorianCalendar (CultureInfo culture)
		{
			if (culture.Calendar is GregorianCalendar)
			{
				return culture;
			}

			var info = new DateTimeFormatInfo();
			var gregorianCalendar = new GregorianCalendar();

			var newCulture = fixPersianCultureResourcesOnly (culture);
			CultureInfoCalendar.SetValue (newCulture, gregorianCalendar);
			DateTimeFormatInfoCalendar.SetValue (info, gregorianCalendar);

			DateTimeFormatInfoReadOnly.SetValue (info, true);
			newCulture.DateTimeFormat = info;

			makeCultureReadOnly (newCulture);
			return newCulture;
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

			//makeCultureReadOnly(culture);

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