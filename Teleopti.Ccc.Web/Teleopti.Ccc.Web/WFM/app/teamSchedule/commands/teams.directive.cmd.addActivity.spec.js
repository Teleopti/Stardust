describe('teamschedule add activity directive test', function () {
	'use strict';

	var $compile,
		$rootScope,
		$httpBackend,
		utility,
		fakeActivityService,
		fakeScheduleManagementSvc,
		fakePersonSelectionService,
		fakeCommandCheckService,
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
		fakeScheduleManagementSvc = new FakeScheduleManagementService();
		fakePersonSelectionService = new FakePersonSelectionService();
		fakeCommandCheckService = new FakeCommandCheckService();
		scheduleHelper = new FakeScheduleHelper();

		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
			$provide.service('ScheduleManagement', function () {
				return fakeScheduleManagementSvc;
			});
			$provide.service('ScheduleHelper', function () {
				return scheduleHelper;
			});
			$provide.service('PersonSelection', function () {
				return fakePersonSelectionService;
			});
			$provide.service('CommandCheckService', function () {
				return fakeCommandCheckService;
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

	it('add-activity should render correctly', function () {
		var result = setUp();

		expect(result.commandControl).not.toBeNull();
	});

	it('add-activity should get date from container', function () {
		var result = setUp();

		expect(moment(result.commandControl.selectedDate()).format('YYYY-MM-DD')).toBe('2016-06-15');
	});

	it('should load activity list', function () {
		var result = setUp();

		var activities = result.container[0].querySelectorAll('.add-activity .activity-selector md-option');

		expect(activities.length).toBe(5);
	});

	it('should see a disabled button when no activity selected', function () {
		var result = setUp();

		var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should see a disabled button when time range input is invalid', function () {
		var result = setUp();

		result.commandControl.timeRange = {
			startTime: new Date('2016-06-15T08:00:00Z'),
			endTime: new Date('2016-06-15T07:00:00Z')
		};

		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should not allow to add activity if time range is not correct', function () {
		var result = setUp();

		var vm = result.commandControl;

		vm.isNextDay = true;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: new Date('2016-06-16T08:00:00Z'),
			endTime: new Date('2016-06-16T17:00:00Z')
		};

		var agent = {
			PersonId: 'agent1',
			Name: 'agent1',
			ScheduleStartTime: new Date('2016-06-15T08:00:00Z'),
			ScheduleEndTime: new Date('2016-06-15T17:00:00Z')
		};

		expect(vm.isNewActivityAllowedForAgent(agent, vm.timeRange)).toBe(false);
	});

	it('should not allow to add activity if agents shift exceed 36 hours', function () {
		var result = setUp();

		var vm = result.commandControl;

		vm.isNextDay = true;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: new Date('2016-06-16T07:00:00Z'),
			endTime: new Date('2016-06-16T12:01:00Z')
		};
		var agent = {
			PersonId: 'agent1',
			Name: 'agent1',
			ScheduleStartTime: new Date('2016-06-15T00:00:00Z'),
			ScheduleEndTime: new Date('2016-06-16T08:00:00Z')
		};

		expect(vm.isNewActivityAllowedForAgent(agent, vm.timeRange)).toBe(false);
	});

	it('should see a disabled button when everyone in selected is not allowed to add current activity', function () {
		var result = setUp();

		var vm = result.commandControl;
		vm.manageScheduleForDistantTimezonesEnabled = true;
		vm.isNextDay = true;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: new Date('2016-06-16T18:00:00Z'),
			endTime: new Date('2016-06-16T19:00:00Z')
		};

		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: new Date('2016-06-15T08:00:00Z'),
				ScheduleEndTime: new Date('2016-06-15T17:00:00Z')
			}];

		var timezone1 = {
			IanaId: "Asia/Shanghai",
			DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
		};

		vm.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('agent1', {
			Date: '2016-06-15',
			PersonId: 'agent1',
			Timezone: timezone1,
			Shifts: [
			{
				Date: '2016-06-15',
				Projections: [
				{
					Start: '2016-06-15 08:00',
					End: '2016-06-15 17:00',
					Minutes: 540
				}],
				ProjectionTimeRange: {
					Start: '2016-06-15 08:00',
					End: '2016-06-15 17:00',
				}
			}]
		});

		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
		expect(vm.anyValidAgent()).toBe(false);
	});

	it('should call add activity when click apply with correct data', function () {
		var result = setUp();
		var vm = result.commandControl;
		vm.manageScheduleForDistantTimezonesEnabled = true;
		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: moment('2016-06-15 08:00').toDate(),
			endTime:  moment('2016-06-15 16:00').toDate()
		};

		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: null,
				ScheduleEndTime: null
			}];

		vm.selectedActivityId = '472e02c8-1a84-4064-9a3b-9b5e015ab3c6';

		fakePersonSelectionService.setFakeCheckedPersonInfoList(vm.selectedAgents);

		var timezone1 = {
			IanaId: "Asia/Hong_Kong",
			DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
		};

		vm.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('agent1', {
			Date: '2016-06-15',
			PersonId: 'agent1',
			Timezone: timezone1,
			Shifts: [
			{
				Date: '2016-06-15',
				Projections: [
				],
				ProjectionTimeRange: null
			}]
		});

		result.scope.$apply();


		var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		var activityData = fakeActivityService.getAddActivityCalledWith();
		expect(activityData).not.toBeNull();
		expect(activityData.PersonDates.length).toEqual(vm.selectedAgents.length);
		expect(activityData.ActivityId).toEqual(vm.selectedActivityId);
		expect(moment(activityData.StartTime).format('YYYY-MM-DDTHH:mm')).toEqual('2016-06-15T08:00');
		expect(moment(activityData.EndTime).format('YYYY-MM-DDTHH:mm')).toEqual('2016-06-15T16:00');

		expect(activityData.TrackedCommandInfo.TrackId).toBe(vm.trackId);
	});

	it('should invoke action callback after calling add activity', function () {
		var result = setUp();

		var cbMonitor = null;
		function actionCb() {
			cbMonitor = true;
		}

		result.container.isolateScope().vm.setActionCb('AddActivity', actionCb);

		var vm = result.commandControl;

		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: moment('2016-06-15 08:00').toDate(),
			endTime: moment('2016-06-15 16:00').toDate()
		};

		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: null,
				ScheduleEndTime: null,
			}];

		var timezone1 = {
			IanaId: "Asia/Hong_Kong",
			DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
		};

		vm.containerCtrl.scheduleManagementSvc.setPersonScheduleVm('agent1', {
			Date: '2016-06-15',
			PersonId: 'agent1',
			Timezone: timezone1,
			Shifts: [
			{
				Date: '2016-06-15',
				Projections: [
				],
				ProjectionTimeRange: null
			}]
		});

		result.scope.$apply();


		vm.selectedActivityId = '472e02c8-1a84-4064-9a3b-9b5e015ab3c6';

		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".add-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(cbMonitor).toBeTruthy();
	});


	it('should have correct default start time when no other shifts on today', function () {
		var date = new Date(utility.nowInUserTimeZone());
		scheduleHelper.setLatestEndTime(null);
		scheduleHelper.setLatestStartTime(null);

		var result = setUp(date);
		var vm = result.commandControl;
		vm.selectedAgents = [];

		var defaultStartTime = vm.getDefaultActvityStartTime();
		var nextTick = new Date(utility.getNextTickNoEarlierThanEight());
		expect(defaultStartTime.getHours()).toBe(nextTick.getHours());
		expect(defaultStartTime.getMinutes()).toBe(nextTick.getMinutes());
	});

	it('should have correct default start time when no other shifts on selected date which is not today', function () {
		var today = new Date(moment(utility.nowInUserTimeZone()).add(1, 'day'));
		scheduleHelper.setLatestEndTime(null);
		scheduleHelper.setLatestStartTime(null);

		var result = setUp(today);
		var vm = result.commandControl;
		vm.selectedAgents = [];

		var defaultStartTime = vm.getDefaultActvityStartTime();
		var defaultEndTime = vm.getDefaultActvityEndTime();
		expect(defaultStartTime.getHours()).toBe(8);
		expect(defaultStartTime.getMinutes()).toBe(0);
		expect(defaultEndTime.getHours()).toBe(9);
		expect(defaultEndTime.getMinutes()).toBe(0);
	});

	it('should have later default start time than previous day over night shift end', function () {
		var date = moment(utility.nowInUserTimeZone()).add(7, 'day');

		scheduleHelper.setLatestEndTime(date.clone().hour(10).toDate());
		scheduleHelper.setLatestStartTime(date.clone().hour(9).toDate());

		var result = setUp(date);
		var vm = result.commandControl;

		var defaultStartTime = vm.getDefaultActvityStartTime();
		expect(defaultStartTime.getHours()).toBe(11);
	});

	function setUp(inputDate) {
		var date;
		var html = '<teamschedule-command-container date="curDate" timezone="timezone"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;

		scope.curDate = date;
		scope.timezone = "Asia/Hong_Kong";
		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		vm.setReady(true);
		vm.setActiveCmd('AddActivity');
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector(".add-activity")).scope().vm;

		var obj = {
			container: container,
			commandControl: commandControl,
			scope: scope
		};

		return obj;
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

	function FakeScheduleHelper() {
		var earliestStartTime = null;
		var latestStartTime = null;
		var latestEndTime = null;

		this.setEarliestStartTime = function(date) {
			earliestStartTime = date;
		};

		this.setLatestStartTime = function(date) {
			latestStartTime = date;
		};

		this.setLatestEndTime = function (date) {
			latestEndTime = date;
		};

		this.getEarliestStartOfSelectedSchedules = function() {
			return earliestStartTime;
		};

		this.getLatestStartOfSelectedSchedules = function() {
			return latestStartTime;
		};

		this.getLatestPreviousDayOvernightShiftEnd = function () {
			return latestEndTime;
		};
	}

	function FakeCommandCheckService() {
		var fakeResponse = {
			data: []
		};
		var checkStatus = false,
		fakeOverlappingList = [];

		this.checkOverlappingCertainActivities = function() {
			return {
				then: function(cb) {
					checkStatus = true;
					cb(fakeResponse);
				}
			}
		}

		this.getCommandCheckStatus = function() {
			return checkStatus;
		}

		this.resetCommandCheckStatus = function() {
			checkStatus = false;
		}

		this.getCheckFailedList = function() {
			return fakeOverlappingList;
		}
		this.checkAddActivityOverlapping = function(requestedData){
			return {
				then: function (cb) {
					cb(requestedData);
				}
			}
		};
	}

	function FakePersonSelectionService(){
		var fakePersonList = [];

		this.setFakeCheckedPersonInfoList = function(input){
			fakePersonList = input;
		}

		this.getCheckedPersonInfoList = function(){
			return fakePersonList;
		}
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
});