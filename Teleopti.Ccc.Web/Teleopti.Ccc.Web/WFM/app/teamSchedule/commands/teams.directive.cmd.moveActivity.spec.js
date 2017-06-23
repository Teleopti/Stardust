describe('teamschedule move activity directive tests', function () {
	'use strict';

	var $compile,
		$rootScope,
		fakeActivityService,
		fakeCommandCheckService,
		$httpBackend,
		scheduleHelper,
		fakePersonSelectionService,
		utility,
		fakeMoveActivityValidator;

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
		scheduleHelper = new FakeScheduleHelper();
		fakePersonSelectionService = new FakePersonSelectionService();
		fakeMoveActivityValidator = new FakeMoveActivityValidator();

		module(function ($provide) {
			$provide.service('ActivityService', function () {
				return fakeActivityService;
			});
			$provide.service('CommandCheckService', function () {
				return fakeCommandCheckService;
			});
			$provide.service('ScheduleHelper', function () {
				return scheduleHelper;
			});
			$provide.service('PersonSelection', function () {
				return fakePersonSelectionService;
			});
			$provide.service('ActivityValidator', function () {
				return fakeMoveActivityValidator;
			});
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_, UtilityService) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		utility = UtilityService;

		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
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

	it('should see a disabled button when everyone in selected is not allowed to move activity to the given time', function () {
		var result = setUp(moment('2016-06-15').toDate());

		var vm = result.commandControl;

		vm.nextDay = false;
		vm.moveToTime = new Date('2016-06-16T09:00:00Z');

		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: '2016-06-15T08:00:00Z',
				ScheduleEndTime: '2016-06-15T17:00:00Z',
				SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			}, {
				PersonId: 'agent2',
				Name: 'agent2',
				ScheduleStartTime: '2016-06-15T19:00:00Z',
				ScheduleEndTime: '2016-06-16T08:00:00Z',
				SelectedActivities: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
			}];
		fakePersonSelectionService.setFakeSelectedPersonInfoList(vm.selectedAgents);
		fakeMoveActivityValidator.setInvalidPeople([
			{
				PersonId: 'agent1',
				Name: 'agent1'
			},
			{
				PersonId: 'agent2',
				Name: 'agent2'
			}
		]);
		result.scope.$apply();
		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
		expect(vm.anyValidAgent()).toBe(false);
	});

	it('should call move activity when click apply with correct data', function () {
		var date = moment('2016-06-15');

		var result = setUp(date.toDate());
		var vm = result.commandControl;

		var selectedActivities = {
			date: '2016-06-15',
			shiftLayerId: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6'
		};

		scheduleHelper.setLatestStartTime(date.clone().hour(10).toDate());

		vm.nextDay = false;
		vm.moveToTime = vm.getDefaultMoveToStartTime();

		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: '2016-06-15T08:00:00Z',
				ScheduleEndTime: '2016-06-15T17:00:00Z',
				SelectedActivities: [selectedActivities]
			}, {
				PersonId: 'agent2',
				Name: 'agent2',
				ScheduleStartTime: '2016-06-15T09:00:00Z',
				ScheduleEndTime: '2016-06-15T18:00:00Z',
				SelectedActivities: [selectedActivities]
			}];

		fakePersonSelectionService.setFakeSelectedPersonInfoList(vm.selectedAgents);

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		var activityData = fakeActivityService.getMoveActivityCalledWith();
		expect(activityData).not.toBeNull();
		expect(activityData.PersonActivities.length).toEqual(vm.selectedAgents.length);
		expect(moment(activityData.StartTime).format('YYYY-MM-DDTHH:mm:00')).toEqual(moment(vm.moveToTime).add(8, 'hours').format('YYYY-MM-DDTHH:mm:00'));
		expect(activityData.PersonActivities[0].Date).toEqual(moment(vm.selectedDate()).format('YYYY-MM-DD'));
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
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: '2016-06-15T08:00:00Z',
				ScheduleEndTime: '2016-06-15T17:00:00Z',
				SelectedActivities: selectedActivities
			}, {
				PersonId: 'agent2',
				Name: 'agent2',
				ScheduleStartTime: '2016-06-15T09:00:00Z',
				ScheduleEndTime: '2016-06-15T18:00:00Z',
				SelectedActivities: selectedActivities
			}];

		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(cbMonitor).toBeTruthy();
	});

	it('should have later default start time than previous day over night shift end', function () {
		var date = moment(utility.nowInUserTimeZone()).add(7, 'day');

		scheduleHelper.setLatestEndTime(date.clone().hour(10).toDate());
		scheduleHelper.setLatestStartTime(date.clone().hour(9).toDate());

		var result = setUp(date.toDate());
		var vm = result.commandControl;

		var defaultMoveToStartTime = vm.getDefaultMoveToStartTime();

		expect(defaultMoveToStartTime.getHours()).toBe(11);
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
		scope.timezone = "Etc/UTC";

		var container = $compile(html)(scope);
		scope.$apply();

		var vm = container.isolateScope().vm;
		vm.setReady(true);
		vm.setActiveCmd('MoveActivity');
		scope.$apply();

		var commandControl = angular.element(container[0].querySelector(".move-activity")).scope().vm;

		var obj = {
			container: container,
			commandControl: commandControl,
			scope: scope
		};

		return obj;
	}

	function FakeScheduleHelper() {
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

		this.getLatestStartTimeOfSelectedSchedulesProjections = function () {
			return latestStartTime;
		}
	}

	function FakeActivityService() {
		var targetActivity = {

		};
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

	function FakePersonSelectionService(){
		var fakePersonList = [];

		this.setFakeSelectedPersonInfoList = function(input){
			fakePersonList = input;
		}

		this.getSelectedPersonInfoList = function(){
			return fakePersonList;
		}
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
		this.checkMoveActivityOverlapping = function (requestedData) {
			return {
				then: function (cb) {
					cb(requestedData);
				}
			}
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

		this.getInvalidPeopleNameList = function () {
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