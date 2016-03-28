﻿'use strict';

describe("[Test for TeamScheduleController]:", function() {
	var $q,
		rootScope,
		controller,
		searchScheduleCalledTimes,
		mockSignalRBackendServer = {};

	var nowDate = new Date("2015-10-26 12:16:00");

	beforeEach(function() {
		module('wfm.teamSchedule');
		module('externalModules');
		module('wfm.notice');
		module('shortcutsService');
		module('wfmDate');

		module(function($provide) {
			$provide.service('CurrentUserInfo', setupMockCurrentUserInfoService);
			$provide.service('$locale', setupMockLocale);
			$provide.service('Toggle', setupMockAllTrueToggleService);
			$provide.service('SignalR', setupMockSignalRService);
			$provide.service('WFMDate', function () {
				return {
					nowInUserTimeZone: function() {
						return moment(nowDate);
					}
				};
			});
		});
	});

	beforeEach(inject(function(_$q_, _$rootScope_, _$controller_, _TeamSchedule_) {
		$q = _$q_;
		rootScope = _$rootScope_.$new();
		setupMockTeamScheduleService(_TeamSchedule_);
		controller = setUpController(_$controller_);
	}));

	it("can select one person", inject(function() {
		rootScope.$digest();

		var selectedPersons = controller.selectedPersonInfo();
		var personSchedule1 = controller.groupScheduleVm().Schedules[0];
		selectedPersons[personSchedule1.PersonId].isSelected = true;
		var selectedPersonList = controller.getSelectedPersonIdList();

		expect(selectedPersonList.length).toEqual(1);
		expect(selectedPersonList[0]).toEqual("221B-Baker-SomeoneElse");
	}));

	it("can deselect one person", inject(function() {
		rootScope.$digest();

		var selectedPersons = controller.selectedPersonInfo();
		var personSchedule1 = controller.groupScheduleVm().Schedules[0];
		selectedPersons[personSchedule1.PersonId].isSelected = false;
		var selectedPersonList = controller.getSelectedPersonIdList();

		expect(selectedPersonList.length).toEqual(0);
	}));

	it("should reload schedule when schedule changed by others", inject(function() {
		rootScope.$digest();
		searchScheduleCalledTimes = 0;

		controller.scheduleDate = new Date("2015-10-26");
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-25T00:00:00",
				"EndDate": "D2015-10-27T00:00:00"
			}
		]);

		expect(searchScheduleCalledTimes).toEqual(1);
	}));

	it("should not reload schedule when other people schedule changed", inject(function() {
		rootScope.$digest();
		searchScheduleCalledTimes = 0;

		controller.scheduleDate = new Date("2015-10-26");
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-otherPeople",
				"StartDate": "D2015-10-25T00:00:00",
				"EndDate": "D2015-10-27T00:00:00"
			}
		]);

		expect(searchScheduleCalledTimes).toEqual(0);
	}));

	it("should not reload schedule when schedule changed beyond the date", inject(function() {
		rootScope.$digest();
		searchScheduleCalledTimes = 0;

		controller.scheduleDate = new Date("2015-10-26");
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-27T00:00:00",
				"EndDate": "D2015-10-28T00:00:00"
			}
		]);

		expect(searchScheduleCalledTimes).toEqual(0);
	}));

	it("should get correct new activity start for today", function() {
		rootScope.$digest();
		controller.scheduleDate = new Date("2015-10-26");
		var selectedPersons = controller.selectedPersonInfo();
		var personSchedule1 = controller.groupScheduleVm().Schedules[0];
		selectedPersons[personSchedule1.PersonId].isSelected = true;

		expect(controller.defaultNewActivityStart()).toEqual(moment("2015-10-26 12:30:00").format('LT'));
	});

	it("should get correct new activity start for selected date", function () {
		controller.scheduleDate = new Date("2015-10-26");
		nowDate = new Date("2015-10-25 12:16:00");
		rootScope.$digest();
		
		
		var selectedPersons = controller.selectedPersonInfo();
		var personSchedule1 = controller.groupScheduleVm().Schedules[0];
		selectedPersons[personSchedule1.PersonId].isSelected = true;

		expect(controller.defaultNewActivityStart()).toEqual(moment("2015-10-26 08:00:00").format('LT'));
	});

	function setUpController($controller) {
		return $controller("TeamScheduleCtrl", {
			$scope: rootScope
		});
	};

	function setupMockTeamScheduleService(teamScheduleService) {

		teamScheduleService.loadAbsences = {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return {
					$promise: queryDeferred.promise
				};
			}
		};
		teamScheduleService.getPermissions = {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({

				});
				return {
					$promise: queryDeferred.promise
				};
			}
		};
		teamScheduleService.searchSchedules = {
			query: function() {
				var today = "2015-10-26";
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					Schedules: [
						{
							"PersonId": "221B-Baker-SomeoneElse",
							"Name": "SomeoneElse",
							"Date": today,
							"Projection": [
								{
									"Color": "#80FF80",
									"Description": "Email",
									"Start": today + " 07:00",
									"Minutes": 480
								}
							],
							"IsFullDayAbsence": false,
							"DayOff": null
						},
						{
							"PersonId": "221B-Sherlock",
							"Name": "Sherlock Holmes",
							"Date": today,
							"Projection": [
								{
									"Color": "#80FF80",
									"Description": "Email",
									"Start": today + " 08:00",
									"Minutes": 480
								}
							],
							"IsFullDayAbsence": false,
							"DayOff": null
						}
					],
					Total: 2,
					Keyword: ""
				});

				return {
					$promise: queryDeferred.promise
				};
			}
		};

		teamScheduleService.getSchedules = {
			query: function(params) {
				searchScheduleCalledTimes = searchScheduleCalledTimes + 1;
				var queryDeferred = $q.defer();
				queryDeferred.resolve();
				return { $promise: queryDeferred.promise };
			}
		};

		teamScheduleService.getAgentsPerPageSetting = {
			post: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Agents: 50 });
				return { $promise: queryDeferred.promise };
			}
		};
	};

	function setupMockLocale() {
		return {
			DATETIME_FORMATS: {
				shortTime: "h:mm a"
			}
		};
	}

	function setupMockAllTrueToggleService() {
		return {
			WfmTeamSchedule_SetAgentsPerPage_36230: true,
			WfmTeamSchedule_AbsenceReporting_35995: true,
			WfmPeople_AdvancedSearch_32973: true,
			WfmTeamSchedule_SwapShifts_36231: true,
			WfmTeamSchedule_SeeScheduleChangesByOthers_36303: true
		};
	}

	function setupMockCurrentUserInfoService() {
		return {
			DefaultTimeZone: "Etc/UTC",
			DateFormatLocale: "en-GB"
		};
	}

	function setupMockSignalRService() {
		mockSignalRBackendServer.subscriptions = [];

		return {
			subscribe: function(options, eventHandler, errorHandler) {
				mockSignalRBackendServer.subscriptions.push(options);
				mockSignalRBackendServer.notifyClients = eventHandler;
			},
			subscribeBatchMessage: function (options, messageHandler, timeout) {
				mockSignalRBackendServer.notifyClients = messageHandler;
			}
		};
	}
});
