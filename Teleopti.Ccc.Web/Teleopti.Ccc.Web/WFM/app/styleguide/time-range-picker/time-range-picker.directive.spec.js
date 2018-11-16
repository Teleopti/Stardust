'use strict';
describe('time-range-picker directive', function() {
	var elementCompileFn, elementCompileWithHoursLimitFn, $compile, scope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('tmh.dynamicLocale'));
	beforeEach(
		module(function(tmhDynamicLocaleProvider) {
			tmhDynamicLocaleProvider.localeLocationPattern(
				'/base/node_modules/angular-i18n/angular-locale_{{locale}}.js'
			);
		})
	);
	beforeEach(module('wfm.timerangepicker'));

	beforeEach(inject(function(_$compile_, _$rootScope_) {
		$compile = _$compile_;
		scope = _$rootScope_.$new();

		scope.timeRange = {
			startTime: moment({
				hour: 8,
				minute: 30
			}).toDate(),
			endTime: moment({
				hour: 17,
				minute: 30
			}).toDate()
		};

		elementCompileFn = function() {
			return $compile('<time-range-picker ng-model="timeRange"></time-range-picker>');
		};
	}));

	it('Directive compilation should work', function() {
		var element = elementCompileFn()(scope);
		scope.$apply();
		expect(element).toBeDefined();
	});

	it('Should show timepickers for start-time and end-time', function() {
		var element = elementCompileFn()(scope);
		scope.$apply();
		var timepickers = element[0].getElementsByClassName('wfm-timepicker-wrap');
		expect(timepickers.length).toEqual(2);
	});

	it('Should show error when start-time is greater than end-time', function() {
		scope.timeRange.startTime = moment({
			hour: 10,
			minute: 30
		}).toDate();
		scope.timeRange.endTime = moment({
			hour: 8,
			minute: 30
		}).toDate();

		var element = elementCompileFn()(scope);
		scope.$apply();

		expect(element[0].getElementsByClassName('error-msg-container').length).toEqual(1);
		expect(
			element[0].getElementsByClassName('error-msg-container')[0].getElementsByTagName('span')[0].innerText
		).toEqual('EndTimeMustBeGreaterOrEqualToStartTime');
	});

	it('Should show error when time range is larger than max-hours-range', function() {
		scope.timeRange.startTime = moment({
			hour: 2,
			minute: 30
		}).toDate();
		scope.timeRange.endTime = moment({
			hour: 10,
			minute: 30
		}).toDate();

		var element = $compile('<time-range-picker max-hours-range="3" ng-model="timeRange"></time-range-picker>')(
			scope
		);
		scope.$apply();

		expect(element[0].getElementsByClassName('error-msg-container').length).toEqual(1);
		expect(
			element[0].getElementsByClassName('error-msg-container')[0].getElementsByTagName('span')[0].innerText
		).toEqual('InvalidHoursRange');
	});

	it('Should not show error when end-time is on the next day', function() {
		scope.timeRange.startTime = moment({
			hour: 10,
			minute: 30
		}).toDate();
		scope.timeRange.endTime = moment({
			hour: 8,
			minute: 30
		})
			.add(1, 'day')
			.toDate();

		var element = elementCompileFn()(scope);
		scope.$apply();

		expect(element[0].getElementsByClassName('error-msg-container').length).toEqual(0);
	});

	it('Should set the next-day to true when start-time and the end-time are on different days', function() {
		scope.timeRange.startTime = moment({
			hour: 10,
			minute: 30
		}).toDate();
		scope.timeRange.endTime = moment({
			hour: 8,
			minute: 30
		})
			.add(1, 'day')
			.toDate();

		var element = elementCompileFn()(scope);
		scope.$apply();

		expect(element[0].getElementsByClassName('mdi-weather-night').length).toEqual(1);
	});

	it('Setting next day to true will change the end-time to different date value', function() {
		var element = elementCompileFn()(scope);

		scope.$apply();

		angular
			.element(element[0].getElementsByClassName('wfm-time-range-picker'))
			.scope()
			.vm.toggleNextDay();

		scope.$apply();
		expect(scope.timeRange.startTime.getDate()).not.toEqual(scope.timeRange.endTime.getDate());
	});

	it('Setting next day to false will change the end-time to same date value', function() {
		scope.timeRange.startTime = moment({
			hour: 5,
			minute: 30
		}).toDate();
		scope.timeRange.endTime = moment({
			hour: 8,
			minute: 30
		})
			.add(1, 'day')
			.toDate();

		var element = elementCompileFn()(scope);
		scope.$apply();

		angular
			.element(element[0].getElementsByClassName('wfm-time-range-picker'))
			.scope()
			.vm.toggleNextDay();

		scope.$apply();
		expect(scope.timeRange.startTime.getDate()).toEqual(scope.timeRange.endTime.getDate());
	});

	it('should be able to change locale', function(done) {
		inject([
			'$locale',
			'$timeout',
			'tmhDynamicLocale',
			function($locale, $timeout, tmhDynamicLocale) {
				tmhDynamicLocale.set('sv').then(function(locale) {
					expect($locale.id).toBe('sv');
					expect($locale['DATETIME_FORMATS']['shortTime']).toBe('HH:mm');
					done();
				});
				setTimeout($timeout.flush, 400);
			}
		]);
	});

	it('Should not show meridian in Swedish time-format', function(done) {
		inject(function($timeout, tmhDynamicLocale) {
			tmhDynamicLocale.set('sv').then(function(locale) {
				// use timeout to avoid crashing digest loop
				setTimeout(function() {
					var element = elementCompileFn()(scope);
					scope.$apply();
					var timepicker = angular.element(element[0].getElementsByClassName('wfm-time-range-picker'));
					expect(timepicker.scope().vm.showMeridian).toBeFalsy();
					done();
				}, 200);
			});
			setTimeout($timeout.flush, 500);
		});
	});

	it('Should show meridian in US time-format', function(done) {
		inject([
			'$locale',
			'$timeout',
			'tmhDynamicLocale',
			function($locale, $timeout, tmhDynamicLocale) {
				tmhDynamicLocale.set('en-us').then(function(locale) {
					setTimeout(function() {
						var element = elementCompileFn()(scope);
						scope.$apply();
						var timepicker = angular.element(element[0].getElementsByClassName('wfm-time-range-picker'));
						expect(timepicker.scope().vm.showMeridian).toBeTruthy();
						done();
					}, 200);
				});
				setTimeout($timeout.flush, 500);
			}
		]);
	});
});
