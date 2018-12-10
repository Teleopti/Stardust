describe('teamschedule move activity directive tests', function () {
	'use strict';

	var $compile,
		$rootScope,
		fakeActivityService,
		fakeCommandCheckService,
		$httpBackend,
		fakeNoticeService,
		scheduleManagement,
		personSelection;

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
		fakeNoticeService = new FakeNoticeService();

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
			$provide.service('NoticeService',
				function () {
					return fakeNoticeService;
				});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$compile_, _$httpBackend_,PersonSelection, ScheduleManagement) {
		$compile = _$compile_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		personSelection = PersonSelection;
		scheduleManagement = ScheduleManagement;
		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
	}));


	it('move-activity should get date from container', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2016-06-15',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2016-06-15 08:00',
					EndInUtc: '2016-06-15 16:00',
				}]
			}]
			, '2016-06-15', 'Etc/Utc');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2016-06-15');

		var result = setUp();
		expect(result.commandControl.selectedDate()).toBe('2016-06-15');
	});

	it('should see a disabled button when default start time input is invalid', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2016-06-15',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2016-06-15 23:00',
					EndInUtc: '2016-06-16 00:00',
					ShiftLayerIds: ['layer1']
				}]
			}]
			, '2016-06-15', 'Etc/Utc');
		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.IsSelected = true;
		personSelection.updatePersonSelection(personSchedule);
		personSelection.toggleAllPersonProjections(personSchedule, '2016-06-15');

		var result = setUp();
		var applyButton = result.container[0].querySelector(".move-activity .form-submit");
		expect(applyButton.disabled).toBe(true);
	});

	it('should not disable apply button and show error message for invalid agents when someone in selected is allowed to move activity to the given time', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2016-06-15',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2016-06-15 23:00',
					EndInUtc: '2016-06-16 00:00',
					ShiftLayerIds: ['layer1']
				}]
			},
			{
				Date: '2016-06-15',
				PersonId: 'agent2',
				Name: 'agent2',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2016-06-15 20:00',
					EndInUtc: '2016-06-16 22:00',
					ShiftLayerIds: ['layer2']
				},
				{
					StartInUtc: '2016-06-15 22:00',
					EndInUtc: '2016-06-16 02:00',
					ShiftLayerIds: ['layer3']
				}]
			}]
			, '2016-06-15', 'Etc/Utc');

		angular.forEach(scheduleManagement.groupScheduleVm.Schedules, function (personSchedule) {
			personSchedule.Shifts[0].Projections[0].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], '2016-06-15');
		});

		var result = setUp();
		var applyButton = result.container[0].querySelector(".move-activity .form-submit");
		expect(applyButton.disabled).toBe(false);
		expect(!!result.container[0].querySelector('.text-danger')).toBeTruthy();
	});

	it('should set default start time to later one hour than todays shift start', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2016-06-16',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [
					{
						StartInUtc: '2016-06-16 16:00',
						EndInUtc: '2016-06-16 18:00',
						ShiftLayerIds: ['layer2']
					}]
			}]
			, '2016-06-16', 'Etc/Utc');

		scheduleManagement.groupScheduleVm.Schedules.forEach(function (personSchedule) {
			personSchedule.Shifts[0].Projections[0].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], '2016-06-16');
		});

		var result = setUp('2016-06-16');
		var vm = result.commandControl;

		expect(vm.moveToTime).toBe('2016-06-16 17:00');
	});


	it('should get correct move start time when switch next day', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2016-06-15',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [{
					StartInUtc: '2016-06-15 08:00',
					EndInUtc: '2016-06-15 18:00',
					ShiftLayerIds: ['layer1']
				}]
			}]
			, '2016-06-15', 'Etc/Utc');

		angular.forEach(scheduleManagement.groupScheduleVm.Schedules, function (personSchedule) {
			personSchedule.Shifts[0].Projections[0].Selected = true;
			personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], '2016-06-15');
		});

		var result = setUp();
		var vm = result.commandControl;

		result.container[0].querySelector('md-switch').click();
		setTime(result.container, 10);
		result.scope.$apply();
		expect(vm.moveToTime).toEqual('2016-06-16 10:00')

		result.container[0].querySelector('md-switch').click();
		result.scope.$apply();
		expect(vm.moveToTime).toEqual('2016-06-15 10:00')
	});

	//TODO: review the validation of new start time
	xit('should disable apply button when the end of previous days shift is larger than todays shift start', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2016-06-16',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [
					{
						StartInUtc: '2016-06-16 10:00',
						EndInUtc: '2016-06-16 11:00',
						ShiftLayerIds: ['layer2']
					}]
			},
			{
				Date: '2016-06-15',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [
					{
						StartInUtc: '2016-06-15 23:00',
						EndInUtc: '2016-06-16 06:00',
						ShiftLayerIds: ['layer1']
					}]
			}]
			, '2016-06-16', 'Etc/Utc');

		var personSchedule = scheduleManagement.groupScheduleVm.Schedules[0];
		personSchedule.Shifts[0].Projections[0].Selected = true;
		personSelection.updatePersonProjectionSelection(personSchedule.Shifts[0].Projections[0], '2016-06-16');

		var result = setUp('2016-06-16');
		setTime(result.container, 5);

		var applyButton = result.container[0].querySelector(".move-activity .form-submit");
		expect(applyButton.disabled).toBe(true);
	});

	it('should show warning and error message when move activity partially success', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-07-23',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [
					{
						StartInUtc: '2018-07-23 16:00',
						EndInUtc: '2018-07-23 18:00',
						ShiftLayerIds: ['layer2']
					},
					{
						StartInUtc: '2018-07-23 18:00',
						EndInUtc: '2018-07-23 19:00',
						ShiftLayerIds: ['layer3']
					}]
			},
				{
					Date: '2018-07-23',
					PersonId: 'agent2',
					Name: 'agent2',
					Timezone: {
						IanaId: 'Etc/Utc'
					},
					Projection: [
						{
							StartInUtc: '2018-07-23 16:00',
							EndInUtc: '2018-07-23 18:00',
							ShiftLayerIds: ['layer2']
						}]
				}]
			, '2018-07-23', 'Etc/Utc');

		scheduleManagement.groupScheduleVm.Schedules.forEach(function (personSchedule) {
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-07-23');
		});

		fakeActivityService.setSavingApplyResponseData([{
			PersonId: 'agent1', ErrorMessages: ['CanNotMoveMultipleActivitiesForSelectedAgents']
		}]);

		var result = setUp('2018-07-23');

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(fakeNoticeService.warningMessage).toEqual('PartialSuccessMessageForMovingActivity');
		expect(fakeNoticeService.errorMessage).toEqual('CanNotMoveMultipleActivitiesForSelectedAgents : agent1');
	});

	it('should show success message and invoke action callback after move activity successfully', function () {
		scheduleManagement.resetSchedules(
			[{
				Date: '2018-07-23',
				PersonId: 'agent1',
				Name: 'agent1',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [
					{
						StartInUtc: '2018-07-23 16:00',
						EndInUtc: '2018-07-23 18:00',
						ShiftLayerIds: ['layer2']
					}]
			},
			{
				Date: '2018-07-23',
				PersonId: 'agent2',
				Name: 'agent2',
				Timezone: {
					IanaId: 'Etc/Utc'
				},
				Projection: [
					{
						StartInUtc: '2018-07-23 16:00',
						EndInUtc: '2018-07-23 18:00',
						ShiftLayerIds: ['layer2']
					}]
			}]
			, '2018-07-23', 'Etc/Utc');

		scheduleManagement.groupScheduleVm.Schedules.forEach(function (personSchedule) {
			personSchedule.IsSelected = true;
			personSelection.updatePersonSelection(personSchedule);
			personSelection.toggleAllPersonProjections(personSchedule, '2018-07-23');
		});

		var result = setUp('2018-07-23');

		var cbMonitor = null;
		function actionCb() {
			cbMonitor = true;
		}

		result.container.isolateScope().vm.setActionCb('MoveActivity', actionCb);
		result.scope.$apply();

		fakeActivityService.setSavingApplyResponseData([]);

		var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
		applyButton.triggerHandler('click');

		result.scope.$apply();

		expect(cbMonitor).toBeTruthy();
		expect(fakeNoticeService.successMessage).toEqual('SuccessfulMessageForMovingActivity');
	});

	function commonTestsInDifferentLocale() {
		it('should move activity with correct data', function () {
			scheduleManagement.resetSchedules(
				[{
					Date: '2018-07-23',
					PersonId: 'agent1',
					Name: 'agent1',
					Timezone: {
						IanaId: 'Etc/Utc'
					},
					Projection: [
						{
							StartInUtc: '2018-07-23 08:00',
							EndInUtc: '2018-07-23 17:00',
							ShiftLayerIds: ['layer1']
						}]
				},
				{
					Date: '2018-07-23',
					PersonId: 'agent2',
					Name: 'agent2',
					Timezone: {
						IanaId: 'Etc/Utc'
					},
					Projection: [
						{
							StartInUtc: '2018-07-23 08:00',
							EndInUtc: '2018-07-23 17:00',
							ShiftLayerIds: ['layer2']
						}]
				}]
				, '2018-07-23', 'Etc/Utc');

			scheduleManagement.groupScheduleVm.Schedules.forEach(function (personSchedule) {
				personSchedule.IsSelected = true;
				personSelection.updatePersonSelection(personSchedule);
				personSelection.toggleAllPersonProjections(personSchedule, '2018-07-23');
			});

			var result = setUp('2018-07-23');
			result.scope.$apply();

			var applyButton = angular.element(result.container[0].querySelector(".move-activity .form-submit"));
			applyButton.triggerHandler('click');

			result.scope.$apply();

			var requestData = fakeActivityService.getMoveActivityCalledWith();
			expect(requestData.StartTime).toEqual('2018-07-23T17:00');
			expect(requestData.PersonActivities.length).toEqual(2);
			expect(requestData.PersonActivities[0].Date).toEqual("2018-07-23");
			expect(requestData.PersonActivities[0].PersonId).toEqual('agent1');
			expect(requestData.PersonActivities[0].ShiftLayerIds).toEqual(['layer1']);

			expect(requestData.PersonActivities[1].Date).toEqual("2018-07-23");
			expect(requestData.PersonActivities[1].PersonId).toEqual('agent2');
			expect(requestData.PersonActivities[1].ShiftLayerIds).toEqual(['layer2']);
			expect(requestData.TrackedCommandInfo.TrackId).toEqual(result.commandControl.trackId);
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
		vm.scheduleManagementSvc = scheduleManagement;
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

	function setTime(container, hour) {
		var hourEl = container[0].querySelector('teams-time-picker .hours input');
		hourEl.value = hour;
		angular.element(hourEl).triggerHandler('change');
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