describe('<add-activity>', function () {
	'use strict';

	var $compile,
		$rootScope,
		utility,
		fakeActivityService,
		fakeCommandCheckService,
		personSelection,
		scheduleManagement;

	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: 'Asia/Hong_Kong' };
		}
	};

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		fakeActivityService = new FakeActivityService();
		fakeCommandCheckService = new FakeCommandCheckService();

		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
			$provide.service('CommandCheckService', function () {
				return fakeCommandCheckService;
			});
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_,
		_$compile_,
		_$httpBackend_,
		_UtilityService_,
		PersonSelection,
		ScheduleManagement) {

		$compile = _$compile_;
		$rootScope = _$rootScope_;
		utility = _UtilityService_;
		personSelection = PersonSelection;
		scheduleManagement = ScheduleManagement;

		_$httpBackend_.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
	}));

	it('add-activity should get date from container', function () {
		var result = setUp();
		expect(result.commandControl.selectedDate()).toBe('2016-06-15');
	});

	it('should load activity list', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: [{
					StartInUtc: '2018-08-01 08:00',
					EndInUtc: '2018-08-01 09:00',
				}]
			}]
			, '2018-08-01');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Europe/Stockholm');

		var activities = result.container[0].querySelectorAll('.add-activity .activity-selector md-option');

		expect(activities.length).toBe(5);
	});

	it('should see a disabled button when no activity selected', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: [{
					StartInUtc: '2018-08-01 08:00',
					EndInUtc: '2018-08-01 09:00',
				}]
			}]
			, '2018-08-01');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Europe/Stockholm');

		var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should not allow to add activity if changed the belongs to date', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: [{
					StartInUtc: '2018-08-01 08:00',
					EndInUtc: '2018-08-01 09:00',
				}]
			}]
			, '2018-08-01');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Europe/Stockholm');

		result.container[0].querySelectorAll('.activity-selector md-option')[0].click();
		setTime(result.container, 8, 17);
		result.container[0].querySelector('teams-time-range-picker md-switch').click();
		result.scope.$apply();

		var errorEl = result.container[0].querySelectorAll('.text-danger');
		expect(!!errorEl.length).toBeTruthy();
		expect(result.container[0].querySelector('#applyActivity').disabled).toEqual(true);
	});

	it('should not allow to add activity if agents shift exceed 36 hours', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: [{
					StartInUtc: '2018-08-01 01:00',
					EndInUtc: '2018-08-01 22:00',
				}]
			}]
			, '2018-08-01');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Europe/Stockholm');

		result.container[0].querySelectorAll('.activity-selector md-option')[0].click();
		setTime(result.container, 22, 20);

		var errorEl = result.container[0].querySelectorAll('.text-danger');
		expect(!!errorEl.length).toBeTruthy();
		expect(result.container[0].querySelector('#applyActivity').disabled).toEqual(true);
	});

	it('should allow to add activity for valid agents and show error message for invalid agents', function () {
		scheduleManagement.resetSchedules(
			[
				{
					Date: '2018-08-01',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: "Asia/Shanghai"
					},
					Projection: [{
						StartInUtc: '2018-07-31 16:00',
						EndInUtc: '2018-08-01 23:00',
					}]
				},
				{
					Date: '2018-08-01',
					PersonId: 'agent2',
					Name: 'agent2',
					Timezone: {
						IanaId: "Asia/Shanghai"
					},
					Projection: []
				}]
			, '2018-08-01', 'Europe/Berlin');

		scheduleManagement.groupScheduleVm.Schedules.forEach(function (personSchedule) {
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');
		});

		var result = setUp('2018-08-01', 'Europe/Berlin');
		result.container[0].querySelectorAll('.activity-selector md-option')[0].click();
		setTime(result.container, 12, 11);
		result.scope.$apply();

		var errorEl = result.container[0].querySelectorAll('.text-danger');
		expect(!!errorEl.length).toBeTruthy();
		expect(result.container[0].querySelector('#applyActivity').disabled).toEqual(false);
	});

	it('should set default start time to next quarter from now when no other shifts on today', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-03-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: []
			}]
			, '2018-03-01', 'Europe/Stockholm');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-03-01');

		utility.setNowDate(new Date("2018-03-01T10:00:00+01:00"));

		var result = setUp('2018-03-01', 'Europe/Stockholm');
		expect(result.commandControl.timeRange.startTime).toBe("2018-03-01 10:15");
	});

	it('should set default start time to 8:00 when now is earlier than 8:00 on today', function () {
		var date = new Date("2018-03-01T05:00:00+00:00");
		utility.setNowDate(date);

		scheduleManagement.resetSchedules(
			[{
				Date: '2018-03-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: []
			}]
			, '2018-03-01', 'Europe/Stockholm');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-03-01');

		var result = setUp('2018-03-01', 'Etc/Utc');
		expect(result.commandControl.timeRange.startTime).toBe("2018-03-01 08:00");
	});

	it('should set default start time to next quarter when now is later than 8:00', function () {
		var date = new Date("2018-03-01T09:10:00+00:00");
		utility.setNowDate(date);

		scheduleManagement.resetSchedules(
			[{
				Date: '2018-03-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: []
			}]
			, '2018-03-01', 'Etc/Utc');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-03-01');

		var result = setUp(date, 'Etc/Utc');
		expect(result.commandControl.timeRange.startTime).toBe("2018-03-01 09:15");
	});

	it('should set default start time to an hour from the end of previous day over night shift to avoid to add activity to previous day', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-07-31',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2018-07-31 22:00',
					EndInUtc: '2018-08-01 09:00'
				}]
			}]
			, '2018-08-01', 'Etc/Utc');

		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Etc/Utc');
		var vm = result.commandControl;

		expect(vm.timeRange.startTime).toEqual('2018-08-01 10:00');
	});

	it('should set default start time to an hour from the start of selected days shift is after the end of yesterdays overnight shift ', function () {

		scheduleManagement.resetSchedules(
			[{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2018-08-01 22:00',
					EndInUtc: '2018-08-02 09:00'
				}]
			},
			{
				Date: '2018-07-31',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2018-07-31 22:00',
					EndInUtc: '2018-08-01 09:00'
				}]
			}]
			, '2018-08-01', 'Etc/Utc');

		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Etc/Utc');
		var vm = result.commandControl;

		expect(vm.timeRange.startTime).toEqual('2018-08-01 23:00');
	});

	it('should set default start time to next quarter when selected date is same with now and next quarter is later than an hour from the end of previous day over night shift', function () {
		var date = new Date("2018-08-01T10:00:00+00:00");
		utility.setNowDate(date);

		scheduleManagement.resetSchedules(
			[{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2018-08-01 08:00',
					EndInUtc: '2018-08-01 16:00'
				}]
			},
			{
				Date: '2018-07-31',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2018-07-31 22:00',
					EndInUtc: '2018-08-01 05:00'
				}]
			}]
			, '2018-08-01', 'Etc/Utc');

		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Etc/Utc');
		var vm = result.commandControl;

		expect(vm.timeRange.startTime).toEqual('2018-08-01 10:15');

	});

	it('should set default end time to an hour from default start time', function () {
		var date = new Date("2018-03-01T05:00:00+00:00");
		utility.setNowDate(date);

		scheduleManagement.resetSchedules(
			[{
				Date: '2018-03-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: []
			}]
			, '2018-03-01', 'Europe/Stockholm');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-03-01');

		var result = setUp('2018-03-01', 'Etc/Utc');
		expect(result.commandControl.timeRange.endTime).toBe("2018-03-01 09:00");
	});

	it('should invoke action callback after calling add activity', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-08-01',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Europe/Stockholm'
				},
				Projection: [{
					StartInUtc: '2018-08-01 08:00',
					EndInUtc: '2018-08-01 09:00',
				}]
			}]
			, '2018-08-01', 'Europe/Stockholm');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

		var result = setUp('2018-08-01', 'Europe/Stockholm');

		var cbMonitor = null;
		function actionCb() {
			cbMonitor = true;
		}

		result.container.isolateScope().vm.setActionCb('AddActivity', actionCb);

		result.container[0].querySelectorAll('.activity-selector md-option')[0].click();
		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(cbMonitor).toBeTruthy();
	});

	function commonTestsInDifferentLocale() {
		it('should add activity with correct data', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2018-08-01',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: 'Asia/Hong_Kong'
					},
					Projection: []
				}]
				, '2018-08-01', 'Asia/Hong_Kong');
			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

			var result = setUp('2018-08-01', 'Asia/Hong_Kong');
			result.container[0].querySelectorAll('.activity-selector md-option')[0].click();

			setTime(result.container, 8, 16);

			var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
			applyButton.triggerHandler('click');

			result.scope.$apply();

			var activityData = fakeActivityService.getAddActivityCalledWith();
			expect(activityData).not.toBeNull();
			expect(activityData.PersonDates).toEqual([{ Date: '2018-08-01', PersonId: 'agent1' }]);
			expect(activityData.ActivityId).toEqual('472e02c8-1a84-4064-9a3b-9b5e015ab3c6');
			expect(activityData.ActivityType).toEqual(1);
			expect(activityData.StartTime).toEqual('2018-08-01T08:00');
			expect(activityData.EndTime).toEqual('2018-08-01T16:00');
			expect(activityData.TrackedCommandInfo.TrackId).toBe(result.commandControl.trackId);
		});

		it('should apply with correct time range when selected time zone is different from logon user time zone', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2018-08-01',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: 'Europe/Berlin'
					},
					Projection: []
				}]
				, '2018-08-01', 'Europe/Berlin');
			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

			var result = setUp('2018-08-01', 'Europe/Berlin');
			result.container[0].querySelectorAll('.activity-selector md-option')[0].click();

			setTime(result.container, 8, 16);

			var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
			applyButton.triggerHandler('click');

			result.scope.$apply();

			var activityData = fakeActivityService.getAddActivityCalledWith();
			expect(activityData).not.toBeNull();
			expect(activityData.StartTime).toEqual('2018-08-01T14:00');
			expect(activityData.EndTime).toEqual('2018-08-01T22:00');
		});

		it('should apply with correct activity type for adding personal activity ', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2018-08-01',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: 'Asia/Hong_Kong'
					},
					Projection: []
				}]
				, '2018-08-01', 'Asia/Hong_Kong');
			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-08-01');

			var result = setUp('2018-08-01', 'Asia/Hong_Kong', 'AddPersonalActivity');
			result.container[0].querySelectorAll('.activity-selector md-option')[0].click();

			setTime(result.container, 8, 16);

			var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
			applyButton.triggerHandler('click');

			result.scope.$apply();
			var activityData = fakeActivityService.getAddActivityCalledWith();
			expect(activityData.ActivityType).toEqual(2);
		
		});
	}

	commonTestsInDifferentLocale();

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
		beforeEach(function () {
			moment.locale('fa-IR');
		});

		afterEach(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
	});


	function setUp(inputDate, timezone, commandKey) {
		var date;
		var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;

		scope.curDate = date;
		if (timezone) {
			scope.timezone = timezone;
		} else {
			scope.timezone = "Asia/Hong_Kong";
		}

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		vm.setReady(true);
		vm.setActiveCmd(commandKey || 'AddActivity');
		vm.scheduleManagementSvc = scheduleManagement;
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector(".add-activity")).scope().vm;

		var obj = {
			container: container,
			commandControl: commandControl,
			scope: scope
		};

		return obj;
	}

	function FakeCommandCheckService() {
		var fakeResponse = {
			data: []
		};
		var checkStatus = false,
			fakeOverlappingList = [];

		this.checkOverlappingCertainActivities = function () {
			return {
				then: function (cb) {
					checkStatus = true;
					cb(fakeResponse);
				}
			}
		}

		this.getCommandCheckStatus = function () {
			return checkStatus;
		}

		this.resetCommandCheckStatus = function () {
			checkStatus = false;
		}

		this.getCheckFailedList = function () {
			return fakeOverlappingList;
		}
		this.checkAddActivityOverlapping = function (requestedData) {
			return {
				then: function (cb) {
					cb(requestedData);
				}
			}
		};
	}

	function getAvailableActivities() {
		return [
			{
				"Id": "472e02c8-1a84-4064-9a3b-9b5e015ab3c6",
				"Name": "E-mail"
			},
			{
				"Id": "5c1409de-a0f1-4cd4-b383-9b5e015ab3c6",
				"Name": "Invoice"
			},
			{
				"Id": "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				"Name": "Phone"
			},
			{
				"Id": "84db44f4-22a8-44c7-b376-a0a200da613e",
				"Name": "Sales"
			},
			{
				"Id": "35e33821-862f-461c-92db-9f0800a8d095",
				"Name": "Social Media"
			}
		];
	}

	function FakeActivityService() {
		var availableActivities = [];
		var targetActivity = null;
		var fakeResponse = { data: [] };

		this.fetchAvailableActivities = function () {
			return {
				then: function (cb) {
					cb(availableActivities);
				}
			};
		};

		this.addActivity = function (input) {
			targetActivity = input;
			return {
				then: (function (cb) {
					cb(fakeResponse);
				})
			};
		};

		this.getAddActivityCalledWith = function () {
			return targetActivity;
		};

		this.setAvailableActivities = function (activities) {
			availableActivities = activities;
		};
	}

	function setTime(container, startHour, endHour) {
		var hourEls = container[0].querySelectorAll('.hours input');
		var startHourEl = hourEls[0];
		startHourEl.value = startHour;
		angular.element(startHourEl).triggerHandler('change');
		var endHourEl = hourEls[1];
		endHourEl.value = endHour;
		angular.element(endHourEl).triggerHandler('change');
	}
});