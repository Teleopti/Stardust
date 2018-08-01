﻿describe('teamschedule add absence diretive test', function () {
	'use strict';

	var fakeAbsenceService,
		fakeScheduleManagementSvc,
		fakePermissions,
		$compile,
		$rootScope,
		$timeout,
		fakePersonSelectionService,
		fakeToggle = {};

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		fakeAbsenceService = new FakePersonAbsence();
		fakeScheduleManagementSvc = new FakeScheduleManagementService();
		fakePermissions = new FakePermissions();
		fakePersonSelectionService = new FakePersonSelectionService();

		module(function ($provide) {
			$provide.service('PersonAbsence', function () {
				return fakeAbsenceService;
			});
			$provide.service('ScheduleManagement', function () {
				return fakeScheduleManagementSvc;
			});
			$provide.service('teamsPermissions', function () {
				return fakePermissions;
			});
			$provide.service('PersonSelection', function () {
				return fakePersonSelectionService;
			});
			$provide.service('CommandCheckService', function () {
				return new FakeCommandCheckService();
			});
			$provide.service('CurrentUserInfo', function () {
				return new FakeCurrentUserInfo();
			});
			$provide.service('Toggle', function () {
				return fakeToggle;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_, _$timeout_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		_$httpBackend_.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
		$timeout = _$timeout_;
	}));

	it('should display full day absence check box with only full day absence permission', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true });
		var result = setUp('2018-08-01');
		var checkBoxInput = result.container[0].querySelectorAll('.fullDayAbsenceCheckBox');
		expect(angular.element(checkBoxInput[0]).hasClass("md-checked")).toBe(true);
		expect(checkBoxInput[0].disabled).toBe(true);
		expect(!!result.container[0].querySelector('.start-date')).toBeTruthy();
		expect(!!result.container[0].querySelector('.end-date')).toBeTruthy();
	});

	it('should not display full day absence check box with only intraday absence permission', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });
		var result = setUp('2018-08-01');
		expect(result.container[0].querySelectorAll('.fullDayAbsenceCheckBox').length).toBe(0);
		expect(!!result.container[0].querySelector('.start-time')).toBeTruthy();
		expect(!!result.container[0].querySelector('.end-time')).toBeTruthy();
	});

	it('should set default start date and end date correctly for adding fullday absence', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true });
		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1'
			}];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(selectedAgents);
		var personScheduleVMs = [
			{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Shifts: [{
					Date: '2018-08-01',
					ProjectionTimeRange: {
						Start: '2018-08-01 07:00',
						End: '2018-08-01 16:00'
					}
				}],
				ScheduleStartTime: function () { return "2018-08-01 07:00"; }
			}];
		var result = setUp('2018-08-01', null, personScheduleVMs);

		expect(result.container[0].querySelector('.start-date input').value).toEqual("8/1/18");
		expect(result.container[0].querySelector('.end-date input').value).toEqual("8/1/18");
	});

	it('should set default start time and end time correctly for adding intraday absence', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });
		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1'
			}];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(selectedAgents);
		var personScheduleVMs = [
			{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Shifts: [{
					Date: '2018-08-01',
					ProjectionTimeRange: {
						Start: '2018-08-01 07:00',
						End: '2018-08-01 16:00'
					}
				}],
				ScheduleStartTime: function () { return "2018-08-01 07:00"; }
			}];
		var result = setUp('2018-08-01', null, personScheduleVMs);

		expect(result.container[0].querySelector('.start-time team-schedule-datepicker input').value).toEqual("8/1/18");
		expect(result.container[0].querySelector('.start-time .uib-timepicker .hours input').value).toEqual("07");
		expect(result.container[0].querySelector('.start-time .uib-timepicker .minutes input').value).toEqual("00");
		expect(result.container[0].querySelector('.end-time team-schedule-datepicker input').value).toEqual("8/1/18");
		expect(result.container[0].querySelector('.end-time  .uib-timepicker .hours input').value).toEqual("08");
		expect(result.container[0].querySelector('.end-time .uib-timepicker .minutes input').value).toEqual("00");
	});

	it('should not allow to add intraday absence when startime is early or equal to endtime', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });
		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1'
			}];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(selectedAgents);

		var result = setUp("2018-08-01", null, [{
			Date: '2018-08-01',
			PersonId: 'agent1',
			Timezone: {
				IanaId: 'Europe/Stockholm'
			},
			Shifts: [{
				Date: '2018-08-01',
				ProjectionTimeRange: {
					Start: '2018-08-01 07:00',
					End: '2018-08-01 16:00'
				}
			}],
			ScheduleStartTime: function () { return ""; }
		}]);

		result.container[0].querySelectorAll('.absence-selector md-option')[0].click();
		setTime(result.container, 10, 10);

		var applyButton = result.container[0].querySelectorAll('#applyAbsence');
		expect(applyButton[0].disabled).toBe(true);
	});

	it('should not allow to add full day absence if the selected agents timezone is different from the selected time zone', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true });
		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: { IanaId: 'Asia/Hong_Kong' }
			}];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(selectedAgents);

		var result = setUp("2018-08-01", null, [{
			Date: '2018-08-01',
			PersonId: 'agent1',
			Timezone: {
				IanaId: 'Asia/Hong_Kong'
			},
			ScheduleStartTime: function () { return "2018-08-01 10:00"; }
		}]);

		result.container[0].querySelectorAll('.absence-selector md-option')[0].click();

		var applyButton = result.container[0].querySelector('#applyAbsence');
		expect(applyButton.disabled).toBeTruthy();
	});

	it('should able to add full day absence and show the error message for agents who is in different timezone', function () {
		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true });
		var selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: { IanaId: 'Asia/Hong_Kong' }
			},
			{
				PersonId: 'agent2',
				Name: 'agent2',
				Timezone: { IanaId: 'Europe/Stockholm' }
			}];
		fakePersonSelectionService.setFakeCheckedPersonInfoList(selectedAgents);

		var result = setUp("2018-08-01", null, [{
			Date: '2018-08-01',
			PersonId: 'agent1',
			Timezone: {
				IanaId: 'Asia/Hong_Kong'
			},
			ScheduleStartTime: function () { return ""; }
		},
		{
			Date: '2018-08-01',
			PersonId: 'agent2',
			Timezone: {
				IanaId: 'Europe/Stockholm'
			},
			ScheduleStartTime: function () { return ""; }
		}]);
		result.container[0].querySelectorAll(".absence-selector md-option")[0].click();

		var applyButton = result.container[0].querySelector('#applyAbsence');
		expect(applyButton.disabled).toBeFalsy();
		expect(result.container[0].innerHTML.indexOf('CannotApplyThisToAgentsInADifferentTimeZone') != -1).toBeTruthy();
		expect(result.container[0].innerHTML.indexOf('agent1') != -1).toBeTruthy();
	});

	it('should apply correct data for adding absence from part of day to x day when toggle WfmTeamSchedule_AddAbsenceFromPartOfDayToXDay_46010 is on', function () {
		fakeToggle.WfmTeamSchedule_AddAbsenceFromPartOfDayToXDay_46010 = true;

		fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });
		fakePersonSelectionService.setFakeCheckedPersonInfoList([
			{
				PersonId: 'agent1',
				Name: 'agent1'
			}]);

		var result = setUp('2018-08-01', null, [{
			Date: '2018-08-01',
			PersonId: 'agent1',
			Timezone: {
				IanaId: 'Europe/Stockholm'
			},
			ScheduleStartTime: function () { return ""; }
		}]);

		result.container[0].querySelectorAll('.absence-selector md-option')[0].click();
		var startDateEl = result.container[0].querySelector('.start-time team-schedule-datepicker input');
		var endDateEl = result.container[0].querySelector('.end-time  team-schedule-datepicker input');
		startDateEl.value = "8/1/18";
		angular.element(startDateEl).triggerHandler('change');
		endDateEl.value = "8/3/18";
		angular.element(endDateEl).triggerHandler('change');
		setTime(result.container, "10", "11");

		$timeout(function () {
			var applyButton = result.container[0].querySelector('#applyAbsence');
			applyButton.click();

			var lastAbsence = fakeAbsenceService.getAddAbsenceCalledWith();
			expect(lastAbsence.PersonIds[0]).toEqual('agent1');
			expect(lastAbsence.Date).toEqual('2018-08-01');
			expect(lastAbsence.Start).toEqual('2018-08-01T10:00');
			expect(lastAbsence.End).toEqual('2018-08-03T11:00');
		}, 300);
		$timeout.flush();
	});


	commonTestsInDifferentLocale();

	function commonTestsInDifferentLocale() {
		it('should apply add fullday absence with correct data', function () {
			fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: false, IsAddFullDayAbsenceAvailable: true });
			fakePersonSelectionService.setFakeCheckedPersonInfoList([
				{
					PersonId: 'agent1',
					Name: 'agent1'
				}]);
			var result = setUp('2018-08-01', null, [{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Shifts: [
					{
						Date: '2018-08-01',
						Projections: [
						],
						ProjectionTimeRange: null
					}],
				ScheduleStartTime: function () { return ""; }
			}]);

			result.container[0].querySelectorAll(".absence-selector md-option")[0].click();
			var startInput = result.container[0].querySelector(".start-date input");
			var endInput = result.container[0].querySelector(".end-date input");
			startInput.value = '8/2/18';
			angular.element(startInput).triggerHandler('change');
			endInput.value = '8/2/18';
			angular.element(endInput).triggerHandler('change');

			$timeout(function () {
				var panel = result.container;
				var applyButton = panel[0].querySelector('#applyAbsence');
				applyButton.click();

				var lastAbsence = fakeAbsenceService.getAddAbsenceCalledWith();
				expect(lastAbsence.PersonIds[0]).toEqual('agent1');
				expect(lastAbsence.Start).toEqual('2018-08-02');
				expect(lastAbsence.End).toEqual('2018-08-02');
			}, 300);
			$timeout.flush();
		});

		it('should apply add intraday absence with correct time range based on the selected time zone', function () {
			fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });
			fakePersonSelectionService.setFakeCheckedPersonInfoList([
				{
					PersonId: 'agent1',
					Name: 'agent1'
				}]);
			var result = setUp("2018-03-25", null, [{
				Date: '2018-03-25',
				PersonId: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Shifts: [
					{
						Date: '2018-03-25',
						Projections: [
						],
						ProjectionTimeRange: {
							Start: '2018-03-25 01:00',
							End: '2018-03-25 17:00'
						}
					}],
				ScheduleStartTime: function () { return ""; }
			}]);
			result.container[0].querySelectorAll('.absence-selector md-option')[0].click();
			setTime(result.container, "01", "03");

			var applyButton = result.container[0].querySelectorAll('#applyAbsence');
			applyButton[0].click();

			var lastAbsence = fakeAbsenceService.getAddAbsenceCalledWith();
			expect(lastAbsence.Start).toEqual('2018-03-25T01:00');
			expect(lastAbsence.End).toEqual('2018-03-25T03:00');
		});

		it('should apply add intraday absence with correct data', function () {
			fakePermissions.setPermissions({ IsAddIntradayAbsenceAvailable: true, IsAddFullDayAbsenceAvailable: false });
			fakePersonSelectionService.setFakeCheckedPersonInfoList([
				{
					PersonId: 'agent1',
					Name: 'agent1'
				}]);

			var result = setUp('2018-08-01', null, [{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Shifts: [{
					Date: '2018-08-01',
					ProjectionTimeRange: {
						Start: '2018-08-01 07:00',
						End: '2018-08-01 16:00'
					}
				}],
				ScheduleStartTime: function () { return ""; }
			}]);

			result.container[0].querySelectorAll('.absence-selector md-option')[0].click();
			setTime(result.container, "10", "11");

			var applyButton = result.container[0].querySelector('#applyAbsence');
			applyButton.click();

			var lastAbsence = fakeAbsenceService.getAddAbsenceCalledWith();
			expect(lastAbsence.PersonIds[0]).toEqual('agent1');
			expect(lastAbsence.Date).toEqual('2018-08-01');
			expect(lastAbsence.Start).toEqual('2018-08-01T10:00');
			expect(lastAbsence.End).toEqual('2018-08-01T11:00');
		});
	}

	describe('in locale ar-AE', function () {
		beforeAll(function () {
			moment.locale('ar-AE');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	describe('in locale fa-IR', function () {
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});

	function setTime(container, startHour, endHour) {
		var startHourEl = container[0].querySelector('.start-time .uib-timepicker .hours input');
		startHourEl.value = startHour;
		angular.element(startHourEl).triggerHandler('change');
		var endHourEl = container[0].querySelector('.end-time .uib-timepicker .hours input');
		endHourEl.value = endHour;
		angular.element(endHourEl).triggerHandler('change');
	}

	function setUp(date, timeZone, personSchedulesVMs) {
		var date, configurations;
		var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = moment(date || '2016-06-15').toDate();
		scope.timezone = timeZone || "Europe/Stockholm";

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		vm.setReady(true);
		vm.setActiveCmd('AddAbsence');

		personSchedulesVMs && personSchedulesVMs.forEach(function (scheduleVM) {
			vm.scheduleManagementSvc.setPersonScheduleVm(scheduleVM.PersonId, scheduleVM);
		});
		scope.$apply();

		var commandScope = angular.element(container[0].querySelector('.add-absence')).scope();

		var obj = {
			container: container,
			commandScope: commandScope,
			scope: scope
		};
		return obj;
	}

	function FakeScheduleManagementService() {

		this.schedules = function () {
			var schedules = [];
			for (var personId in savedPersonScheduleVm) {
				schedules.push(savedPersonScheduleVm[personId]);
			}
			return schedules;
		};

		this.newService = function () {
			return new FakeScheduleManagementService();
		};

		var savedPersonScheduleVm = {};

		this.setPersonScheduleVm = function (personId, vm) {
			savedPersonScheduleVm[personId] = vm;
		}

		this.findPersonScheduleVmForPersonId = function (personId) {
			return savedPersonScheduleVm[personId];
		}
	}

	function FakeScheduleHelper() {
		var earliestStartTime = null;
		var latestStartTime = null;
		var latestEndTime = null;

		this.setEarliestStartTime = function (date) {
			earliestStartTime = date;
		};

		this.setLatestStartTime = function (date) {
			latestStartTime = date;
		};

		this.setLatestEndTime = function (date) {
			latestEndTime = date;
		};

		this.getEarliestStartOfSelectedSchedules = function () {
			return earliestStartTime;
		};

		this.getLatestStartOfSelectedSchedules = function () {
			return latestStartTime;
		};

		this.getLatestPreviousDayOvernightShiftEnd = function () {
			return latestEndTime;
		};
	}

	function FakePermissions() {
		var _permissions = {}

		this.all = function () {
			return _permissions;
		}

		this.setPermissions = function (permissions) {
			_permissions = permissions;
		}
	}

	function FakePersonAbsence() {
		var targetAbsence = null;
		this.loadAbsences = function () {
			return {
				then: function (cb) {
					cb([
						{
							Id: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
							Name: 'Sick'
						},
						{
							Id: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
							Name: 'Holiday'
						}
					]);
				}
			};
		};

		this.addAbsence = function (input) {
			targetAbsence = input;
			return {
				then: (function (cb) {
					cb({ data: [] });
				})
			};
		};

		this.getAddAbsenceCalledWith = function () {
			return targetAbsence;
		};
	}

	function FakePersonSelectionService() {
		var fakePersonList = [];

		this.setFakeCheckedPersonInfoList = function (input) {
			fakePersonList = input;
		}

		this.getCheckedPersonInfoList = function () {
			return fakePersonList;
		}
	}

	function FakeCommandCheckService() {
		var checkStatus = false;


		this.getCommandCheckStatus = function () {
			return checkStatus;
		}

		this.resetCommandCheckStatus = function () {
			checkStatus = false;
		}

		this.checkPersonalAccounts = function (requestedData) {
			return {
				then: function (cb) {
					cb(requestedData);
				}
			}
		};
	}

	function FakeCurrentUserInfo() {
		this.CurrentUserInfo = function () {
			return {
				DefaultTimeZone: "Europe/Stockholm",
				DateFormat: 'sv-SE'
			};
		};
	}

});