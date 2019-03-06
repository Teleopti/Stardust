describe('<underlying-info>', function () {
	'use strict';
	var $rootScope,
		$compile

	beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
	beforeEach(inject(function (_$rootScope_, _$compile_) {
		$rootScope = _$rootScope_;
		$compile = _$compile_;
	}));

	it('should render underlying info correctly', function () {
		var scheduleDate = '2018-05-16';
		var underlyingSchedulesSummary = {
			"PersonalActivities": [{
				"Description": "personal activity",
				"TimeSpan": '10:00 - 11:00'
			}],
			"PersonPartTimeAbsences": [{
				"Description": "holiday",
				"TimeSpan": '11:30 - 12:00'
			}],
			"PersonMeetings": [{
				"Description": "administration",
				"TimeSpan": '14:00 - 15:00'
			}]
		};
		var panel = setUp(scheduleDate, {
			UnderlyingScheduleSummary: underlyingSchedulesSummary,
		});

		var element = panel[0].querySelector(".underlying-info");
		var divEls = element.querySelectorAll('div');
		expect(divEls[1].innerText.trim()).toEqual("personal activity: 10:00 - 11:00");
		expect(divEls[2].innerText.trim()).toEqual("holiday: 11:30 - 12:00");
		expect(divEls[3].innerText.trim()).toEqual("administration: 14:00 - 15:00");
	});

	it('should not show personal activity label when underlying activity not include personal acitivity', function () {
		var scheduleDate = '2018-05-16';
		var underlyingSchedulesSummary = {
			"PersonalActivities": [],
			"PersonPartTimeAbsences": [{
				"Description": "holiday",
				"TimeSpan": '11:30 - 12:00'
			}],
			"PersonMeetings": []
		};
		var panel = setUp(scheduleDate, {
			UnderlyingScheduleSummary: underlyingSchedulesSummary,
		});

		var element = panel[0].querySelector(".underlying-info");

		var divEls = element.querySelectorAll('div');
		expect(divEls.length).toEqual(1);
		expect(divEls[0].innerText.trim()).toEqual("holiday: 11:30 - 12:00");
	});


	function setUp(scheduleDate, personSchedule) {

		var scope = $rootScope.$new();
		scope.personSchedule = personSchedule || {};
		scope.scheduleDate = scheduleDate;
		var html = '<underlying-schedule-info date="scheduleDate" person-schedule="personSchedule"></underlying-schedule-info>';
		var element = $compile(html)(scope);
		scope.$apply();
		return element;
	}
});