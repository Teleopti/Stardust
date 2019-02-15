$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Preference initializer');

	test('should load preferences', function() {
		$('#qunit-fixture')
			.append("<li data-mytime-week='week' class='inperiod preference' />")
			.append("<li data-mytime-date='2012-06-11' class='inperiod preference' />")
			.append("<li data-mytime-date='2012-06-12' class='inperiod preference' />");

		var ajax = {
			Ajax: function(options) {
				if (options.url != 'Preference/PeriodFeedback') return;
				equal(options.data.startDate, '2012-06-11');
				equal(options.data.endDate, '2012-07-23');
			}
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);
		target.InitViewModels();
	});

	test('should load day feedback and bind', function() {
		$('#qunit-fixture')
			.html("<div class='warning-indicator'></div>")
			.append(
				"<div id='Preference-period-feedback-view' data-bind='text: PossibleResultContractTimeLower'>No data!</div>"
			)
			.append("<li data-mytime-week='week' />")
			.append(
				"<li data-mytime-date='2012-06-11' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />"
			)
			.append(
				"<li data-mytime-date='2012-06-12' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />"
			);

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					options.success([{ Date: '2012-06-11', Feedback: true }, { Date: '2012-06-12', Feedback: true }]);
				}
				if (options.url == 'PreferenceFeedback/Feedback') {
					if (options.data.Date == '2012-06-11')
						options.success({
							PossibleContractTimeMinutesLower: 6 * 60,
							RestTimeToNextDayTimeSpan: '10:00',
							RestTimeToPreviousDayTimeSpan: '11:00',
							ExpectedNightRestTimeSpan: '11:00'
						});
					if (options.data.Date == '2012-06-12')
						options.success({
							PossibleContractTimeMinutesLower: 8 * 60,
							RestTimeToNextDayTimeSpan: '10:00',
							RestTimeToPreviousDayTimeSpan: '11:00',
							ExpectedNightRestTimeSpan: '11:00'
						});
				}
				if (options.url == 'Preference/PeriodFeedback') {
					options.success([
						{
							Date: '2012-06-11',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						},
						{
							Date: '2012-06-12',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						}
					]);
				}
			}
		};

		expect(3);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);
		target.InitViewModels();

		equal($('#Preference-period-feedback-view').text(), '15:00');
		equal($('li[data-mytime-date="2012-06-11"]').text(), '7:30');
		equal($('li[data-mytime-date="2012-06-12"]').text(), '7:30');
	});

	test('should only load feedback for days with class feedback', function() {
		$('#qunit-fixture')
			.html("<div class='warning-indicator'></div>")
			.append("<li data-mytime-week='week' />")
			.append("<li data-mytime-date='2012-06-11' data-bind='text: PossibleContractTimeLower' />")
			.append(
				"<li data-mytime-date='2012-06-12' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />"
			);

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					options.success([{ Date: '2012-06-12', Feedback: true }]);
				}
				if (options.url == 'PreferenceFeedback/Feedback') {
					options.success({
						PossibleContractTimeMinutesLower: 8 * 60,
						RestTimeToNextDayTimeSpan: '10:00',
						RestTimeToPreviousDayTimeSpan: '11:00',
						ExpectedNightRestTimeSpan: '11:00'
					});
				}
				if (options.url == 'Preference/PeriodFeedback') {
					options.success([
						{
							Date: '2012-06-11',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						},
						{
							Date: '2012-06-12',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						}
					]);
				}
			}
		};

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		equal($('li[data-mytime-date="2012-06-11"]').text(), '');
		equal($('li[data-mytime-date="2012-06-12"]').text(), '7:30');
	});

	test('should compute with schedule data', function() {
		$('#qunit-fixture')
			.html("<div class='warning-indicator'></div>")
			.append(
				"<div id='Preference-period-feedback-view'><span data-bind='text: PossibleResultContractTimeLower' /><span data-bind='text: PossibleResultContractTimeUpper' /></div>"
			)
			.append("<li data-mytime-week='week'></li>")
			.append("<li data-mytime-date='2012-06-13' class='inperiod'></li>")
			.append("<li data-mytime-date='2012-06-14' class='inperiod'></li>");

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'PreferenceFeedback/Feedback') ok(true, 'feedback should not be loaded');
				if (options.url == 'Preference/PreferencesAndSchedules') {
					var data1 = {
						Date: '2012-06-13',
						PersonAssignment: {
							ContractTimeMinutes: 120
						}
					};
					var data2 = {
						Date: '2012-06-14',
						PersonAssignment: {
							ContractTimeMinutes: 120
						}
					};
					var data = [data1, data2];
					options.success(data);
				}
				if (options.url == 'Preference/PeriodFeedback') {
					options.success([
						{
							Date: '2012-06-13',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						},
						{
							Date: '2012-06-14',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						}
					]);
				}
			}
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);
		target.InitViewModels();

		equal(
			$('#Preference-period-feedback-view [data-bind*="PossibleResultContractTimeLower"]').text(),
			'4:00',
			'lower contract time'
		);
		equal(
			$('#Preference-period-feedback-view [data-bind*="PossibleResultContractTimeUpper"]').text(),
			'4:00',
			'upper contract time'
		);
	});

	test('should load period feedback', function() {
		var fakeHtml = '<ul id="fakePreferenceHtml"><li data-mytime-date="2012-06-13"></li><ul>';
		$('body').append(fakeHtml);

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					options.success();
				}
				if (options.url == 'Preference/PeriodFeedback') {
					equal(options.url, 'Preference/PeriodFeedback', 'period feedback ajax url');
					equal(options.data.startDate, '2012-06-13', 'period feedback date');
				}
			}
		};
		var portal = {
			CurrentFixedDate: function() {
				return '2012-06-13';
			}
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax, portal);
		target.InitViewModels();

		$('#fakePreferenceHtml').remove();
	});

	test('should clear day view models on init', function() {
		$('#qunit-fixture')
			.html("<div class='warning-indicator'></div>")
			.append(
				"<div id='Preference-period-feedback-view' data-bind='text: PossibleResultContractTimeLower'>No data!</div>"
			)
			.append("<li data-mytime-week='week' />")
			.append("<li data-mytime-date='2012-06-19' class='inperiod feedback' />");

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					if (options.data.From == '2012-06-19') {
						options.success([{ Date: '2012-06-19', Feedback: true }]);
					}
					if (options.data.From == '2012-06-20') {
						options.success([{ Date: '2012-06-20', Feedback: true }]);
					}
				}
				if (options.url == 'PreferenceFeedback/Feedback') {
					if (options.data.Date == '2012-06-19' || options.data.Date == '2012-06-20')
						options.success({
							PossibleContractTimeMinutesLower: 6 * 60,
							RestTimeToNextDayTimeSpan: '10:00',
							RestTimeToPreviousDayTimeSpan: '11:00',
							ExpectedNightRestTimeSpan: '11:00'
						});
				}
				if (options.url == 'Preference/PeriodFeedback') {
					options.success([
						{
							Date: '2012-06-19',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						},
						{
							Date: '2012-06-20',
							DateInternal: '/Date(1514736000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						}
					]);
				}
			}
		};

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		$('#qunit-fixture')
			.html(
				"<div id='Preference-period-feedback-view' data-bind='text: PossibleResultContractTimeLower'>No data!</div>"
			)
			.append("<li data-mytime-date='2012-06-20' class='inperiod feedback' />")
			.append("<div class='warning-indicator'></div>");

		target.InitViewModels();

		equal($('#Preference-period-feedback-view').text(), '7:30', 'day sum after reinit same instance');
	});

	test('should show night rest violation message and indicate which day', function() {
		$('#qunit-fixture')
			.html("<div class='warning-indicator'></div>")
			.append(
				"<div id='Preference-period-feedback-view' data-bind='foreach:PossibleNightRestViolations()'><span data-bind='text:firstDay'></span></div>"
			)
			.append("<li data-mytime-week='week' />")
			.append(
				"<li data-mytime-date='2016-01-26' class='inperiod feedback' data-bind='text: NightRestViolationSwitch()' />"
			)
			.append(
				"<li data-mytime-date='2016-01-27' class='inperiod feedback' data-bind='text: NightRestViolationSwitch()' />"
			);

		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var date = '/Date(1453737600000)/'; //2016-01-26

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					options.success([{ Date: '2016-01-26', Feedback: true }, { Date: '2016-01-27', Feedback: true }]);
				}
				if (options.url == 'PreferenceFeedback/Feedback') {
					if (options.data.Date == '2016-01-26')
						options.success({
							HasNightRestViolationToPreviousDay: false,
							HasNightRestViolationToNextDay: true,
							DateInternal: date
						});
					if (options.data.Date == '2016-01-27')
						options.success({
							HasNightRestViolationToPreviousDay: true,
							HasNightRestViolationToNextDay: false,
							DateInternal: '/Date(1453824000000)/' //2016-01-27
						});
				}
				if (options.url == 'Preference/PeriodFeedback') {
					options.success([
						{
							Date: '2016-01-26',
							DateInternal: '/Date(1453737600000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: true,
							HasNightRestViolationToPreviousDay: false,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						},
						{
							Date: '2016-01-27',
							DateInternal: '/Date(1453824000000)/',
							ExpectedNightRestTimeSpan: '11:00',
							FeedbackError: null,
							HasNightRestViolationToNextDay: false,
							HasNightRestViolationToPreviousDay: true,
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							PossibleEndTimes: '15:30-23:00',
							PossibleStartTimes: '07:00-14:15',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						}
					]);
				}
			}
		};

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);
		target.InitViewModels();

		equal($('#Preference-period-feedback-view').text(), Teleopti.MyTimeWeb.Common.FormatDate(moment(date)));
		equal($('li[data-mytime-date="2016-01-26"]').text(), 'true');
		equal($('li[data-mytime-date="2016-01-27"]').text(), 'true');
	});

	test('should get feedback data by period', function() {
		$('#qunit-fixture')
			.html("<div class='warning-indicator'></div>")
			.append("<li data-mytime-week='week' />")
			.append(
				"<li data-mytime-date='2012-06-11' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />"
			)
			.append(
				"<li data-mytime-date='2012-06-12' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />"
			);

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					options.success([{ Date: '2012-06-11', Feedback: true }, { Date: '2012-06-12', Feedback: true }]);
				}
				if (options.url == 'Preference/PeriodFeedback') {
					options.success([
						{
							Date: '2012-06-11',
							DateInternal: '/Date(1339372800000)/',
							PossibleStartTimes: '06:00-14:30',
							PossibleEndTimes: '14:30-23:00',
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							FeedbackError: null,
							HasNightRestViolationToPreviousDay: false,
							HasNightRestViolationToNextDay: false,
							ExpectedNightRestTimeSpan: '11:00',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						}
					]);
				}
			}
		};

		expect(1);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax, {
			CurrentFixedDate: function() {
				return new Date('2012-06-11');
			}
		});

		target.InitViewModels();

		equal($('li[data-mytime-date="2012-06-11"]').text(), '7:30');
	});

	test('should load feedback data correctly', function() {
		var weeksHTML = [],
			datesHTML = [],
			preferenceDaysNum = 42,
			today = '2012-06-11',
			startDate = '2012-05-21',
			endDate = '2012-07-01',
			dateStr = '',
			periodDatesList = [];

		var weeksHTMLHeader =
			'\n<ul class="calendarview-week" onselectstart="return false;"> ' +
			'\n<li data-mytime-week="week" style="width: 55px;" data-bind="css: { \'week-view-non-editable\': !IsEditable() }" class="week-view-non-editable">' +
			'\n<div class="day-header"></div>' +
			'\n<div class="day-content"></div>' +
			'\n</div>' +
			'\n</li>' +
			'\n</ul>';

		for (var i = 0; i < preferenceDaysNum; i++) {
			dateStr = moment(startDate)
				.add('day', i)
				.format('YYYY-MM-DD');
			periodDatesList.push(dateStr);
			datesHTML.push(
				"<li data-mytime-date='" +
					dateStr +
					"' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />"
			);

			if ((i + 1) % 7 == 0) {
				weeksHTML.push($(weeksHTMLHeader).append(datesHTML.toString().replace(/,/g, '\n'))[0].outerHTML);
				datesHTML.length = 0;
			}
		}

		$('#qunit-fixture').html(
			"<div id='Preference-body-inner'>\n" + weeksHTML.toString().replace(/,/g, '\n') + '\n</div>'
		).append("<div class='warning-indicator'></div>");

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					var fakePreferenceAndSchedulesData = [];
					periodDatesList.forEach(function(d) {
						fakePreferenceAndSchedulesData.push({
							Date: d,
							Feedback: true
						});
					});
					options.success(fakePreferenceAndSchedulesData);
				}
				if (options.url == 'Preference/PeriodFeedback') {
					options.success([
						{
							Date: startDate,
							DateInternal: '/Date(1337558400000)/',
							PossibleStartTimes: '06:00-14:30',
							PossibleEndTimes: '14:30-23:00',
							PossibleContractTimeMinutesLower: '420',
							PossibleContractTimeMinutesUpper: '510',
							FeedbackError: null,
							HasNightRestViolationToPreviousDay: false,
							HasNightRestViolationToNextDay: false,
							ExpectedNightRestTimeSpan: '11:00',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						},
						{
							Date: today,
							DateInternal: '/Date(1339372800000)/',
							PossibleStartTimes: '06:00-14:30',
							PossibleEndTimes: '14:30-23:00',
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							FeedbackError: null,
							HasNightRestViolationToPreviousDay: false,
							HasNightRestViolationToNextDay: false,
							ExpectedNightRestTimeSpan: '11:00',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						},
						{
							Date: endDate,
							DateInternal: '/Date(1341100800000)/',
							PossibleStartTimes: '06:00-14:30',
							PossibleEndTimes: '14:30-23:00',
							PossibleContractTimeMinutesLower: '480',
							PossibleContractTimeMinutesUpper: '510',
							FeedbackError: null,
							HasNightRestViolationToPreviousDay: false,
							HasNightRestViolationToNextDay: false,
							ExpectedNightRestTimeSpan: '11:00',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						}
					]);
				}
			}
		};

		expect(3);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax, {
			CurrentFixedDate: function() {
				return new Date('2012-06-11');
			}
		});

		target.InitViewModels();

		equal($('li[data-mytime-date="' + startDate + '"]').text(), '7:00');
		equal($('li[data-mytime-date="' + today + '"]').text(), '7:30');
		equal($('li[data-mytime-date="' + endDate + '"]').text(), '8:00');
	});

	test('should calculate possible result weekly contract time (lower and higher) correctly', function() {
		var weeksHTML = [],
			datesHTML = [],
			preferenceDaysNum = 42,
			today = '2012-06-11',
			startDate = '2012-05-21',
			endDate = '2012-07-01',
			dateStr = '',
			periodDatesList = [];

		var weeksHTMLHeader =
			'\n<ul class="calendarview-week" onselectstart="return false;"> ' +
			'\n<li data-mytime-week="week" style="width: 55px;" data-bind="css: { \'week-view-non-editable\': !IsEditable() }" class="week-view-non-editable">' +
			'\n<div class="day-header"></div>' +
			'\n<div class="day-content"></div>' +
			'\n<div class="day-content">' +
			'\n<!-- ko if: IsWeeklyWorkTimeVisible -->' +
			'\n<div class="min-hours-per-week">' +
			'\n<span>&gt;</span> ' +
			'\n<span data-bind="text:PossibleResultWeeklyContractTimeLower"></span>' +
			'\n</div>' +
			'\n<div class="max-hours-per-week">' +
			'\n<span>&lt;</span>' +
			'\n<span data-bind="text:PossibleResultWeeklyContractTimeUpper"></span>' +
			'\n</div>' +
			'\n<!-- /ko -->' +
			'\n</div>' +
			'\n</div>' +
			'\n</li>' +
			'\n</ul>' +
			"\n<div class='warning-indicator'></div>";

		for (var i = 0; i < preferenceDaysNum; i++) {
			dateStr = moment(startDate)
				.add('day', i)
				.format('YYYY-MM-DD');
			periodDatesList.push(dateStr);
			datesHTML.push(
				"<li data-mytime-date='" +
					dateStr +
					"' class='inperiod feedback' data-mytime-editable='True' data-bind='text: PossibleContractTimeLower' />"
			);

			if ((i + 1) % 7 == 0) {
				weeksHTML.push($(weeksHTMLHeader).append(datesHTML.toString().replace(/,/g, '\n'))[0].outerHTML);
				datesHTML.length = 0;
			}
		}

		$('#qunit-fixture').html(
			"<div id='Preference-body-inner'>\n" + weeksHTML.toString().replace(/,/g, '\n') + '\n</div>'
		).append("<div class='warning-indicator'></div>");

		var ajax = {
			Ajax: function(options) {
				if (options.url == 'Preference/PreferencesAndSchedules') {
					var fakePreferenceAndSchedulesData = [];
					periodDatesList.forEach(function(d) {
						var feedback = true;
						var temp = {
							Date: d
						};

						if (moment(d).isBefore(moment(today))) {
							feedback = false;
							temp.PersonAssignment = {
								ContractTime: '9:00',
								ContractTimeMinutes: 540,
								ShiftCategory: 'Day',
								TimeSpan: '08:00 - 17:00'
							};
						}
						temp.Feedback = feedback;
						fakePreferenceAndSchedulesData.push(temp);
					});
					options.success(fakePreferenceAndSchedulesData);
				}
				if (options.url == 'Preference/PeriodFeedback') {
					var fakeData = [];
					periodDatesList.forEach(function(date, index) {
						var dateInterval = new Date(date).getTime();
						fakeData.push({
							Date: date,
							DateInternal: '/Date(' + dateInterval + ')/',
							PossibleStartTimes: '06:00-14:30',
							PossibleEndTimes: '14:30-23:00',
							PossibleContractTimeMinutesLower: '450',
							PossibleContractTimeMinutesUpper: '510',
							FeedbackError: null,
							HasNightRestViolationToPreviousDay: false,
							HasNightRestViolationToNextDay: false,
							ExpectedNightRestTimeSpan: '11:00',
							RestTimeToNextDayTimeSpan: '00:00',
							RestTimeToPreviousDayTimeSpan: '00:00'
						});
					});
					options.success(fakeData);
				}
			}
		};

		expect(15);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax, {
			CurrentFixedDate: function() {
				return new Date('2012-06-11');
			}
		});

		target.InitViewModels();

		equal($('li[data-mytime-date="' + startDate + '"]').text(), '');
		equal($('li[data-mytime-date="' + today + '"]').text(), '7:30');
		equal($('li[data-mytime-date="' + endDate + '"]').text(), '7:30');

		var assertPossibleResultMin1 = '63'; // 7 * 450 (ContractTimeMinutes) / 60
		var assertPossibleResultMax1 = '63'; // 7 * 450 (ContractTimeMinutes) / 60
		var assertPossibleResultMin2 = '52:30'; // 7 * 450 (PossibleContractTimeMinutesLower) / 60
		var assertPossibleResultMax2 = '59:30'; // 7 * 510 (PossibleContractTimeMinutesUpper) / 60

		$('li[data-mytime-week]').each(function(index, el) {
			switch (index) {
				case 0:
					equal(
						$(el)
							.find('.min-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMin1) > -1,
						true
					);
					equal(
						$(el)
							.find('.max-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMax1) > -1,
						true
					);
					break;
				case 1:
					equal(
						$(el)
							.find('.min-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMin1) > -1,
						true
					);
					equal(
						$(el)
							.find('.max-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMax1) > -1,
						true
					);
					break;
				case 2:
					equal(
						$(el)
							.find('.min-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMin1) > -1,
						true
					);
					equal(
						$(el)
							.find('.max-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMax1) > -1,
						true
					);
					break;
				case 3:
					equal(
						$(el)
							.find('.min-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMin2) > -1,
						true
					);
					equal(
						$(el)
							.find('.max-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMax2) > -1,
						true
					);
					break;
				case 4:
					equal(
						$(el)
							.find('.min-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMin2) > -1,
						true
					);
					equal(
						$(el)
							.find('.max-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMax2) > -1,
						true
					);
					break;
				case 5:
					equal(
						$(el)
							.find('.min-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMin2) > -1,
						true
					);
					equal(
						$(el)
							.find('.max-hours-per-week')
							.text()
							.indexOf(assertPossibleResultMax2) > -1,
						true
					);
					break;
			}
		});
	});
});
