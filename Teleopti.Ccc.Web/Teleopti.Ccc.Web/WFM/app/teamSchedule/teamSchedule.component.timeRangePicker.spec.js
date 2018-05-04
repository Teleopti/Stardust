'use strict';
describe('<teams-time-range-picker>', function () {
	var $templateCache, $compile, element, scope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(inject(function (_$compile_, _$rootScope_, _$templateCache_) {
		$templateCache = _$templateCache_;
		$compile = _$compile_;
		scope = _$rootScope_.$new();
		moment.locale('sv');
	}));

	function setUp(date, timeRange) {
		scope.date = date || "2016-04-08";

		scope.timeRange = timeRange || {
			startTime: scope.date + ' 08:30',
			endTime: scope.date + ' 09:30',
		};
		scope.timezone = 'Europe/Berlin';
		var el = $compile('<teams-time-range-picker ng-model="timeRange" timezone="timezone" date="date" />')(scope);
		scope.$apply();
		return el;
	}

	it('directive compilation should work', function () {
		var element = setUp();
		expect(element).toBeDefined();
	});

	it('should show timepickers for start-time and end-time', function () {
		var element = setUp();
		var timepickers = element[0].querySelectorAll('teams-time-picker');
		expect(timepickers.length).toEqual(2);
	});

	it('should set next day to false and disable the next day switch when start-time is greater than end-time', function () {
		var element = setUp();

		var ctrl = element.isolateScope().$ctrl;

		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		hoursEls[0].value = 10;
		angular.element(hoursEls[0]).triggerHandler('change');
		hoursEls[1].value = 8;
		angular.element(hoursEls[1]).triggerHandler('change');

		
		expect(!!element[0].querySelectorAll('md-switch.md-checked').length).toBeFalsy();
		expect(!!element[0].querySelector('md-switch').disabled).toBeTruthy();

		expect(scope.timeRange.startTime).toEqual("2016-04-08 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-09 08:30");

	});

	it('should set next day to false and disable the next day switch when start-time equals end-time', function () {
		var element = setUp();
		var ctrl = element.isolateScope().$ctrl;

		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		hoursEls[0].value = 10;
		angular.element(hoursEls[0]).triggerHandler('change');
		hoursEls[1].value = 10;
		angular.element(hoursEls[1]).triggerHandler('change');

		expect(!!element[0].querySelectorAll('md-switch.md-checked').length).toBeFalsy();
		expect(!!element[0].querySelector('md-switch').disabled).toBeTruthy();

		expect(scope.timeRange.startTime).toEqual("2016-04-08 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-09 10:30");
	});

	it('should set date to next day when next switch is true and start is smaller than end', function () {
		var element = setUp();
		var ctrl = element.isolateScope().$ctrl;
	
		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		hoursEls[0].value = 10;
		angular.element(hoursEls[0]).triggerHandler('change');
		hoursEls[1].value = 11;
		angular.element(hoursEls[1]).triggerHandler('change');

		angular.element(element[0].querySelector('md-switch')).triggerHandler('click');
		angular.element(element[0].querySelector('md-switch')).triggerHandler('change');

		expect(scope.timeRange.startTime).toEqual("2016-04-09 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-09 11:30");
	});

	it('should set date to reference day when next switch is false and start is smaller than end', function () {
		var element = setUp();
		var isolateScope = element.isolateScope();

		isolateScope.isNextDay = false;

		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		hoursEls[0].value = 10;
		angular.element(hoursEls[0]).triggerHandler('change');
		hoursEls[1].value = 11;
		angular.element(hoursEls[1]).triggerHandler('change');
		scope.$apply();

		expect(scope.timeRange.startTime).toEqual("2016-04-08 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-08 11:30");

	});

	it('should init correct status when start-time is greater than end-time', function () {
		var timeRange = {
			startTime: "2018-04-20 23:00",
			endTime: "2018-04-21 01:00"
		};
		var element = setUp("2018-04-20", timeRange);

		var isolateScope = element.isolateScope();
		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		var minutesEls = element[0].querySelectorAll('teams-time-picker .minutes input');

		expect(hoursEls[0].value).toBe("11");
		expect(hoursEls[1].value).toBe("01");

		expect(minutesEls[0].value).toBe("00");
		expect(minutesEls[1].value).toBe("00");

		expect(!!element[0].querySelectorAll('md-switch.md-checked').length).toBeFalsy();
		expect(!!element[0].querySelector('md-switch').disabled).toBeTruthy();

	});

	it('should init correct status when start time and end time are next day', function () {
		var timeRange = {
			startTime: "2018-04-21 08:30",
			endTime: "2018-04-21 09:15"
		};
		var element = setUp("2018-04-20", timeRange);

		var isolateScope = element.isolateScope();
		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		var minutesEls = element[0].querySelectorAll('teams-time-picker .minutes input');

		expect(hoursEls[0].value).toBe("08");
		expect(hoursEls[1].value).toBe("09");

		expect(minutesEls[0].value).toBe("30");
		expect(minutesEls[1].value).toBe("15");

		expect(!!element[0].querySelectorAll('md-switch.md-checked').length).toBeTruthy();
		expect(!!element[0].querySelector('md-switch').disabled).toBeFalsy();

	});

	it('should set end time invalid when the start-time greater than end-time and the end time is on DST changing date and the time value is invalid', function () {
		var timeRange = {
			startTime: "2018-03-24 08:00",
			endTime: "2018-03-24 09:00"
		};
		var element = setUp("2018-03-24", timeRange);

		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		hoursEls[1].value = 2;
		angular.element(hoursEls[1]).triggerHandler('change');
		scope.$apply();

		expect(scope.timeRange.endTime).toEqual("2018-03-25 02:00");
		expect(angular.element(element[0].querySelectorAll('teams-time-picker')[1]).hasClass('ng-invalid-dst')).toBeTruthy();
	});

	it('should set end-time invalid and keep the origin value when the end time is on DST and the time value is invalid', function () {
		var timeRange = {
			startTime: "2018-03-25 08:00",
			endTime: "2018-03-25 09:00"
		};
		var element = setUp("2018-03-25", timeRange);

		var hoursEls = element[0].querySelectorAll('teams-time-picker .hours input');
		hoursEls[0].value = 1;
		angular.element(hoursEls[0]).triggerHandler('change');
		hoursEls[1].value = 2;
		angular.element(hoursEls[1]).triggerHandler('change');
		scope.$apply();

		expect(scope.timeRange.startTime).toEqual("2018-03-25 01:00");
		expect(scope.timeRange.endTime).toEqual("2018-03-25 02:00");
		expect(angular.element(element[0].querySelectorAll('teams-time-picker')[1]).hasClass('ng-invalid-dst')).toBeTruthy();
	});
});

