"use strict";

describe("teamschedule move activity directive tests", function () {

	var $compile,
		$rootScope,
		fakeActivityService,
		$httpBackend,
		WFMDate,
		fakeScheduleManagementSvc,
		fakeMoveActivityValidator,
		PersonSelection;

	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		fakeActivityService = new FakeActivityService();
		fakeScheduleManagementSvc = new FakeScheduleManagementService();
		fakeMoveActivityValidator = new FakeMoveActivityValidator();

		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
			$provide.service('ScheduleManagement', function () {
				return fakeScheduleManagementSvc;
			});
			$provide.service('MoveActivityValidator', function () {
				return fakeMoveActivityValidator;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_, _WFMDate_, _PersonSelection_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		WFMDate = _WFMDate_;

		PersonSelection = _PersonSelection_;
		PersonSelection.clearPersonInfo();
		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));

	it('move-activity should render correctly', function () {
		var result = setUp();

		expect(result.commandControl).not.toBeNull();
	});

	it('move-activity should get date from container', function () {
		var result = setUp();

		expect(moment(result.commandControl.selectedDate()).format('YYYY-MM-DD')).toBe('2016-06-15');
	});

	it('should see a disabled button when no activity selected', function () {
		var result = setUp();

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should see a disabled button when default start time input is invalid', function () {
		var result = setUp(moment('2016-06-15').toDate());

		result.commandControl.moveToTime = new Date('2016-06-14');

		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
	});

	it('should not allow to move activity if move to time is not correct', function () {
		var vm = setUp(moment('2016-06-15').toDate()).commandControl;

		vm.moveToTime = new Date('2016-06-14');
		vm.selectedAgents = [{
			personId: 'agent1',
			name: 'agent1',
			scheduleStartTime: '2016-06-15T08:00:00Z',
			scheduleEndTime: '2016-06-15T17:00:00Z',
			selectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
		}];

		expect(vm.isInputValid()).toBe(false);
	});

	it('should see a disabled button when anyone in selected is not allowed to move activity to the given time', function () {
		var result = setUp(moment('2016-06-15').toDate());

		var vm = result.commandControl;

		vm.nextDay = false;
		vm.moveToTime = new Date('2016-06-16T09:00:00Z');

		vm.selectedAgents = [
			{
				personId: 'agent1',
				name: 'agent1',
				scheduleStartTime: '2016-06-15T08:00:00Z',
				scheduleEndTime: '2016-06-15T17:00:00Z',
				selectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			}, {
				personId: 'agent2',
				name: 'agent2',
				scheduleStartTime: '2016-06-15T19:00:00Z',
				scheduleEndTime: '2016-06-16T08:00:00Z',
				selectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			}];

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
		expect(vm.isInputValid()).toBe(false);
	});

	it('should call move activity when click apply with correct data', function () {
		var date = moment('2016-06-15');

		var result = setUp(date.toDate());
		var vm = result.commandControl;

		var selectedActivities = ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'];

		fakeScheduleManagementSvc.setLatestStartTime(date.clone().hour(10).toDate());

		vm.nextDay = false;
		vm.moveToTime = vm.getDefaultMoveToStartTime();

		vm.selectedAgents = [
			{
				personId: 'agent1',
				name: 'agent1',
				scheduleStartTime: '2016-06-15T08:00:00Z',
				scheduleEndTime: '2016-06-15T17:00:00Z',
				selectedActivities: selectedActivities
			}, {
				personId: 'agent2',
				name: 'agent2',
				scheduleStartTime: '2016-06-15T09:00:00Z',
				scheduleEndTime: '2016-06-15T18:00:00Z',
				selectedActivities: selectedActivities
			}];

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		var activityData = fakeActivityService.getMoveActivityCalledWith();
		expect(activityData).not.toBeNull();
		expect(activityData.PersonActivities.length).toEqual(vm.selectedAgents.length);
		expect(moment(activityData.StartTime).format('YYYY-MM-DDTHH:mm:00')).toEqual(moment(vm.moveToTime).format('YYYY-MM-DDTHH:mm:00'));
		expect(activityData.Date).toEqual(vm.selectedDate());
		expect(activityData.TrackedCommandInfo.TrackId).toBe(vm.trackId);
	});

	it('should invoke action callback after calling move activity', function () {
		var date = moment('2016-06-15');

		var result = setUp(date.toDate());

		var cbMonitor = null;
		function actionCb() {
			cbMonitor = true;
		}

		result.container.isolateScope().vm.setActionCb('MoveActivity', actionCb);
		var vm = result.commandControl;

		var selectedActivities = ['472e02c8-1a84-4064-9a3b-9b5e015ab3c6'];

		vm.nextDay = false;
		vm.moveToTime = vm.getDefaultMoveToStartTime();

		vm.selectedAgents = [
			{
				personId: 'agent1',
				name: 'agent1',
				scheduleStartTime: '2016-06-15T08:00:00Z',
				scheduleEndTime: '2016-06-15T17:00:00Z',
				selectedActivities: selectedActivities
			}, {
				personId: 'agent2',
				name: 'agent2',
				scheduleStartTime: '2016-06-15T09:00:00Z',
				scheduleEndTime: '2016-06-15T18:00:00Z',
				selectedActivities: selectedActivities
			}];

		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(cbMonitor).toBeTruthy();
	});

	it('should have later default start time than previous day over night shift end', function () {
		var date = moment(WFMDate.nowInUserTimeZone()).add(7, 'day');

		fakeScheduleManagementSvc.setLatestEndTime(date.clone().hour(10).toDate());
		fakeScheduleManagementSvc.setLatestStartTime(date.clone().hour(9).toDate());

		var result = setUp(date.toDate());
		var vm = result.commandControl;

		var defaultMoveToStartTime = vm.getDefaultMoveToStartTime();

		expect(defaultMoveToStartTime.getHours()).toBe(11);
	});

	function setUp(inputDate) {
		var date;
		var html = '<teamschedule-command-container date="curDate"></teamschedule-command-container>';
		var scope = $rootScope.$new();

		if (inputDate == null)
			date = moment('2016-06-15').toDate();
		else
			date = inputDate;

		scope.curDate = date;

		var container = $compile(html)(scope);
		scope.$apply();

		container.isolateScope().vm.setActiveCmd('MoveActivity');
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector(".move-activity")).scope().vm;

		var obj = {
			container: container,
			commandControl: commandControl,
			scope: scope
		};

		return obj;
	}

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

		this.getLatestStartTimeOfSelectedScheduleProjection = function () {
			return latestStartTime;
		}

		this.getLatestStartOfSelectedSchedule = function () {
			return latestStartTime;
		}
	}

	function FakeActivityService() {
		var targetActivity = null;
		var fakeResponse = { data: [] };

		this.moveActivity = function (input) {
			targetActivity = input;
			return {
				then: (function (cb) {
					cb(fakeResponse);
				})
			};
		}

		this.getMoveActivityCalledWith = function () {
			return targetActivity;
		};
	}

	function FakeMoveActivityValidator() {
		var validate = false;
		var invalidPeople = [];

		this.setInvalidPeople = function (input) {
			return invalidPeople = input;
		}

		this.getInvalidPeople = function () {
			return invalidPeople;
		}

		this.setValidateMoveToTime = function (input) {
			return validate = input;
		}

		this.validateMoveToTime = function () {
			return validate;
		}
	};
});