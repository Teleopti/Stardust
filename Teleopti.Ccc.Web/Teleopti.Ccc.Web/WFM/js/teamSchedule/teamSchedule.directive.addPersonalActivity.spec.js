describe("teamschedule add personal activity directive test", function () {

	var $compile,
		$rootScope,
		fakeActivityService,
		WFMDate,
		fakeScheduleManagementSvc;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		fakeActivityService = new FakeActivityService();
		fakeScheduleManagementSvc = new FakeScheduleManagementService();

		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
			$provide.service('ScheduleManagement', function () {
				return fakeScheduleManagementSvc;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_, _WFMDate_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		WFMDate = _WFMDate_;
		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));

	it('add-personal-activity should render correctly', function () {

		var html = '<teamschedule-command-container><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();

		var target = $compile(html)(scope);

		scope.$apply();
		var result = target[0].querySelector('.add-personal-activity');
		expect(result).not.toBeNull();
	});

	it('add-personal-activity should get date from container', function () {

		var date = moment('2016-06-15').toDate();
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		var target = $compile(html)(scope);

		scope.$apply();

		var vm = angular.element(target[0].querySelector(".add-personal-activity")).scope().vm;

		expect(moment(vm.referenceDay()).format('YYYY-MM-DD')).toBe('2016-06-15');
	});

	it('should load activity list', function () {
		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);
		scope.$apply();

		var result = target[0].querySelectorAll('.activity-selector .activity-option-item');

		expect(result.length).toBe(5);
	});

	it('should see a disabled button when no activity selected', function () {
		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);
		scope.$apply();

		var applyButton = angular.element(target[0].querySelector(".add-personal-activity .form-submit"));
		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should see a disabled button when time range input is invalid', function () {
		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);
		scope.$apply();

		var vm = angular.element(target[0].querySelector(".add-personal-activity")).scope().vm;

		vm.timeRange = {
			startTime: new Date('2016-06-15T08:00:00Z'),
			endTime: new Date('2016-06-15T07:00:00Z')
		};

		scope.$apply();

		var applyButton = angular.element(target[0].querySelector(".add-personal-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should not allow to add personal activity if time range is not correct', function () {
		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);
		scope.$apply();

		var vm = angular.element(target[0].querySelector(".add-personal-activity")).scope().vm;

		vm.isNextDay = true;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: new Date('2016-06-16T08:00:00Z'),
			endTime: new Date('2016-06-16T16:00:00Z')
		};
		var agent = {
			personId: 'agent1',
			name: 'agent1',
			scheduleEndTime: '2016-06-15T17:00:00Z'
		};

		expect(vm.isNewActivityAllowedForAgent(agent, vm.timeRange.startTime)).toBe(false);
	});

	it('should see a disabled button when anyone in selected is not allowed to add current activity', function () {
		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);
		scope.$apply();

		var vm = angular.element(target[0].querySelector(".add-personal-activity")).scope().vm;

		vm.isNextDay = true;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: new Date('2016-06-16T08:00:00Z'),
			endTime: new Date('2016-06-16T16:00:00Z')
		};

		vm.selectedAgents = [
			{
				personId: 'agent1',
				name: 'agent1',
				scheduleEndTime: '2016-06-15T17:00:00Z'
			}, {
				personId: 'agent2',
				name: 'agent2',
				scheduleEndTime: '2016-06-16T09:00:00Z'
			}];

		var applyButton = angular.element(target[0].querySelector(".add-personal-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
		expect(vm.isInputValid()).toBe(false);
	});

	it('should call add personal activity when click apply with correct data', inject(function (ReloadScheduleEvent) {
		var date = new Date('2016-06-15T00:00:00Z');
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;
		scope.eventMonitor = null;
		scope.$on(ReloadScheduleEvent, function (event, data) {
			scope.eventMonitor = data.personIds;
		});
	
		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);

		scope.$apply();

		var monitorInCb = 0;
		function actionCb() {
			monitorInCb = 1;
		}

		target.isolateScope().vm.setActionCb('AddPersonalActivity', actionCb);

		var vm = angular.element(target[0].querySelector(".add-personal-activity")).scope().vm;

		vm.isNextDay = false;
		vm.disableNextDay = false;
		vm.timeRange = {
			startTime: new Date('2016-06-15T08:00:00Z'),
			endTime: new Date('2016-06-15T16:00:00Z')
		};

		vm.selectedAgents = [
			{
				personId: 'agent1',
				name: 'agent1',
				scheduleEndTime: '2016-06-15T17:00:00Z'
			}, {
				personId: 'agent2',
				name: 'agent2',
				scheduleEndTime: '2016-06-15T17:00:00Z'
			}];

		vm.selectedActivityId = '472e02c8-1a84-4064-9a3b-9b5e015ab3c6';

		scope.$apply();

		var applyButton = angular.element(target[0].querySelector(".add-personal-activity .form-submit"));
		applyButton.triggerHandler('click');

		scope.$apply();

		var activityData = fakeActivityService.getAddActivityCalledWith();
		expect(activityData).not.toBeNull();
		expect(activityData.PersonIds.length).toEqual(vm.selectedAgents.length);
		expect(activityData.ActivityId).toEqual(vm.selectedActivityId);
		expect(moment(activityData.StartTime).format('YYYY-MM-DDTHH:mm:00')).toEqual(moment(vm.timeRange.startTime).format('YYYY-MM-DDTHH:mm:00'));
		expect(moment(activityData.EndTime).format('YYYY-MM-DDTHH:mm:00')).toEqual(moment(vm.timeRange.endTime).format('YYYY-MM-DDTHH:mm:00'));
		expect(activityData.Date).toEqual(vm.referenceDay());
		expect(activityData.TrackedCommandInfo.TrackId).toBe(vm.trackId);
		expect(monitorInCb).toEqual(1);
		expect(scope.eventMonitor.length).toEqual(vm.selectedAgents.length);
	}));

	it('should have correct default start time when no other shifts', function () {

		var date = new Date(WFMDate.nowInUserTimeZone());

		fakeScheduleManagementSvc.setLatestEndTime(null);
		fakeScheduleManagementSvc.setLatestStartTime(null);

	
		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = date;

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);

		scope.$apply();

		var vm = angular.element(target[0].querySelector(".add-personal-activity")).scope().vm;


		vm.selectedAgents = [];

		var result = vm.getDefaultActvityStartTime();

		var nextTick = new Date(WFMDate.getNextTick());


		expect(result.getHours()).toBe(nextTick.getHours());
		expect(result.getMinutes()).toBe(nextTick.getMinutes());
	});

	it('should have later default start time than previous day over night shift end', function () {
		var date = moment(WFMDate.nowInUserTimeZone()).add(7, 'day');

		fakeScheduleManagementSvc.setLatestEndTime(date.clone().hour(10).toDate());
		fakeScheduleManagementSvc.setLatestStartTime(date.clone().hour(9).toDate());

		var html = '<teamschedule-command-container date="curDate"><add-personal-activity></add-personal-activity></teamschedule-command-container>';
		var scope = $rootScope.$new();
		scope.curDate = new Date(WFMDate.nowInUserTimeZone());

		fakeActivityService.setAvailableActivities(getAvailableActivities());

		var target = $compile(html)(scope);
		scope.$apply();
		var vm = angular.element(target[0].querySelector(".add-personal-activity")).scope().vm;
		var result = vm.getDefaultActvityStartTime();
		expect(result.getHours()).toBe(11);
	});

	function FakeScheduleManagementService() {
		var latestEndTime = null;
		var latestStartTime = null;

		this.setLatestEndTime = function (date) {
			latestEndTime = date;
		}

		this.setLatestStartTime = function (date) {
			latestStartTime = date;
		}

		this.getLatestPreviousDayOvernightShiftEnd = function () {
			return latestEndTime;
		}

		this.getLatestStartOfSelectedSchedule = function () {
			return latestStartTime;
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

		this.addPersonalActivity = function (input) {
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