'use strict';
describe('activity-time-range-picker directive', function () {
	var elementCompileFn, $templateCache, $compile, element,
        scope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(inject(function (_$compile_, _$rootScope_, _$templateCache_) {
		$templateCache = _$templateCache_;
		$compile = _$compile_;
		scope = _$rootScope_.$new();
		var startTime = moment({ hour: 8, minute: 30 }).toDate();
		var endTime = moment({ hour: 17, minute: 30 }).toDate();
		moment.locale('sv');
		scope.timeRange = {
			startTime: startTime,
			endTime: endTime
		};
		scope.isNextDay = false;
		var date = new Date("2016-04-08");
		scope.date = function() {
			return date;
		};

		elementCompileFn = function () {
			return $compile('<activity-time-range-picker ng-model="timeRange" xdisable-next-day="disableNextDay" is-next-day="isNextDay" reference-day="date"></activity-time-range-picker>');
		};
	}));

	it('Directive compilation should work', function () {
		var element = elementCompileFn()(scope);
		scope.$apply();
		expect(element).toBeDefined();
	});

	describe('custom template', function () {
		it('should allow custom templates', function () {
			$templateCache.put('foo/bar.html', '<div class="custom-template">baz</div>');
			element = $compile('<activity-time-range-picker ng-model="timeRange" template-url="foo/bar.html"></activity-time-range-picker>')(scope);
			scope.$apply();
			expect(element.children().hasClass('custom-template')).toBeTruthy();
			expect(element.children().html()).toBe('baz');
		});
	});

	it('Should show timepickers for start-time and end-time', function () {
		var element = elementCompileFn()(scope);
		scope.$apply();
		var timepickers = element.find('tmp-timepicker-wrap');		
		expect(timepickers.length).toEqual(2);
	});


	it('Should show error when time is invalid', function () {
		var element = elementCompileFn()(scope);
		scope.$apply();
		var divs = element.children();
		var validityDiv = angular.element(divs[0]);

		validityDiv.scope().startTime = "";
		validityDiv.scope().endTime = moment({ hour: 8, minute: 30 }).toDate();
		scope.$apply();

		expect(element.hasClass('ng-invalid')).toBeTruthy();
	});

	it('Should set next day to false and disable the next day switch when start-time is greater than end-time', function () {
		var element = elementCompileFn()(scope);
		scope.$apply();
		var divs = element.children();
		var validityDiv = angular.element(divs[0]);

		expect(validityDiv.scope().disableNextDay).toBeFalsy();

		validityDiv.scope().isNextDay = true;
		validityDiv.scope().startTime = moment({ hour: 10, minute: 30 }).toDate();
		validityDiv.scope().endTime = moment({ hour: 8, minute: 30 }).toDate();
		scope.$apply();

		expect(validityDiv.scope().isNextDay).toBeFalsy();
		expect(validityDiv.scope().disableNextDay).toBeTruthy();
		expect(scope.timeRange.startTime).toEqual(new Date("2016-04-08 10:30"));
		expect(scope.timeRange.endTime).toEqual(new Date("2016-04-09 08:30"));

	});

	it('Should set date to next day when next switch is true and start is smaller than end', function () {
		var element = elementCompileFn()(scope);
		scope.$apply();
		
		var divs = element.children();
		var validityDiv = angular.element(divs[0]);

		validityDiv.scope().isNextDay = true;
		validityDiv.scope().startTime = moment({ hour: 10, minute: 30 }).toDate();
		validityDiv.scope().endTime = moment({ hour: 11, minute: 30 }).toDate();
		
		scope.$apply();

		expect(scope.timeRange.startTime).toEqual(new Date("2016-04-09 10:30"));
		expect(scope.timeRange.endTime).toEqual(new Date("2016-04-09 11:30"));

	});

	it('Should set date to reference day when next switch is false and start is smaller than end', function () {
		var element = elementCompileFn()(scope);
		scope.$apply();
		
		var divs = element.children();
		var validityDiv = angular.element(divs[0]);

		validityDiv.scope().isNextDay = false;
		validityDiv.scope().startTime = moment({ hour: 10, minute: 30 }).toDate();
		validityDiv.scope().endTime = moment({ hour: 11, minute: 30 }).toDate();
		
		scope.$apply();

		expect(scope.timeRange.startTime).toEqual(new Date("2016-04-08 10:30"));
		expect(scope.timeRange.endTime).toEqual(new Date("2016-04-08 11:30"));

	});


});


describe('custom locale sv', function () {
	var elementCompileFn, $templateCache, $compile, element,
	scope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$locale', function () {
				return {
					id: 'sv-se',
					DATETIME_FORMATS: {
						AMPMS: ['fm', 'em'],
						shortTime: 'HH:mm'
					}
				};
			});
		});
	});

	beforeEach(inject(function (_$compile_, _$rootScope_, _$templateCache_) {
		$templateCache = _$templateCache_;
		$compile = _$compile_;
		scope = _$rootScope_.$new();
		var startTime = moment({ hour: 8, minute: 30 }).toDate();
		var endTime = moment({ hour: 17, minute: 30 }).toDate();
		scope.timeRange = {
			startTime: startTime,
			endTime: endTime
		};
		scope.isNextDay = false;
		var date = new Date("2016-04-08");
		scope.date = function () {
			return date;
		};

		elementCompileFn = function () {
			return $compile('<activity-time-range-picker ng-model="timeRange" is-next-day="isNextDay" reference-day="date"></activity-time-range-picker>');
		};
	}));

	it('Should not show meridian in Swedish time-format', function () {
		element = elementCompileFn()(scope);
		scope.$apply();
		var timepicker = angular.element(element.find('tmp-timepicker-wrap')[0]);
		expect(timepicker.scope().showMeridian).toBeFalsy();

	});
});

describe('custom locale en', function () {
	var elementCompileFn, $templateCache, $compile, element,
	scope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$locale', function () {
				return {
					id: 'en-us',
					DATETIME_FORMATS: {
						AMPMS: ['AM', 'PM'],
						shortTime: 'h:mm a'
					}
				};
			});
		});
	});

	beforeEach(inject(function (_$compile_, _$rootScope_, _$templateCache_) {
		$templateCache = _$templateCache_;
		$compile = _$compile_;
		scope = _$rootScope_.$new();
		var startTime = moment({ hour: 8, minute: 30 }).toDate();
		var endTime = moment({ hour: 17, minute: 30 }).toDate();
		scope.timeRange = {
			startTime: startTime,
			endTime: endTime
		};
		scope.isNextDay = false;
		var date = new Date("2016-04-08");
		scope.date = function () {
			return date;
		};

		elementCompileFn = function () {
			return $compile('<activity-time-range-picker ng-model="timeRange" xdisable-next-day="disableNextDay" is-next-day="isNextDay" reference-day="date"></activity-time-range-picker>');
		};
	}));

	it('Should show meridian in US time-format', function () {
		element = elementCompileFn()(scope);
		scope.$apply();
		var timepicker = angular.element(element.find('tmp-timepicker-wrap')[0]);
		expect(timepicker.scope().showMeridian).toBeTruthy();

	});
});
