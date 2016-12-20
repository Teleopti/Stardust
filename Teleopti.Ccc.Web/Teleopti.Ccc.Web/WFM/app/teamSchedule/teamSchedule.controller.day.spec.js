'use strict';

describe("teamschedule controller tests", function() {
	var $q,
		rootScope,
		controller,
		searchScheduleCalledTimes,
		mockSignalRBackendServer = {},
		personSelection,
		scheduleMgmt;

	beforeEach(function() {		
		module('externalModules');
		module('wfm.notice');
		module('shortcutsService');
		module('wfm.teamSchedule');

		module(function($provide) {
			$provide.service('CurrentUserInfo', setupMockCurrentUserInfoService);
			$provide.service('$locale', setupMockLocale);
			$provide.service('Toggle', setupMockAllTrueToggleService);
			$provide.service('signalRSVC', setupMockSignalRService);			
		});
	});

	beforeEach(inject(function(_$q_, _$rootScope_, _$controller_, _TeamSchedule_, _PersonSelection_, _ScheduleManagement_) {
		$q = _$q_;
		rootScope = _$rootScope_.$new();
		personSelection = _PersonSelection_;
		scheduleMgmt = _ScheduleManagement_;
		setupMockTeamScheduleService(_TeamSchedule_);
		controller = setUpController(_$controller_);
	}));

	it("can display person selection status correctly when turning pages", inject(function () {
		controller.scheduleDate = new Date("2015-10-26");
		rootScope.$digest();

		personSelection.personInfo['person-emptySchedule'] = { Checked: true };
		controller.loadSchedules();
		rootScope.$digest();

		var schedules = scheduleMgmt.groupScheduleVm.Schedules;
		expect(schedules[2].IsSelected).toEqual(true);
		expect(schedules[1].IsSelected).toEqual(false);
		expect(schedules[0].IsSelected).toEqual(false);
	}));
	
	
	it("should keep the activity selection when schedule reloaded", function () {
		controller.scheduleDate = new Date("2015-10-26");
		rootScope.$digest();
		personSelection.personInfo['221B-Baker-SomeoneElse'] = {
			SelectedActivities: [
			{
				shiftLayerId: "activity1",
				date: "2015-10-26"
			} ],
			SelectedAbsences:[]
		}

		controller.loadSchedules();
		rootScope.$digest();
		var personSchedule1 = scheduleMgmt.groupScheduleVm.Schedules[0];
		expect(personSchedule1.Shifts[0].Projections[0].Selected).toEqual(true);

	});

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

		controller.scheduleDate = new Date("2015-10-25");
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-27T00:00:00",
				"EndDate": "D2015-10-28T00:00:00"
			}
		]);

		expect(searchScheduleCalledTimes).toEqual(0);
	}));

	it("should not reload schedule when schedule changed beyond the yesterday, today and tomorrow when ManageScheduleForDistantTimezonesEnabled is on", inject(function() {
		rootScope.$digest();
		searchScheduleCalledTimes = 0;

		controller.scheduleDate = new Date("2015-10-25");
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-22T00:00:00",
				"EndDate": "D2015-10-23T00:00:00"
			}
		]);

		expect(searchScheduleCalledTimes).toEqual(0);
	}));

	it("should reload schedule when schedule changed from yesterday when toggle ManageScheduleForDistantTimezonesEnabled is on", inject(function() {
		rootScope.$digest();
		searchScheduleCalledTimes = 0;

		controller.scheduleDate = new Date("2015-10-25");
		controller.toggles.ManageScheduleForDistantTimezonesEnabled = true;
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-24T00:00:00",
				"EndDate": "D2015-10-24T00:00:00"
			}
		]);

		expect(searchScheduleCalledTimes).toEqual(1);
	}));

	it("should reload schedule when schedule changed from tomorrow when toggle ManageScheduleForDistantTimezonesEnabled is on", inject(function() {
		rootScope.$digest();
		searchScheduleCalledTimes = 0;

		controller.scheduleDate = new Date("2015-10-25");
		controller.toggles.ManageScheduleForDistantTimezonesEnabled = true;
		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "221B-Baker-SomeoneElse",
				"StartDate": "D2015-10-26T00:00:00",
				"EndDate": "D2015-10-27T00:00:00"
			}
		]);

		expect(searchScheduleCalledTimes).toEqual(1);
	}));
		
	function setUpController($controller) {
		return $controller("TeamScheduleController", {
			$scope: rootScope,
			personSelectionSvc: personSelection
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

		teamScheduleService.searchSchedules = function (input) {
			var today = "2015-10-26";
			var scheduleData = {
				Schedules: [
					{
						"PersonId": "221B-Baker-SomeoneElse",
						"Name": "SomeoneElse",
						"Date": today,
						"Projection": [
							{
								"ShiftLayerIds": ["activity1"],
								"Color": "#80FF80",
								"Description": "Email",
								"Start": today + " 07:00",
								"Minutes": 480
							},
							{
								"ShiftLayerIds": ["activity2"],
								"Color": "#80FF80",
								"Description": "Email",
								"Start": today + " 15:00",
								"Minutes": 120
							}
						],
						"IsFullDayAbsence": false,
						"DayOff": null,
						"Timezone": {
							"IanaId": "Europe/Berlin",
							"DisplayName": "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
						}
					},
					{
						"PersonId": "221B-Sherlock",
						"Name": "Sherlock Holmes",
						"Date": today,
						"Projection": [
							{
								"ShiftLayerIds": ["activity1"],
								"Color": "#80FF80",
								"Description": "Email",
								"Start": today + " 08:00",
								"Minutes": 480
							}
						],
						"IsFullDayAbsence": false,
						"DayOff": null,
						"Timezone": {
							"IanaId": "Europe/Berlin",
							"DisplayName": "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
						}
					},
					{
						"PersonId": "person-emptySchedule",
						"Name": "Sherlock Holmes",
						"Date": today,
						"Projection": [],
						"IsFullDayAbsence": false,
						"DayOff": null,
						"Timezone": {
							"IanaId": "Asia/Shanghai",
							"DisplayName": "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
						}
					}
				],
				Total: 3,
				Keyword: ""
			};
			var response = { data: scheduleData };
			return {
				then:function(callback) {
					callback(response);
				}
			}
		}
		

		teamScheduleService.getSchedules = function(date, agents) {
			return {
				then: function(cb) {
					searchScheduleCalledTimes = searchScheduleCalledTimes + 1;
				}
			}
		}	

		teamScheduleService.getAgentsPerPageSetting = {
			post: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Agents: 50 });
				return { $promise: queryDeferred.promise };
			}
		};

		teamScheduleService.getAvalableHierachy = function(date) {
			return {
				then: function (){}
			}
		}
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
			WfmTeamSchedule_SwapShifts_36231: true,
			WfmTeamSchedule_SeeScheduleChangesByOthers_36303: true
		};
	}

	function setupMockCurrentUserInfoService() {
		return {
			CurrentUserInfo: function() {
				return {
					DefaultTimeZone: "Etc/UTC",
					DefaultTimeZoneName: "Etc/UTC",
					DateFormatLocale: "en-GB"
				};
			}			
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
