$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Preference day view model');

	test('should read preference', function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		viewModelDay.ReadPreference({
			Preference: 'a shift category',
			MustHave: true,
			Color: '0,0,0',
			Extended: true,
			ExtendedTitle: 'ExtendedTitle',
			StartTimeLimitation: '8:00 am-9:00 am',
			EndTimeLimitation: '5:00 pm-6:00 pm',
			WorkTimeLimitation: '8:00-9:00',
			Activity: 'Lunch',
			ActivityStartTimeLimitation: '8:00 am-9:00 am',
			ActivityEndTimeLimitation: '5:00 pm-6:00 pm',
			ActivityTimeLimitation: '1:00-2:00'
		});

		equal(viewModelDay.Preference(), 'a shift category');
		equal(viewModelDay.MustHave(), true);
		equal(viewModelDay.Color(), 'rgb(0,0,0)');
		equal(viewModelDay.Extended(), true);
		equal(viewModelDay.ExtendedTitle(), 'ExtendedTitle');
		equal(viewModelDay.StartTimeLimitation(), '8:00 am-9:00 am');
		equal(viewModelDay.EndTimeLimitation(), '5:00 pm-6:00 pm');
		equal(viewModelDay.WorkTimeLimitation(), '8:00-9:00');
		equal(viewModelDay.Activity(), 'Lunch');
		equal(viewModelDay.ActivityStartTimeLimitation(), '8:00 am-9:00 am');
		equal(viewModelDay.ActivityEndTimeLimitation(), '5:00 pm-6:00 pm');
		equal(viewModelDay.ActivityTimeLimitation(), '1:00-2:00');
	});

	test('should read dayoff', function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		viewModelDay.ReadDayOff({
			DayOff: 'Day off'
		});

		equal(viewModelDay.DayOff(), 'Day off');
	});

	test('should read absence', function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		viewModelDay.ReadAbsence({
			Absence: 'Illness'
		});

		equal(viewModelDay.Absence(), 'Illness');
	});

	test('should read person assignment', function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		viewModelDay.ReadPersonAssignment({
			ShiftCategory: 'Late',
			TimeSpan: 'PersonAssignmentTimeSpan',
			ContractTime: 'PersonAssignmentContractTime',
			ContractTimeMinutes: 'PersonAssignmentContractTimeMinutes'
		});

		equal(viewModelDay.PersonAssignmentShiftCategory(), 'Late');
		equal(viewModelDay.PersonAssignmentTimeSpan(), 'PersonAssignmentTimeSpan');
		equal(viewModelDay.PersonAssignmentContractTime(), 'PersonAssignmentContractTime');
		equal(viewModelDay.ContractTimeMinutes(), 'PersonAssignmentContractTimeMinutes');
	});

	test('should read bankholidaycalendar', function () {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		viewModelDay.ReadBankHolidayCalendar({
			CalendarId: '2567c32a-3c08-4930-a9c5-433bf6228caf',
			CalendarName: 'ChinaBankHolidayCalendar',
			DateDescription:'NewYear'
		});

		equal(viewModelDay.CalendarId(), '2567c32a-3c08-4930-a9c5-433bf6228caf');
		equal(viewModelDay.CalendarName(), 'ChinaBankHolidayCalendar');
		equal(viewModelDay.DateDescription(), 'NewYear');

	});

	test('should load preference', function() {
		var ajax = function(model, options) {
			equal(options.data.Date, '2012-06-11');
			options.success({
				Preference: 'a shift category',
				MustHave: true,
				Color: '0,0,0',
				Extended: true,
				ExtendedTitle: 'ExtendedTitle',
				StartTimeLimitation: '8:00 am-9:00 am',
				EndTimeLimitation: '5:00 pm-6:00 pm',
				WorkTimeLimitation: '8:00-9:00',
				Activity: 'Lunch',
				ActivityStartTimeLimitation: '8:00 am-9:00 am',
				ActivityEndTimeLimitation: '5:00 pm-6:00 pm',
				ActivityTimeLimitation: '1:00-2:00'
			});
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = '2012-06-11';

		expect(13);

		viewModelDay.LoadPreference();

		equal(viewModelDay.Preference(), 'a shift category');
		equal(viewModelDay.MustHave(), true);
		equal(viewModelDay.Color(), 'rgb(0,0,0)');
		equal(viewModelDay.Extended(), true);
		equal(viewModelDay.ExtendedTitle(), 'ExtendedTitle');
		equal(viewModelDay.StartTimeLimitation(), '8:00 am-9:00 am');
		equal(viewModelDay.EndTimeLimitation(), '5:00 pm-6:00 pm');
		equal(viewModelDay.WorkTimeLimitation(), '8:00-9:00');
		equal(viewModelDay.Activity(), 'Lunch');
		equal(viewModelDay.ActivityStartTimeLimitation(), '8:00 am-9:00 am');
		equal(viewModelDay.ActivityEndTimeLimitation(), '5:00 pm-6:00 pm');
		equal(viewModelDay.ActivityTimeLimitation(), '1:00-2:00');
	});

	test('should bind extended', function() {
		var element = $('#qunit-fixture').append(
			"<div class='extended-indication' data-bind='visible: Extended' style='display: none'></div>"
		)[0];

		var dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		ko.applyBindings(dayViewModel, element);

		dayViewModel.Extended(true);

		equal($(element).is(':visible'), true);
	});

	test('should set preference', function() {
		var ajax = function(model, options) {
			var result = jQuery.parseJSON(options.data);

			equal(result.Date, '2012-06-11');
			equal(result.PreferenceId, 'id');
			options.success({
				Preference: 'a shift category',
				Color: '0,0,0',
				Extended: false
			});
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = '2012-06-11';

		expect(4);

		viewModelDay.SetPreference('id');

		equal(viewModelDay.Preference(), 'a shift category');
		equal(viewModelDay.Color(), 'rgb(0,0,0)');
	});

	test('should set extended preference', function() {
		var ajax = function(model, options) {
			var result = jQuery.parseJSON(options.data);

			equal(result.Date, '2012-09-03', 'Date');
			equal(result.PreferenceId, 'id1', 'PreferenceId');
			equal(result.EarliestStartTime, '8:00', 'StartTimeMinimum');
			equal(result.LatestStartTime, '9:00', 'StartTimeMaximum');
			equal(result.EarliestEndTime, '15:00');
			equal(result.LatestEndTime, '18:00');
			equal(result.WorkTimeMinimum, '6:00');
			equal(result.WorkTimeMaximum, '8:00');
			equal(result.ActivityPreferenceId, 'id2');
			equal(result.ActivityEarliestStartTime, '11:00');
			equal(result.ActivityLatestStartTime, '12:00');
			equal(result.ActivityEarliestEndTime, '12:02');
			equal(result.ActivityLatestEndTime, '13:00');
			equal(result.ActivityTimeMinimum, '1:00');
			equal(result.ActivityTimeMaximum, '0:30');
			options.success({
				Preference: 'a shift category',
				Color: '0,0,0',
				Extended: true
			});
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = '2012-09-03';

		expect(15 + 1);

		viewModelDay.SetPreference({
			PreferenceId: 'id1',
			EarliestStartTime: '8:00',
			LatestStartTime: '9:00',
			EarliestEndTime: '15:00',
			LatestEndTime: '18:00',
			WorkTimeMinimum: '6:00',
			WorkTimeMaximum: '8:00',
			ActivityPreferenceId: 'id2',
			ActivityEarliestStartTime: '11:00',
			ActivityLatestStartTime: '12:00',
			ActivityEarliestEndTime: '12:02',
			ActivityLatestEndTime: '13:00',
			ActivityTimeMinimum: '1:00',
			ActivityTimeMaximum: '0:30'
		});

		equal(viewModelDay.Extended(), true);
	});

	test('should delete preference', function() {
		var ajax = function(mode, options) {
			var result = jQuery.parseJSON(options.data);

			equal(result.Date, '2012-06-11');
			options.success({
				Preference: 'deleted!',
				Color: 'deleted!',
				Extended: 'deleted!'
			});
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = '2012-06-11';

		expect(4);

		viewModelDay.DeletePreference();

		equal(viewModelDay.Preference(), 'deleted!');
		equal(viewModelDay.Color(), 'rgb(deleted!)');
		equal(viewModelDay.Extended(), 'deleted!');
	});

	test('should format possible contract time', function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		viewModelDay.PossibleContractTimeMinutesLower(6 * 60 + 30);
		viewModelDay.PossibleContractTimeMinutesUpper(8 * 60 + 5);

		expect(2);
		equal(viewModelDay.PossibleContractTimeLower(), '6:30');
		equal(viewModelDay.PossibleContractTimeUpper(), '8:05');
	});

	test('should load feedback', function() {
		var ajax = function(model, options) {
			options.success({
				FeedbackError: 'an error',
				PossibleStartTimes: '6:00-9:00',
				PossibleEndTimes: '16:00-18:00',
				PossibleContractTimeMinutesLower: 7 * 60,
				PossibleContractTimeMinutesUpper: 12 * 60,
				RestTimeToNextDayTimeSpan: '10:00',
				RestTimeToPreviousDayTimeSpan: '11:00',
				ExpectedNightRestTimeSpan: '11:00'
			});
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = '2012-06-11';
		viewModelDay.Feedback(true);
		viewModelDay.LoadFeedback();

		equal(viewModelDay.FeedbackError(), 'an error');
		equal(viewModelDay.PossibleStartTimes(), '6:00-9:00');
		equal(viewModelDay.PossibleEndTimes(), '16:00-18:00');
		equal(viewModelDay.PossibleContractTimes(), '7:00-12:00');
	});

	test('should compute DisplayFeedbackError', function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		equal(viewModelDay.DisplayFeedbackError(), false, 'compute not initialized');

		viewModelDay.FeedbackError(null);
		equal(viewModelDay.DisplayFeedbackError(), false, 'compute with null');

		viewModelDay.FeedbackError(undefined);
		equal(viewModelDay.DisplayFeedbackError(), false, 'compute with undefined');

		viewModelDay.FeedbackError('');
		equal(viewModelDay.DisplayFeedbackError(), false, 'compute with empty string');

		viewModelDay.FeedbackError('an error');
		equal(viewModelDay.DisplayFeedbackError(), true, 'compute with string');
	});

	test('should compute DisplayFeedback', function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		equal(viewModelDay.DisplayFeedback(), false, 'compute not initialized');

		var resetViewModel = function() {
			viewModelDay.PossibleStartTimes(undefined);
			viewModelDay.PossibleEndTimes(undefined);
			viewModelDay.PossibleContractTimeMinutesLower(undefined);
			viewModelDay.PossibleContractTimeMinutesUpper(undefined);
			viewModelDay.FeedbackError(undefined);
		};

		resetViewModel();
		viewModelDay.PossibleStartTimes('a value');
		viewModelDay.FeedbackError('an error');
		equal(viewModelDay.DisplayFeedback(), false, 'compute with FeedbackError');

		resetViewModel();
		viewModelDay.PossibleStartTimes('a value');
		equal(viewModelDay.DisplayFeedback(), true, 'compute with PossibleStartTimes');

		resetViewModel();
		viewModelDay.PossibleEndTimes('a value');
		equal(viewModelDay.DisplayFeedback(), true, 'compute with PossibleEndTimes');

		resetViewModel();
		viewModelDay.PossibleContractTimeMinutesLower('a value');
		equal(viewModelDay.DisplayFeedback(), true, 'compute with PossibleContractTimeMinutesLower');

		resetViewModel();
		viewModelDay.PossibleContractTimeMinutesUpper('a value');
		equal(viewModelDay.DisplayFeedback(), true, 'compute with PossibleContractTimeMinutesUpper');
	});

	test('should set must have', function() {
		var ajax = function(model, options) {
			var result = jQuery.parseJSON(options.data);

			equal(result.Date, '2012-06-11');
			equal(result.MustHave, true);
			options.success(true);
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = '2012-06-11';

		expect(3);

		viewModelDay.SetMustHave(true);

		equal(viewModelDay.MustHave(), true);
	});

	test('should make night rest violation objects', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var date = '/Date(1454515200000)/';
		var currentDay = moment(date);
		var minusOneDay = moment(date).subtract(1, 'days');
		var plusOneDay = moment(date).add(1, 'days');

		var ajax = function(model, options) {
			options.success({
				RestTimeToNextDayTimeSpan: '10:00',
				RestTimeToPreviousDayTimeSpan: '10:00',
				ExpectedNightRestTimeSpan: '11:00',
				HasNightRestViolationToPreviousDay: true,
				HasNightRestViolationToNextDay: true,
				DateInternal: '/Date(1454515200000)/'
			});
		};
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Feedback(true);
		viewModelDay.LoadFeedback();

		var nightRestViolationObjs = viewModelDay.MakeNightRestViolationObjs();

		equal(nightRestViolationObjs[0].firstDay, Teleopti.MyTimeWeb.Common.FormatDate(minusOneDay)); //"2016-02-03"
		equal(nightRestViolationObjs[0].sencondDay, Teleopti.MyTimeWeb.Common.FormatDate(currentDay)); //"2016-02-04"
		equal(nightRestViolationObjs[0].nightRestTimes, '11:00');
		equal(nightRestViolationObjs[0].hoursBetweenTwoDays, '10:00');

		equal(nightRestViolationObjs[1].firstDay, Teleopti.MyTimeWeb.Common.FormatDate(currentDay)); //"2016-02-04"
		equal(nightRestViolationObjs[1].sencondDay, Teleopti.MyTimeWeb.Common.FormatDate(plusOneDay)); // "2016-02-05"
		equal(nightRestViolationObjs[1].nightRestTimes, '11:00');
		equal(nightRestViolationObjs[1].hoursBetweenTwoDays, '10:00');
	});

	test('should turn on the night rest violation switch', function() {
		var ajax = function(model, options) {
			options.success({
				RestTimeToNextDayTimeSpan: '10:00',
				RestTimeToPreviousDayTimeSpan: '10:00',
				ExpectedNightRestTimeSpan: '11:00',
				HasNightRestViolationToPreviousDay: true,
				HasNightRestViolationToNextDay: false
			});
		};
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Feedback(true);
		viewModelDay.LoadFeedback();

		equal(viewModelDay.NightRestViolationSwitch(), true);
	});
});
