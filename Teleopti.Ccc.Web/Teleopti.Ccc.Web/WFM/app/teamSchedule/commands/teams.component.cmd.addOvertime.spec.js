describe('teamschedule add overtime activity directive tests', function () {
	'use strict';

	var $compile,
		$rootScope,
		$httpBackend,
		utility,
		fakeActivityService,
		fakePersonSelectionService,
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
			AllowSwap: false,
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

	it('should get default start time for one agent', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			AllowSwap: false,
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

	it('should use first selected agent\'s shift end time as default start time', function () {
		var personInfoList = [{
			PersonId: '7c25f4ae-96ea-409e-b959-2c02587c649e',
			Name: 'Bill',
			Checked: true,
			AllowSwap: false,
			ScheduleStartTime: '2018-01-23T08:00:00',
			ScheduleEndTime: '2018-01-23T16:00:00',
			SelectedAbsences: [],
			SelectedActivities: ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'],
			Timezone: {
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			},
			SelectedDayOffs: []
		}, {
			PersonId: '56a1fbbc-61f1-4dd8-9a5b-45b98d9aa72f',
			Name: 'John',
			Checked: true,
			AllowSwap: false,
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

		expect(moment(result.commandControl.fromTime).format('YYYY-MM-DD HH:mm')).toEqual(moment('2018-01-23 16:00:00').format('YYYY-MM-DD HH:mm'));
	});

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

});