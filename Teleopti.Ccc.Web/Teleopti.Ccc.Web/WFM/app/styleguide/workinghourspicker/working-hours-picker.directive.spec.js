'use strict';
describe('working hours picker directive', function() {
	var $compile, scope;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.workingHoursPicker'));
	beforeEach(module('tmh.dynamicLocale'));
	beforeEach(
		module(function(tmhDynamicLocaleProvider) {
			tmhDynamicLocaleProvider.localeLocationPattern(
				'/base/node_modules/angular-i18n/angular-locale_{{locale}}.js'
			);
		})
	);

	beforeEach(inject(function(_$compile_, _$rootScope_) {
		$compile = _$compile_;
		scope = _$rootScope_.$new();

		scope.openHours = [];
	}));

	it('should render working hours picker', function() {
		var element = $compile(
			'<working-hours-picker working-hours="openHours" enabled="true"></working-hours-picker>'
		)(scope);

		scope.$apply();

		expect(element).toBeTruthy();
		expect(element[0].getElementsByClassName('working-hours-picker-container').length, 1);
	});

	it("should not push the working hour if the newWorkingPeriod's end time is earlier than its start time", function() {
		var element = $compile(
			'<working-hours-picker working-hours="openHours" enabled="true"></working-hours-picker>'
		)(scope);
		scope.$apply();

		var workingHoursScope = angular
			.element(element[0].getElementsByClassName('working-hours-picker-container'))
			.scope().$parent;

		workingHoursScope.newWorkingPeriod = {
			startTime: new Date(2016, 0, 1, 8),
			endTime: new Date(2016, 0, 1, 9)
		};
		angular.element(element[0].getElementsByClassName('add-working-hours-button')[0]).triggerHandler('click');

		scope.$apply();

		workingHoursScope.newWorkingPeriod = {
			startTime: new Date(2016, 0, 2, 8),
			endTime: new Date(2016, 0, 2, 7)
		};
		angular.element(element[0].getElementsByClassName('add-working-hours-button')[0]).triggerHandler('click');

		scope.$apply();

		expect(workingHoursScope.workingHours.length).toBe(1);
		expect(workingHoursScope.workingHours[0].StartTime.getDate()).toBe(1);
		expect(workingHoursScope.workingHours[0].StartTime.getHours()).toBe(8);
		expect(workingHoursScope.workingHours[0].EndTime.getDate()).toBe(1);
		expect(workingHoursScope.workingHours[0].EndTime.getHours()).toBe(9);
	});
});
