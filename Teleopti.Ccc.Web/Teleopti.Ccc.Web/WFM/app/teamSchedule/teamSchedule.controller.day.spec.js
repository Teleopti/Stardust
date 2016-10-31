﻿'use strict';

describe("teamschedule controller tests", function() {
	var $q,
		rootScope,
		controller,
		searchScheduleCalledTimes,
		mockSignalRBackendServer = {},
		personSelection,
		scheduleMgmt;

	var nowDate = new Date("2015-10-26 12:16:00");

	beforeEach(function() {		
		module('externalModules');
		module('wfm.notice');
		module('shortcutsService');
		module('wfmDate');
		module('wfm.teamSchedule');

		module(function($provide) {
			$provide.service('CurrentUserInfo', setupMockCurrentUserInfoService);
			$provide.service('$locale', setupMockLocale);
			$provide.service('Toggle', setupMockAllTrueToggleService);
			$provide.service('signalRSVC', setupMockSignalRService);
			$provide.service('WFMDate', function () {
				return {
					nowInUserTimeZone: function() {
						return moment(nowDate);
					}
				};
			});
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
			SelectedActivities: ["activity1"],
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
		
	function setUpController($controller) {
		return $controller("TeamScheduleCtrl", {
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
				});

				return {
					$promise: queryDeferred.promise
				};
			}
		};

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
	
	function createSchedule(personId, belongsToDate, dayOff, projectionInfoArray) {

		var dateMoment = moment(belongsToDate);
		var projections = [];

		var fakeSchedule = {
			PersonId: personId,
			Date: dateMoment,
			DayOff: dayOff == null ? null : createDayOff(),
			Shifts: [{
				Date: dateMoment,
				Projections: createProjection(),
				AbsenceCount: 0,
				ActivityCount: 0
			}],
			ScheduleEndTime: function () { return dateMoment.endOf('day') },
			AllowSwap: function () { return false; }
		};

		function createProjection() {

			if (dayOff == null) {
				projectionInfoArray.forEach(function (projectionInfo) {
					var dateMomentCopy = moment(dateMoment);

					projections.push({
						Start: dateMomentCopy.add(projectionInfo.startHour, 'hours').format('YYYY-MM-DD HH:mm'),
						Minutes: moment.duration(projectionInfo.endHour - projectionInfo.startHour, 'hours').asMinutes()
					});
				});
			}

			return projections;
		};

		function createDayOff() {
			return {
				DayOffName: 'Day off',
				Start: dateMoment.format('YYYY-MM-DD HH:mm'),
				Minutes: 1440
			};

		};

		return fakeSchedule;
	}
});
