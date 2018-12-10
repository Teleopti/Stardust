describe('teamschedule add overtime activity directive tests', function () {
	'use strict';

	var $compile,
		$rootScope,
		$httpBackend,
		fakeActivityService,
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
		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_, PersonSelection, ScheduleManagement) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		personSelection = PersonSelection;
		scheduleManagement = ScheduleManagement;

		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
	}));

	it('should set default start time to the end of schedule and use 1 hour for default overtime activity length', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-12-04',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Asia/Hong_Kong'
				},
				Projection: [{
					ShiftLayerIds: ['layer1'],
					StartInUtc: '2018-12-04 00:00',
					EndInUtc: '2018-12-04 09:00',
				}]
			}]
			, '2018-12-04', 'Asia/Hong_Kong');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-12-04');

		var result = setUp('2018-12-04', 'Asia/Hong_Kong');

		expect(result.commandControl.fromTime).toEqual('2018-12-04 17:00');
		expect(result.commandControl.toTime).toEqual('2018-12-04 18:00');
	});

	it('should set default start time to 8 and end time to 17 when adding overtime activity on day offs', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-12-04',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Asia/Hong_Kong'
				},
				Projection: [],
				DayOff:
				{
					"DayOffName": "DayOff",
					"StartInUtc": "2018-12-04 00:00",
					"EndInUtc": "2018-12-04 23:59"
				}
			}]
			, '2018-12-04', 'Asia/Hong_Kong');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-12-04');

		var result = setUp('2018-12-04', 'Asia/Hong_Kong');

		expect(result.commandControl.fromTime).toEqual('2018-12-04 08:00');
		expect(result.commandControl.toTime).toEqual('2018-12-04 17:00');
	});

	it('should set default start time to 8 and end time to 17 when adding overtime activity on empty days', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-12-04',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Asia/Hong_Kong'
				},
				Projection: [],
			}]
			, '2018-12-04', 'Asia/Hong_Kong');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-12-04');

		var result = setUp('2018-12-04', 'Asia/Hong_Kong');

		expect(result.commandControl.fromTime).toEqual('2018-12-04 08:00');
		expect(result.commandControl.toTime).toEqual('2018-12-04 17:00');
	});

	it('should set default start time to 8 and end time to 17 when adding overtime activity on full day absence day', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-12-04',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Asia/Hong_Kong'
				},
				IsFullDayAbsence: true,
				Projection: [
					{
						ActivityId: "47d9292f-ead6-40b2-ac4f-9b5e015ab330",
						ShiftLayerIds: null,
						ParentPersonAbsences: ["bc63cf37-e243-4adc-8456-a97b0085c70a"],
						Color: "#795548",
						Description: "Phone",
						StartInUtc: "2018-10-16 08:00",
						EndInUtc: "2018-10-16 10:00",
						IsOvertime: false
					}
				],
			}]
			, '2018-12-04', 'Asia/Hong_Kong');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2018-12-04');

		var result = setUp('2018-12-04', 'Asia/Hong_Kong');

		expect(result.commandControl.fromTime).toEqual('2018-12-04 08:00');
		expect(result.commandControl.toTime).toEqual('2018-12-04 17:00');
	});

	it('should get default overtime period based on the upper most agents schedule', function () {
		scheduleManagement.resetSchedules(
			[
				{
					Date: '2018-12-04',
					PersonId: 'agent2',
					Name: 'agent2',
					Timezone: {
						IanaId: 'Asia/Hong_Kong'
					},
					Projection: [{
						ShiftLayerIds: ['layer2'],
						StartInUtc: '2018-12-04 02:00',
						EndInUtc: '2018-12-04 11:00',
					}]
				},
				{
					Date: '2018-12-04',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: 'Asia/Hong_Kong'
					},
					Projection: []
				}]
			, '2018-12-04', 'Asia/Hong_Kong');
		scheduleManagement.groupScheduleVm.Schedules.forEach(function (personSchedule) {
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-12-04');
		});

		var result = setUp('2018-12-04', 'Asia/Hong_Kong');

		expect(result.commandControl.fromTime).toEqual('2018-12-04 19:00');
		expect(result.commandControl.toTime).toEqual('2018-12-04 20:00');
	});

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

	function commonTestsInDifferentLocale() {
		it('should call add overtime when click apply with correct data', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2018-12-04',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: 'Asia/Hong_Kong'
					},
					IsFullDayAbsence: true,
					Projection: [],
					MultiplicatorDefinitionSetIds: ['5c1409de-a0f1-4cd4-b383-9b5e015ab789']
				}]
				, '2018-12-04', 'Asia/Hong_Kong');

			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-12-04');

			var result = setUp('2018-12-04', 'Asia/Hong_Kong');

			result.container[0].querySelector('.activity-selector md-option').click();
			result.container[0].querySelectorAll('.definition-selector md-option')[0].click();
			
			setTime(result.container, "09", "10");
			result.scope.$apply();

			var applyButton = result.container[0].querySelectorAll('#applyOvertime');
			applyButton[0].click();

			expect(fakeActivityService.lastAddedOvertimeActivity.PersonDates[0].PersonId).toEqual('agent1');
			expect(fakeActivityService.lastAddedOvertimeActivity.PersonDates[0].Date).toEqual('2018-12-04');
			expect(fakeActivityService.lastAddedOvertimeActivity.ActivityId).toEqual('472e02c8-1a84-4064-9a3b-9b5e015ab3c6');
			expect(fakeActivityService.lastAddedOvertimeActivity.MultiplicatorDefinitionSetId).toEqual('5c1409de-a0f1-4cd4-b383-9b5e015ab789');
			expect(fakeActivityService.lastAddedOvertimeActivity.StartDateTime).toEqual('2018-12-04T09:00');
			expect(fakeActivityService.lastAddedOvertimeActivity.EndDateTime).toEqual('2018-12-04T10:00');
		});

		it('should apply with correct time range based on the selected time zone', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2018-12-04',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: 'Asia/Hong_Kong'
					},
					IsFullDayAbsence: true,
					Projection: [],
					MultiplicatorDefinitionSetIds: ['5c1409de-a0f1-4cd4-b383-9b5e015ab789']
				}]
				, '2018-12-04', 'Asia/Hong_Kong');

			var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-12-04');

			var result = setUp('2018-12-04', 'Etc/Utc');

			result.container[0].querySelector('.activity-selector md-option').click();
			result.container[0].querySelectorAll('.definition-selector md-option')[0].click();

			setTime(result.container, "09", "10");
			result.scope.$apply();

			var applyButton = result.container[0].querySelectorAll('#applyOvertime');
			applyButton[0].click();


			expect(fakeActivityService.lastAddedOvertimeActivity.StartDateTime).toEqual('2018-12-04T17:00');
			expect(fakeActivityService.lastAddedOvertimeActivity.EndDateTime).toEqual('2018-12-04T18:00');
		});

	}

	function setUp(inputDate, timezone) {
		var date;
		var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2018-01-23').toDate();
		else
			date = inputDate;

		scope.curDate = date;
		scope.timezone = timezone || 'Asia/Shanghai';
		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var container = $compile(html)(scope);
		scope.$apply();

		var containerVm = container.isolateScope().vm;
		containerVm.setReady(true);
		containerVm.setActiveCmd('AddOvertimeActivity');
		containerVm.scheduleManagementSvc = scheduleManagement;
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector('.add-overtime-activity')).scope().$ctrl;

		return {
			container: container,
			commandControl: commandControl,
			scope: scope
		};
	}

	function setTime(container, startHour, endHour) {
		var hoursEl = container[0].querySelectorAll('.uib-timepicker .hours input');
		var startHourEl = hoursEl[0];
		startHourEl.value = startHour;
		angular.element(startHourEl).triggerHandler('change');
		var endHourEl = hoursEl[1];
		endHourEl.value = endHour;
		angular.element(endHourEl).triggerHandler('change');
	}

	function getAvailableActivities() {
		return [
			{
				'Id': '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
				'Name': 'E-mail'
			},
			{
				'Id': '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
				'Name': 'Invoice'
			},
			{
				'Id': '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
				'Name': 'Phone'
			},
			{
				'Id': '84db44f4-22a8-44c7-b376-a0a200da613e',
				'Name': 'Sales'
			},
			{
				'Id': '35e33821-862f-461c-92db-9f0800a8d095',
				'Name': 'Social Media'
			}
		];
	}

	function FakeActivityService() {
		var availableActivities = [];
		this.lastAddedOvertimeActivity = null;
		var fakeResponse = { data: [] };

		this.fetchAvailableActivities = function () {
			return {
				then: function (cb) {
					cb(availableActivities);
				}
			};
		};

		this.addOvertimeActivity = function (input) {
			this.lastAddedOvertimeActivity = input;
			return {
				then: (function (cb) {
					cb(fakeResponse);
				})
			};
		};

		this.setAvailableActivities = function (activities) {
			availableActivities = activities;
		};

		this.fetchAvailableDefinitionSets = function () {
			return {
				then: function (cb) {
					cb([{
						'Id': '5c1409de-a0f1-4cd4-b383-9b5e015ab789',
						'Name': 'Overtime Paid'
					}]);
				}
			};
		}
	}
});