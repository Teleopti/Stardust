using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	[TestFixture]
	public class DateBoxGlobalizationViewModelFactoryTest
	{
		public class CurrentThreadCultureProvider : ICultureProvider
		{
			#region ICultureProvider Members

			public CultureInfo GetCulture()
			{
				return CultureInfo.CurrentCulture;
			}

			#endregion
		}

		[Test, SetCulture("ar-SA"), SetUICulture("ar-SA")]
		public void ShouldCreateModelFromProvidedArSaCulture()
		{
			var target = new DateBoxGlobalizationViewModelFactory(new CurrentThreadCultureProvider());

			var result = target.CreateDateBoxGlobalizationViewModel();

			result.DateFormat.Should().Be.EqualTo("DD/MM/YYYY");
			result.DateFieldOrder.Should().Have.SameSequenceAs(new[] { "d", "m", "y" });
			result.IsRtl.Should().Be.True();
		}

		[Test, SetCulture("en-US"), SetUICulture("en-US")]
		public void ShouldCreateModelFromProvidedEnUsCulture()
		{
			var target = new DateBoxGlobalizationViewModelFactory(new CurrentThreadCultureProvider());

			var result = target.CreateDateBoxGlobalizationViewModel();

			result.DaysOfWeek[0].Should().Be.EqualTo("Sunday");
			result.DateFormat.Should().Be.EqualTo("mm/dd/YYYY");
			result.DateFieldOrder.Should().Have.SameSequenceAs(new[] {"m", "d", "y"});
			result.IsRtl.Should().Be.False();
		}

		[Test, SetCulture("sv-SE"), SetUICulture("sv-SE")]
		public void ShouldCreateModelFromProvidedSwedishCulture()
		{
			var target = new DateBoxGlobalizationViewModelFactory(new CurrentThreadCultureProvider());

			var result = target.CreateDateBoxGlobalizationViewModel();

			result.SetDateButtonLabel.Should().Be.EqualTo(Resources.SelectDates);
			result.CalTodayButtonLabel.Should().Be.EqualTo(Resources.Today);
			result.Tooltip.Should().Be.EqualTo(Resources.SelectDates);
			result.DateFieldOrder.Should().Have.SameSequenceAs(new[] {"y", "m", "d"});
			result.IsRtl.Should().Be.False();
			result.DaysOfWeek[0].Should().Be.EqualTo("söndag");
		}
	}

	/*
 * jQuery Mobile Framework : plugin to provide a date and time picker.
 * Copyright (c) JTSage
 * CC 3.0 Attribution.  May be relicensed without permission/notifcation.
 * https://github.com/jtsage/jquery-mobile-datebox
 *
 * Swedish localisation for JQM DateBox plugin *
 * Written by: Henrik Ekselius (henrik@xelius.net)
 
 
jQuery.extend(jQuery.mobile.datebox.prototype.options.lang, {
	'sv': {
		setDateButtonLabel: 'Välj datum',
		setTimeButtonLabel: 'Välj tid',
		setDurationButtonLabel: 'Välj varaktighet',
		calTodayButtonLabel: 'Gå till idag',
		titleDateDialogLabel: 'Välj datum',
		titleTimeDialogLabel: 'Välj tid',
		daysOfWeek: ['Söndag','Måndag','Tisdag','Onsdag','Torsdag','Fredag','Lördag'],
		daysOfWeekShort: ['Sö','Må','Ti','On','To','Fr','Lö'],
		monthsOfYear: ['Januari','Februari','Mars','April','Maj','Juni','July','Augusti','September','Oktober','November','December'],
		monthsOfYearShort: ['Jan','Feb','Mar','Apr','Maj','Jun','Jul','Aug','Sep','Okt','Nov','Dec'],
		durationLabel: ['Dagar','Timmar','Minuter','Sekunder'],
		durationDays: ['Dag','Dagar'],
		timeFormat: 24,
		dateFieldOrder: ['y', 'm', 'd'],
		timeFieldOrder: ['h', 'i', 'a'],
		slideFieldOrder: ['y', 'm', 'd'],
		headerFormat: 'ddd, dd mmm, YYYY',
		dateFormat: 'YYYY-MM-DD',
		isRTL: false
	}
});
jQuery.extend(jQuery.mobile.datebox.prototype.options, {
	useLang: 'sv'
});
	 * */

	/*jQuery.extend(jQuery.mobile.datebox.prototype.options.lang, {
    'en-US': {
        setDateButtonLabel: 'Set Date',
        setTimeButtonLabel: 'Set Time',
        setDurationButtonLabel: 'Set Duration',
        calTodayButtonLabel: 'Jump to Today',
        titleDateDialogLabel: 'Choose Date',
        titleTimeDialogLabel: 'Choose Time',
        daysOfWeek: ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'],
        daysOfWeekShort: ['Su','Mo','Tu','We','Th','Fr','Sa'],
        monthsOfYear: ['January','February','March','April','May','June','July','August','September','October','Novemeber','December'],
        monthsOfYearShort: ['Jan','Feb','Mar','Arp','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
        durationLabel: ['Days','Hours','Minutes','Seconds'],
        durationDays: ['Day','Days'],
        timeFormat: 12,
        dateFieldOrder: ['m', 'd', 'y'],
        timeFieldOrder: ['h', 'i', 'a'],
        slideFieldOrder: ['y', 'm', 'd'],
        headerFormat: 'ddd, mmm dd, YYYY',
	tooltip: 'Open Date Picker',
	dateFormat: 'dd/mm/YYYY',
	useArabicIndic: false,
        isRTL: false
   
}
) ;*/
}