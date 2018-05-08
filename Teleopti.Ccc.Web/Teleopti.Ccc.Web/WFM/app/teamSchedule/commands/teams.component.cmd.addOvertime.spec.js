﻿describe('teamschedule add overtime activity directive tests', function () {
	'use strict';

	var $compile,
		$rootScope,
		$httpBackend,
		utility,
		fakeActivityService,
		fakePersonSelectionService,
		fakeScheduleManagementSvc,
		scheduleHelper;

	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: 'Asia/Hong_Kong' };
		}
	};

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		fakeActivityService = new FakeActivityService();
		fakePersonSelectionService = new FakePersonSelectionService();
		fakeScheduleManagementSvc = new FakeScheduleManagementService();

		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
			$provide.service('ScheduleHelper', function () {
				return scheduleHelper;
			});
			$provide.service('PersonSelection', function () {
				return fakePersonSelectionService;
			});
			$provide.service('ScheduleManagement', function () {
				return fakeScheduleManagementSvc;
			});
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_, _UtilityService_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		utility = _UtilityService_;

		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
	}));

	it('should use 1 hour for default overtime activity length', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			OrderIndex: 0,
			AllowSwap: false,
			IsDayOff: false,
			IsEmptyDay: false,
			IsFullDayAbsence: false,
			ScheduleStartTime: '2018-01-23T08:00:00',
			ScheduleEndTime: '2018-01-23T17:00:00',
			SelectedAbsences: [],
			SelectedActivities: ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: []
		}];

		fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);
		var result = setUp();

		expect(moment(result.commandControl.fromTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 17:00:00').format('YYYY-MM-DD HH:mm'));
		expect(moment(result.commandControl.toTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 18:00:00').format('YYYY-MM-DD HH:mm'));
	});

	it('should get normal shift end time as default start time for one agent', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			OrderIndex: 0,
			AllowSwap: false,
			IsDayOff: false,
			IsEmptyDay: false,
			IsFullDayAbsence: false,
			ScheduleStartTime: '2018-01-23T08:00:00',
			ScheduleEndTime: '2018-01-23T17:00:00',
			SelectedAbsences: [],
			SelectedActivities: ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: []
		}];

		fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);
		var result = setUp();

		expect(moment(result.commandControl.fromTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 17:00:00').format('YYYY-MM-DD HH:mm'));
	});

	it('should get default work time period when adding overtime activity on day offs', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			OrderIndex: 0,
			AllowSwap: false,
			IsDayOff: true,
			IsEmptyDay: false,
			IsFullDayAbsence: false,
			ScheduleStartTime: '2018-01-23T00:00:00',
			ScheduleEndTime: '2018-01-23T23:59:00',
			SelectedAbsences: [],
			SelectedActivities: [],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: [{
				Date: '2018-01-23',
				DayOffName: 'Day off',
				Length: 100,
				Parent: null,
				StartPosition: 0
			}]
		}];

		fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);
		var result = setUp();

		expect(moment(result.commandControl.fromTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 08:00:00').format('YYYY-MM-DD HH:mm'));
		expect(moment(result.commandControl.toTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 17:00:00').format('YYYY-MM-DD HH:mm'));
	});

	it('should get default work time period when adding overtime activity on empty days', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			OrderIndex: 0,
			AllowSwap: false,
			IsDayOff: false,
			IsEmptyDay: true,
			IsFullDayAbsence: false,
			ScheduleStartTime: '2018-01-23T00:00:00',
			ScheduleEndTime: '2018-01-23T23:59:00',
			SelectedAbsences: [],
			SelectedActivities: [],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: []
		}];

		fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);
		var result = setUp();

		expect(moment(result.commandControl.fromTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 08:00:00').format('YYYY-MM-DD HH:mm'));
		expect(moment(result.commandControl.toTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 17:00:00').format('YYYY-MM-DD HH:mm'));
	});

	it('should get default work time period when adding overtime activity on full day absence day', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			OrderIndex: 0,
			AllowSwap: false,
			IsDayOff: false,
			IsEmptyDay: false,
			IsFullDayAbsence: true,
			ScheduleStartTime: '2018-01-23T00:00:00',
			ScheduleEndTime: '2018-01-23T23:59:00',
			SelectedAbsences: ['8d97a1d1-45be-4447-96c6-9976e18c1664'],
			SelectedActivities: [],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: []
		}];

		fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);
		var result = setUp();

		expect(moment(result.commandControl.fromTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 08:00:00').format('YYYY-MM-DD HH:mm'));
		expect(moment(result.commandControl.toTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 17:00:00').format('YYYY-MM-DD HH:mm'));
	});

	it('should get default overtime period based on the upper most agent\'s schedule', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			OrderIndex: 1,
			AllowSwap: false,
			IsDayOff: false,
			IsEmptyDay: true,
			IsFullDayAbsence: false,
			ScheduleStartTime: '2018-01-23T00:00:00',
			ScheduleEndTime: '2018-01-23T23:59:00',
			SelectedAbsences: [],
			SelectedActivities: [],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: []
		}, {
			PersonId: 'c1a6918a-94ea-415d-9e34-26d96e469bd9',
			Name: 'Ashely',
			Checked: true,
			OrderIndex: 0,
			AllowSwap: false,
			IsDayOff: false,
			IsEmptyDay: false,
			IsFullDayAbsence: false,
			ScheduleStartTime: '2018-01-23T08:00:00',
			ScheduleEndTime: '2018-01-23T17:00:00',
			SelectedAbsences: [],
			SelectedActivities: ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: []
		}];

		fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);
		var result = setUp();

		expect(moment(result.commandControl.fromTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 17:00:00').format('YYYY-MM-DD HH:mm'));
		expect(moment(result.commandControl.toTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 18:00:00').format('YYYY-MM-DD HH:mm'));
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
			var personInfoList = [{
				PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
				Name: 'Bill',
				Checked: true,
				OrderIndex: 1,
				AllowSwap: false,
				IsDayOff: false,
				IsEmptyDay: true,
				IsFullDayAbsence: false,
				ScheduleStartTime: '2018-02-12T00:00:00',
				ScheduleEndTime: '2018-02-12T23:59:00',
				Schedules: [],
				SelectedAbsences: [],
				SelectedActivities: [],
				Timezone: {
					IanaId: "Asia/Shanghai",
					DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
				},
				SelectedDayOffs: []
			}];

			fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);

			var date = "2018-02-12";
			var result = setUp(date);
			var scope = result.scope;
			var ctrl = result.commandControl;
			var panel = result.container;

			ctrl.selectedActivityId = '472e02c8-1a84-4064-9a3b-9b5e015ab3c6';
			ctrl.selectedDefinitionSetId = '5c1409de-a0f1-4cd4-b383-9b5e015ab789';
			scope.$apply();

			ctrl.fromTime = '2018-02-12T10:00:00';
			ctrl.toTime = '2018-02-12T10:30:00';
			var timezone1 = {
				IanaId: "Asia/Hong_Kong",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			};
			ctrl.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('7c25f4ae-96ea-409e-b959-2c02587c649e', {
				Date: date,
				PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
				Timezone: timezone1,
				Shifts: [
					{
						Date: date,
						Projections: [
						],
						ProjectionTimeRange: null
					}]
			});

			scope.$apply();

			ctrl.addOvertime();

			expect(fakeActivityService.lastAddedOvertimeActivity.PersonDates[0].PersonId).toEqual(personInfoList[0].PersonId);
			expect(fakeActivityService.lastAddedOvertimeActivity.PersonDates[0].Date).toEqual(date);
			expect(fakeActivityService.lastAddedOvertimeActivity.ActivityId).toEqual('472e02c8-1a84-4064-9a3b-9b5e015ab3c6');
			expect(fakeActivityService.lastAddedOvertimeActivity.StartDateTime).toEqual('2018-02-12T10:00');
			expect(fakeActivityService.lastAddedOvertimeActivity.EndDateTime).toEqual('2018-02-12T10:30');
		});

		it('should apply with correct time range based on the selected time zone', function () {
			var personInfoList = [{
				PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
				Name: 'Bill',
				Checked: true,
				OrderIndex: 1,
				AllowSwap: false,
				IsDayOff: false,
				IsEmptyDay: true,
				IsFullDayAbsence: false,
				ScheduleStartTime: '2018-03-25T00:00:00',
				ScheduleEndTime: '2018-03-25T23:59:00',
				Schedules: [],
				SelectedAbsences: [],
				SelectedActivities: [],
				Timezone: {
					IanaId: "Asia/Shanghai",
					DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
				},
				SelectedDayOffs: []
			}];

			fakePersonSelectionService.setFakeCheckedPersonInfoList(personInfoList);

			var date = "2018-03-25";
			var result = setUp(date);
			var scope = result.scope;
			var ctrl = result.commandControl;
			var panel = result.container;

			ctrl.selectedActivityId = '472e02c8-1a84-4064-9a3b-9b5e015ab3c6';
			ctrl.selectedDefinitionSetId = '5c1409de-a0f1-4cd4-b383-9b5e015ab789';
			scope.$apply();

			ctrl.fromTime = '2018-03-25 01:00';
			ctrl.toTime = '2018-03-25 02:00';
			var timezone1 = {
				IanaId: "Asia/Hong_Kong",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			};
			ctrl.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('7c25f4ae-96ea-409e-b959-2c02587c649e', {
				Date: date,
				PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
				Timezone: timezone1,
				Shifts: [
					{
						Date: date,
						Projections: [
						],
						ProjectionTimeRange: null
					}]
			});

			scope.$apply();

			ctrl.addOvertime();
			
			expect(fakeActivityService.lastAddedOvertimeActivity.StartDateTime).toEqual('2018-03-25T01:00');
			expect(fakeActivityService.lastAddedOvertimeActivity.EndDateTime).toEqual('2018-03-25T02:00');

		});

	}

	function setUp(inputDate) {
		var date;
		var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2018-01-23').toDate();
		else
			date = inputDate;

		scope.curDate = date;
		scope.timezone = 'Asia/Shanghai';
		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var container = $compile(html)(scope);
		scope.$apply();

		var containerVm = container.isolateScope().vm;
		containerVm.setReady(true);
		containerVm.setActiveCmd('AddOvertimeActivity');
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector('.add-overtime-activity')).scope().$ctrl;

		return {
			container: container,
			commandControl: commandControl,
			scope: scope
		};
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

	function FakePersonSelectionService() {
		var fakePersonList = [];

		this.setFakeCheckedPersonInfoList = function (input) {
			fakePersonList = input;
		};

		this.getCheckedPersonInfoList = function () {
			return fakePersonList;
		};

		this.getSelectedPersonInfoList = function () {
			return fakePersonList;
		};
	}

	function FakeScheduleManagementService() {
		var savedPersonScheduleVm = {};

		this.setPersonScheduleVm = function (personId, vm) {
			savedPersonScheduleVm[personId] = vm;
		}

		this.findPersonScheduleVmForPersonId = function (personId) {
			return savedPersonScheduleVm[personId];
		}

		this.schedules = function () {
			return null;
		};

		this.newService = function () {
			return new FakeScheduleManagementService();
		};

		function FakeScheduleManagementService() {
			var savedPersonScheduleVm = {};

			this.setPersonScheduleVm = function (personId, vm) {
				savedPersonScheduleVm[personId] = vm;
			}

			this.findPersonScheduleVmForPersonId = function (personId) {
				return savedPersonScheduleVm[personId];
			}

			this.schedules = function () {
				return null;
			};
		}
	}

});