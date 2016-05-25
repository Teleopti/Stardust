﻿describe('teamschedule add absence diretive test', function() {
 
	var	fakeAbsenceService,
		fakeScheduleManagementSvc;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function() {
		fakeAbsenceService = new FakePersonAbsence();
		fakeScheduleManagementSvc = new FakeScheduleManagementService();

		module(function($provide) {
			$provide.service('PersonAbsence', function() {
				return fakeAbsenceService;
			});
			$provide.service('ScheduleManagement', function() {
				return fakeScheduleManagementSvc;
			});

		});
	});


	beforeEach(inject(function(_$rootScope_, _$compile_, _$httpBackend_, _WFMDate_, _PersonSelection_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		WFMDate = _WFMDate_;

		PersonSelection = _PersonSelection_;
		PersonSelection.clearPersonInfo()
		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));

	it('add-absence should render correctly', function() {
		var result = setUp();
		expect(result.commandControl).not.toBeNull();
	});

	it('should handle default start and end time attribute', function () {
		fakeScheduleManagementSvc.setEarliestStartTime(new Date('2015-01-01 10:00:00'));
		var result = setUp(new Date('2015-01-01 10:00:00'), { permissions: { IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: true } });		
		var startDateString = result.container[0].querySelectorAll('team-schedule-datepicker input')[0].value,

			endDateString = result.container[0].querySelectorAll('team-schedule-datepicker input')[1].value,

			startTimeString = result.container[0].querySelectorAll('.uib-timepicker input')[0].value
				+ result.container[0].querySelectorAll('.uib-timepicker input')[1].value,

			endTimeString = result.container[0].querySelectorAll('.uib-timepicker input')[2].value
				+ result.container[0].querySelectorAll('.uib-timepicker input')[3].value;

		expect(startDateString).toBe('1/1/15');
		expect(startTimeString).toBe('1000');
		expect(endDateString).toBe('1/1/15');
		expect(endTimeString).toBe('1100');
	});

	it('should display full day absence check box with only full day absence permission', function() {
		var result = setUp(new Date('2015-01-01 10:00:00'), { permissions: { IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true } });
		var checkBoxInput = result.container[0].querySelectorAll('.wfm-checkbox input#is-full-day');
		expect(checkBoxInput[0].value).toBe('on');
		expect(checkBoxInput[0].disabled).toBe(true);
	});

	it('should not display full day absence check box with only intraday absence permission', function () {
		var result = setUp(new Date('2015-01-01 10:00:00'), { permissions: { IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false } });		
		var checkBoxInput = result.container[0].querySelectorAll('.wfm-checkbox input#is-full-day');
		expect(checkBoxInput.length).toBe(0);
	});

	function setUp(inputDate, inputConfigurations) {
		var date, configurations;
		var html = '<teamschedule-command-container date="curDate" configurations="configurations"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;

		if (inputConfigurations == null) {
			configurations = {};
		} else {
			configurations = inputConfigurations;
		}

		scope.curDate = date;
		scope.configurations = configurations;
		fakeAbsenceService.setAvailableAbsenceTypes(getAvailableAbsenceTypes());

		var container = $compile(html)(scope);
		scope.$apply();

		container.isolateScope().vm.setActiveCmd('AddAbsence');
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector(".add-absence")).scope().vm;

		var obj = {
			container: container,
			commandControl: commandControl,
			scope: scope
		};

		return obj;
	}	

	function getAvailableAbsenceTypes() {
		return [
			{
				"Id": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
				"Name": "Sick"
			},
			{
				"Id": "5c1409de-a0f1-4cd4-b383-9b5e015ab3c6",
				"Name": "Holiday"
			}
		];
	}


	function FakeScheduleManagementService() {
		var latestEndTime = null;
		var latestStartTime = null;
		var earliestStartTime = null;

		this.setLatestEndTime = function(date) {
			latestEndTime = date;
		}

		this.setLatestStartTime = function(date) {
			latestStartTime = date;
		}

		this.setEarliestStartTime = function(date) {
			earliestStartTime = date;
		}

		this.getLatestPreviousDayOvernightShiftEnd = function() {
			return latestEndTime;
		}

		this.getLatestStartOfSelectedSchedule = function() {
			return latestStartTime;
		}

		this.getEarliestStartOfSelectedSchedule = function() {
			return earliestStartTime;
		}
	}

	function FakePersonAbsence() {
		var availableAbsenceTypes = [];
		var targetAbsence = null;
		var fakeResponse = { data: [] };

		this.loadAbsences = function() {
			return {
				then: function(cb) {
					cb(availableAbsenceTypes);
				}
			};
		};

		this.addFullDayAbsence = function(input) {
			targetAbsence = input;
			return {
				then: (function(cb) {
					cb(fakeResponse);
				})
			};
		};

		this.addIntradayAbsence = function(input) {
			targetAbsence = input;
			return {
				then: (function(cb) {
					cb(fakeResponse);
				})
			};
		};

		this.getAddAbsenceCalledWith = function() {
			return targetAbsence;
		};

		this.setAvailableAbsenceTypes = function(absences) {
			availableAbsenceTypes = absences;
		};
	}

});