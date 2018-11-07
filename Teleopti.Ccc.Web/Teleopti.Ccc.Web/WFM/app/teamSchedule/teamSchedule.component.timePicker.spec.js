describe('<teams-time-picker>', function () {
	var scope, $compile;

	beforeEach(function () {
		module('wfm.templates');
		module("wfm.teamSchedule");
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return {
					CurrentUserInfo: function () {
						return {
							DefaultTimeZone: 'Europe/London',
							DateFormatLocale: 'en',
							FirstDayOfWeek: 0,
							DateTimeFormat: {
								ShowMeridian: true,
								ShortTimePattern: 'h:mm A',
								AMDesignator: 'AM',
								PMDesignator: 'PM'
							}
						};
					}
				};
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_) {
		scope = _$rootScope_.$new();
		$compile = _$compile_;
	}));

	it('should render correctly', function () {
		var element = setUp("2018-04-13", "Europe/Berlin", "2018-04-13 01:30");
		expect(!!element).toBe(true);
		expect(element[0].querySelector('.uib-time.hours input').value).toEqual("01");
		expect(element[0].querySelector('.uib-time.minutes input').value).toEqual("30");
		expect(scope.dateTime).toEqual("2018-04-13 01:30");
	});

	it('should show meridian', function () {
		var element = setUp("2018-04-13");
		expect(element[0].querySelectorAll('.uib-time.am-pm').length).toEqual(1);
	});

	it('should display time correctly', function () {
		var element = setUp("2018-04-13");
		var ctrl = element.isolateScope().$ctrl;

		ctrl.dateTimeObj = moment({ hour: 8, minute: 30 }).toDate();
		scope.$apply();

		expect(element[0].querySelector('.uib-time.hours input').value).toEqual("08");
		expect(element[0].querySelector('.uib-time.minutes input').value).toEqual("30");
	});

	it('should output correct time value', function () {
		var element = setUp("2018-04-13", "Europe/Berlin", "2018-04-13 01:30");
		var hoursEl = element[0].querySelector('.uib-time.hours input');
		hoursEl.value = 8;
		angular.element(hoursEl).triggerHandler('change');
		var mimuteEl = element[0].querySelector('.uib-time.minutes input')
		mimuteEl.value = 30;
		angular.element(mimuteEl).triggerHandler('change');
		scope.$apply();

		expect(scope.dateTime).toEqual("2018-04-13 08:30");
	});

	it('should set dateTime to null if time is null or incorrect', function () {
		var element = setUp("2018-04-13");

		var hoursEl = element[0].querySelector('.uib-time.hours input');
		hoursEl.value = null;
		angular.element(hoursEl).triggerHandler('change');
		var mimuteEl = element[0].querySelector('.uib-time.minutes input')
		mimuteEl.value = 30;
		angular.element(mimuteEl).triggerHandler('change');
		scope.$apply();

		expect(scope.dateTime).toBe(null);
	});

	it('should show error if dateTime is invalid in DST when init time picker', function () {
		var element = setUp("2018-03-25", 'Europe/Berlin', "2018-03-25 02:00");
		expect(angular.element(element[0]).hasClass('ng-invalid-dst')).toBe(true);
	});

	it('should show error if time is invalid in DST', function () {
		var element = setUp("2018-03-25", 'Europe/Berlin', "2018-03-25 01:00");

		var hoursEl = element[0].querySelector('.uib-time.hours input');
		hoursEl.value = 2;
		angular.element(hoursEl).triggerHandler('change');
		var mimuteEl = element[0].querySelector('.uib-time.minutes input')
		mimuteEl.value = 30;
		angular.element(mimuteEl).triggerHandler('change');
		scope.$apply();
		expect(angular.element(element[0]).hasClass('ng-invalid-dst')).toBe(true);
	});
	
	function setUp(date, timezone, dateTime) {
		scope.date = date;
		scope.timezone = timezone;
		scope.dateTime = dateTime;
		var element = $compile('<teams-time-picker date="date" ng-model="dateTime" timezone="timezone"></teams-time-picker>')(scope);
		scope.$apply();
		return element;
	}
});

describe('<teams-time-picker> in locale sv-SE', function () {
	var $templateCache, $compile, element, scope;
	beforeEach(function () {
		module('wfm.templates');
		module('wfm.teamSchedule');
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return {
					CurrentUserInfo: function () {
						return {
							DefaultTimeZone: 'Europe/Berlin',
							DateFormatLocale: 'sv-se',
							FirstDayOfWeek: 0,
							DateTimeFormat: {
								ShowMeridian: false,
								ShortTimePattern: 'HH:mm',
								AMDesignator: 'fm',
								PMDesignator: 'em'
							}
						};
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

	it('should not show meridian', function () {
		element = setUp("2018-04-13");
		expect(element[0].querySelectorAll('.uib-time.am-pm.ng-hide').length).toEqual(1);
	});

	function setUp(date, timezone) {
		scope.timezone = timezone;
		var element = $compile('<teams-time-picker ng-model="time" timezone="timezone" date="date"></teams-time-picker>')(scope);
		scope.$apply();
		return element;
	}
});

describe('<teams-time-picker> in locale zh-CN', function () {
	var $templateCache, $compile, element, scope;
	beforeEach(function () {
		module('wfm.templates');
		module('wfm.teamSchedule');
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return {
					CurrentUserInfo: function () {
						return {
							DefaultTimeZone: 'Asia/Hong_Kong',
							DateFormatLocale: 'zh-CN',
							FirstDayOfWeek: 1,
							DateTimeFormat: {
								ShowMeridian: false,
								ShortTimePattern: 'HH:mm',
								AMDesignator: '上午',
								PMDesignator: '下午'
							}
							
						};
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

	it('should not show meridian', function () {
		element = setUp("2018-04-13");
		expect(element[0].querySelectorAll('.uib-time.am-pm.ng-hide').length).toEqual(1);
	});

	function setUp(date, timezone) {
		scope.timezone = timezone;
		var element = $compile('<teams-time-picker ng-model="time" timezone="timezone" date="date"></teams-time-picker>')(scope);
		scope.$apply();
		return element;
	}
});
