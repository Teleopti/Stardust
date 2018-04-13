'use strict';
describe('teamschedule activity-time-range-picker directive tests', function () {
	var $templateCache, $compile, element, scope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(inject(function (_$compile_, _$rootScope_, _$templateCache_) {
		$templateCache = _$templateCache_;
		$compile = _$compile_;
		scope = _$rootScope_.$new();
		moment.locale('sv');
	}));

	function setUp(timezone, selectedDate) {
		var startTime = moment({ hour: 8, minute: 30 }).toDate();
		var endTime = moment({ hour: 17, minute: 30 }).toDate();
		scope.timeRange = {
			startTime: startTime,
			endTime: endTime
		};
		scope.isNextDay = false;
		scope.timeZone = !!timezone ? timezone : "";
		scope.date = function () {
			return selectedDate || "2016-04-08";
		};

		var el = $compile('<activity-time-range-picker ng-model="timeRange" timezone="timeZone" xdisable-next-day="disableNextDay" is-next-day="isNextDay" reference-day="date"></activity-time-range-picker>')(scope);

		scope.$apply();
		return el;
	}

	it('Directive compilation should work', function () {
		var element = setUp();
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
		var element = setUp();
		var timepickers = element[0].querySelectorAll('.wfm-timepicker-wrap');
		expect(timepickers.length).toEqual(2);
	});


	it('Should show error when time is invalid', function () {
		var element = setUp();
		var isolateScope = element.isolateScope();

		isolateScope.startTime = "";
		isolateScope.endTime = moment({ hour: 8, minute: 30 }).toDate();
		scope.$apply();

		expect(element.hasClass('ng-invalid')).toBeTruthy();
	});

	it('Should show error when time is invalid because of DST', function () {
		var element = setUp('Europe/Berlin', '2018-03-25');

		var isolateScope = element.isolateScope();
		isolateScope.startTime = moment({ hour: 2, minute: 30 }).toDate();
		isolateScope.endTime = moment({ hour: 2, minute: 30 }).toDate();
		scope.$apply();

		expect(element.hasClass('ng-invalid')).toBeTruthy();
	});

	it('Should set next day to false and disable the next day switch when start-time is greater than end-time', function () {
		var element = setUp();
		var isolateScope = element.isolateScope();
		expect(isolateScope.disableNextDay).toBeFalsy();

		isolateScope.isNextDay = true;
		isolateScope.startTime = moment({ hour: 10, minute: 30 }).toDate();
		isolateScope.endTime = moment({ hour: 8, minute: 30 }).toDate();

		scope.$apply();

		expect(isolateScope.isNextDay).toBeFalsy();
		expect(isolateScope.disableNextDay).toBeTruthy();
		expect(scope.timeRange.startTime).toEqual("2016-04-08 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-09 08:30");

	});

	it('Should set next day to false and disable the next day switch when start-time equals end-time', function () {
		var element = setUp();
		var isolateScope = element.isolateScope();

		expect(isolateScope.disableNextDay).toBeFalsy();

		isolateScope.isNextDay = true;
		isolateScope.startTime = moment({ hour: 10, minute: 30 }).toDate();
		isolateScope.endTime = moment({ hour: 10, minute: 30 }).toDate();
		scope.$apply();

		expect(isolateScope.isNextDay).toBeFalsy();
		expect(isolateScope.disableNextDay).toBeTruthy();
		expect(scope.timeRange.startTime).toEqual("2016-04-08 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-09 10:30");
	});

	it('Should set date to next day when next switch is true and start is smaller than end', function () {
		var element = setUp();
		var isolateScope = element.isolateScope();

		isolateScope.isNextDay = true;
		isolateScope.startTime = moment({ hour: 10, minute: 30 }).toDate();
		isolateScope.endTime = moment({ hour: 11, minute: 30 }).toDate();

		scope.$apply();

		expect(scope.timeRange.startTime).toEqual("2016-04-09 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-09 11:30");

	});

	it('Should set date to reference day when next switch is false and start is smaller than end', function () {
		var element = setUp();
		var isolateScope = element.isolateScope();

		isolateScope.isNextDay = false;
		isolateScope.startTime = moment({ hour: 10, minute: 30 }).toDate();
		isolateScope.endTime = moment({ hour: 11, minute: 30 }).toDate();
		scope.$apply();

		expect(scope.timeRange.startTime).toEqual("2016-04-08 10:30");
		expect(scope.timeRange.endTime).toEqual("2016-04-08 11:30");

	});

});

describe('custom locale sv', function () {
	var $templateCache, $compile, element, scope;

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
	}));

	it('Should not show meridian in Swedish time-format', function () {
		element = setUp();
		var timepicker = angular.element(element[0].querySelector('.wfm-timepicker-wrap'));
		expect(timepicker.scope().showMeridian).toBeFalsy();

	});

	function setUp() {
		var date = new Date("2016-04-08");
		var startTime = moment({ hour: 8, minute: 30 }).toDate();
		var endTime = moment({ hour: 17, minute: 30 }).toDate();
		scope.timeRange = {
			startTime: startTime,
			endTime: endTime
		};
		scope.isNextDay = false;

		scope.date = function () {
			return date;
		};
		var el = $compile('<activity-time-range-picker ng-model="timeRange" is-next-day="isNextDay" reference-day="date"></activity-time-range-picker>')(scope);
		scope.$apply();

		return el;
	}
});

describe('custom locale en', function () {
	var $templateCache, $compile, element, scope;

	beforeEach(module('wfm.templates'));
	describe('custom locale sv', function () {
		var $templateCache, $compile, element, scope;

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
		}));

		it('Should not show meridian in Swedish time-format', function () {
			element = setUp();
			var timepicker = angular.element(element[0].querySelector('.wfm-timepicker-wrap'));
			expect(timepicker.scope().showMeridian).toBeFalsy();
		});

		function setUp() {
			var date = new Date("2016-04-08");
			var startTime = moment({ hour: 8, minute: 30 }).toDate();
			var endTime = moment({ hour: 17, minute: 30 }).toDate();
			scope.timeRange = {
				startTime: startTime,
				endTime: endTime
			};
			scope.isNextDay = false;
			scope.date = function () {
				return date;
			};
			var el = $compile('<activity-time-range-picker ng-model="timeRange" is-next-day="isNextDay" reference-day="date"></activity-time-range-picker>')(scope);
			scope.$apply();

			return el;
		}
	});
});
describe('custom locale en', function () {
	var $templateCache, $compile, element, scope;

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
	}));

	function setUp() {
		var date = new Date("2016-04-08");
		var startTime = moment({ hour: 8, minute: 30 }).toDate();
		var endTime = moment({ hour: 17, minute: 30 }).toDate();

		scope.date = function () {
			return date;
		};
		scope.timeRange = {
			startTime: startTime,
			endTime: endTime
		};
		scope.isNextDay = false;
		var el = $compile('<activity-time-range-picker ng-model="timeRange" xdisable-next-day="disableNextDay" is-next-day="isNextDay" reference-day="date"></activity-time-range-picker>')(scope);
		scope.$apply();

		return el;
	}

	it('Should show meridian in US time-format', function () {
		element = setUp();
		var timepicker = angular.element(element[0].querySelector('.wfm-timepicker-wrap'));
		expect(timepicker.scope().showMeridian).toBeTruthy();
	});
});
