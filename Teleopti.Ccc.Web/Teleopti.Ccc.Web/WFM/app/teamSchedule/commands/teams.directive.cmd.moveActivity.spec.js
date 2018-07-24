﻿describe('teamschedule move activity directive tests', function () {
	'use strict';

	var $compile,
		$rootScope,
		fakeActivityService,
		fakeCommandCheckService,
		$httpBackend,
		scheduleHelper,
		fakePersonSelectionService,
		fakeNoticeService,
		utility,
		fakeMoveActivityValidator,
		serviceDateFormatHelper;

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
		fakeNoticeService = new FakeNoticeService();

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
			$provide.service('NoticeService',
				function () {
					return fakeNoticeService;
				});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_, UtilityService, _serviceDateFormatHelper_) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		utility = UtilityService;
		serviceDateFormatHelper = _serviceDateFormatHelper_;

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
		vm.updateInvalidAgents();
		result.scope.$apply();
		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));

		expect(applyButton.hasClass('wfm-btn-primary-disabled')).toBeTruthy();
		expect(applyButton.attr('disabled')).toBe('disabled');
		expect(vm.anyValidAgent()).toBe(false);
	});

	it('should show warning and error message when move activity partially success', function () {
		var result = setUp(moment('2018-07-23').toDate());
		var vm = result.commandControl;
		vm.nextDay = false;
		vm.moveToTime = vm.getDefaultMoveToStartTime();

		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: '2018-07-23T08:00:00Z',
				ScheduleEndTime: '2018-07-23T17:00:00Z',
				SelectedActivities: [
					{ shiftLayerId: "4b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" },
					{ shiftLayerId: "5b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" }]
			},
			{
				PersonId: 'agent2',
				Name: 'agent2',
				ScheduleStartTime: '2018-07-23T08:00:00Z',
				ScheduleEndTime: '2018-07-23T17:00:00Z',
				SelectedActivities: [{ shiftLayerId: "6b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" }]
			}];

		result.scope.$apply();

		fakeActivityService.setSavingApplyResponseData([{
			PersonId: vm.selectedAgents[0].PersonId, ErrorMessages: ['CanNotMoveMultipleActivitiesForSelectedAgents']
		}]);
		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(fakeNoticeService.warningMessage).toEqual('PartialSuccessMessageForMovingActivity');
		expect(fakeNoticeService.errorMessage).toEqual('CanNotMoveMultipleActivitiesForSelectedAgents : agent1');
	});

	it('should show success message and invoke action callback after move activity successfully', function () {
		var date = moment('2018-07-23');

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
				ScheduleStartTime: '2018-07-23T08:00:00Z',
				ScheduleEndTime: '2018-07-23T17:00:00Z',
				SelectedActivities: [{ shiftLayerId: "4b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" }]
			},
			{
				PersonId: 'agent2',
				Name: 'agent2',
				ScheduleStartTime: '2018-07-23T08:00:00Z',
				ScheduleEndTime: '2018-07-23T17:00:00Z',
				SelectedActivities: [{ shiftLayerId: "6b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" }]
			}];

		result.scope.$apply();

		fakeActivityService.setSavingApplyResponseData([]);

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(cbMonitor).toBeTruthy();
		expect(fakeNoticeService.successMessage).toEqual('SuccessfulMessageForMovingActivity');
	});

	it('should have later default start time than previous day over night shift end', function () {
		var date = moment(utility.nowInUserTimeZone()).add(7, 'day');

		scheduleHelper.setLatestEndTime(date.clone().hour(10).toDate());
		scheduleHelper.setLatestStartTime(date.clone().hour(9).toDate());

		var result = setUp(date.toDate());
		var vm = result.commandControl;

		var defaultMoveToStartTime = vm.getDefaultMoveToStartTime();

		expect(moment(defaultMoveToStartTime).hours()).toBe(11);
	});

	it('should move activity with correct data', function () {
		var result = setUp(moment('2018-07-23').toDate());
		var vm = result.commandControl;
		vm.nextDay = false;
		vm.moveToTime = vm.getDefaultMoveToStartTime();

		vm.selectedAgents = [
			{
				PersonId: 'agent1',
				Name: 'agent1',
				ScheduleStartTime: '2018-07-23T08:00:00Z',
				ScheduleEndTime: '2018-07-23T17:00:00Z',
				SelectedActivities: [{ shiftLayerId: "4b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" },
				{ shiftLayerId: "5b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" }]
			},
			{
				PersonId: 'agent2',
				Name: 'agent2',
				ScheduleStartTime: '2018-07-23T08:00:00Z',
				ScheduleEndTime: '2018-07-23T17:00:00Z',
				SelectedActivities: [{ shiftLayerId: "6b132007-41f8-4f05-85a9-a927001434a6", date: "2018-07-23" }]
			}];


		result.scope.$apply();

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		var requestData = fakeActivityService.getMoveActivityCalledWith();
		expect(requestData.PersonActivities[0].Date).toEqual("2018-07-23");
		expect(requestData.PersonActivities[0].PersonId).toEqual('agent1');
		expect(requestData.PersonActivities[0].ShiftLayerIds).toEqual(['4b132007-41f8-4f05-85a9-a927001434a6', '5b132007-41f8-4f05-85a9-a927001434a6']);

		expect(requestData.PersonActivities[1].Date).toEqual("2018-07-23");
		expect(requestData.PersonActivities[1].PersonId).toEqual('agent2');
		expect(requestData.PersonActivities[1].ShiftLayerIds).toEqual(['6b132007-41f8-4f05-85a9-a927001434a6']);

		expect(requestData.TrackedCommandInfo.TrackId).toEqual(vm.trackId);
	});

	function commonTestsInDifferentLocale() {
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
			expect(activityData.StartTime).toEqual('2016-06-15T19:00');
			expect(activityData.PersonActivities[0].Date).toEqual('2016-06-15');
			expect(activityData.TrackedCommandInfo.TrackId).toBe(vm.trackId);
		});

		it('should get correct move start time when switch next day', function () {
			var result = setUp('2018-03-05');
			var timePickerCtrl = angular.element(result.commandElement[0].querySelector('teams-time-picker')).isolateScope().$ctrl;
			var vm = result.commandControl;

			timePickerCtrl.dateTimeObj = moment(timePickerCtrl.dateTimeObj).hours(10).minutes(30);
			vm.nextDay = false;
			result.scope.$apply();

			var nextDayEl = result.container[0].querySelector('md-switch');
			nextDayEl.click();
			result.scope.$apply();
			expect(vm.moveToTime).toEqual('2018-03-06 10:30')

			nextDayEl.click();
			result.scope.$apply();
			expect(vm.moveToTime).toEqual('2018-03-05 10:30')
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
		beforeAll(function () {
			moment.locale('fa-IR');
		});

		afterAll(function () {
			moment.locale('en');
		});

		commonTestsInDifferentLocale();
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
		var commandElement = angular.element(container[0].querySelector(".move-activity"));
		var commandControl = commandElement.scope().vm;

		var obj = {
			container: container,
			commandControl: commandControl,
			commandElement: commandElement,
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

		this.setSavingApplyResponseData = function (data) {
			fakeResponse.data = data;
		}

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

	function FakePersonSelectionService() {
		var fakePersonList = [];

		this.setFakeSelectedPersonInfoList = function (input) {
			fakePersonList = input;
		}

		this.getSelectedPersonInfoList = function () {
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
	function FakeNoticeService() {
		this.successMessage = '';
		this.errorMessage = '';
		this.warningMessage = '';
		this.success = function (message, time, destroyOnStateChange) {
			this.successMessage = message;
		}
		this.error = function (message, time, destroyOnStateChange) {
			this.errorMessage = message;
		}
		this.warning = function (message, time, destroyOnStateChange) {
			this.warningMessage = message;
		}

	}
});