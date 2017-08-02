'use strict';

describe("teamschedule controller tests", function() {
	var $q,
		rootScope,
		controller,
		searchScheduleCalledTimes,
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
			$provide.service('groupPageService', setUpMockGroupPagesService);
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

	it("should clear person selection when search text changed", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.loadSchedules();
		controller.selectAllForAllPages();

		var personSchedule1 = scheduleMgmt.groupScheduleVm.Schedules[0];
		expect(personSchedule1.IsSelected).toEqual(true);

		
		controller.searchOptions.searchKeywordChanged = true;
		controller.onKeyWordInSearchInputChanged();

		expect(scheduleMgmt.groupScheduleVm.Schedules[0].IsSelected).toEqual(false);
		expect(scheduleMgmt.groupScheduleVm.Schedules[1].IsSelected).toEqual(false);
		expect(scheduleMgmt.groupScheduleVm.Schedules[2].IsSelected).toEqual(false);
		expect(Object.keys(personSelection.personInfo).length).toEqual(0);
	});

	it("should clear person selection when date changed", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.loadSchedules();
		controller.selectAllForAllPages();

		var personSchedule1 = scheduleMgmt.groupScheduleVm.Schedules[0];
		expect(personSchedule1.IsSelected).toEqual(true);

		controller.onScheduleDateChanged();

		expect(scheduleMgmt.groupScheduleVm.Schedules[0].IsSelected).toEqual(false);
		expect(scheduleMgmt.groupScheduleVm.Schedules[1].IsSelected).toEqual(false);
		expect(scheduleMgmt.groupScheduleVm.Schedules[2].IsSelected).toEqual(false);

		expect(Object.keys(personSelection.personInfo).length).toEqual(0);
	});

	it("should clear person selection when selected teams changed", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.loadSchedules();
		controller.selectAllForAllPages();

		var personSchedule1 = scheduleMgmt.groupScheduleVm.Schedules[0];
		expect(personSchedule1.IsSelected).toEqual(true);

		controller.onSelectedTeamsChanged(['empty Team']);

		expect(scheduleMgmt.groupScheduleVm.Schedules[0].IsSelected).toEqual(false);
		expect(scheduleMgmt.groupScheduleVm.Schedules[1].IsSelected).toEqual(false);
		expect(scheduleMgmt.groupScheduleVm.Schedules[2].IsSelected).toEqual(false);
		expect(Object.keys(personSelection.personInfo).length).toEqual(0);
	});

	it("should active search status after selected teams changed", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.searchOptions = {
			focusingSearch: false
		};

		controller.onSelectedTeamsChanged(['empty Team']);

		expect(controller.searchOptions.focusingSearch).toEqual(true);
	});

	it("should deactive search status after selected date changed", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.searchOptions = {
			focusingSearch: true
		};

		controller.onScheduleDateChanged();

		expect(controller.searchOptions.focusingSearch).toEqual(false);
	});

	it("should deactive search status after search text changed", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.searchOptions = {
			focusingSearch: true
		};

		controller.onKeyWordInSearchInputChanged();

		expect(controller.searchOptions.focusingSearch).toEqual(false);
	});

	it("should deactive search status after selected favorite changed", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.searchOptions = {
			focusingSearch: true
		};

		controller.applyFavorite({
			Name: 'fakeFavorite',
			SearchTerm: 'a',
			TeamIds: ['fakeTeam1Id']
		});

		expect(controller.searchOptions.focusingSearch).toEqual(false);
	});

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

		teamScheduleService.hierarchy = function () {
			return $q(function (resolve) {
				resolve({ Children: [] });
			});
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
						"IsSelected":false,
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
						"IsSelected":false,
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
						"IsSelected":false,
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
	};

	function setupMockLocale() {
		return {
			DATETIME_FORMATS: {
				shortTime: "h:mm a"
			}
		};
	}

	function setupMockAllTrueToggleService() {
		return { };
	}

	function setUpMockGroupPagesService() {
		return {
			fetchAvailableGroupPages: function() {
				return {
					then: function() {

					}
			};
			}

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

});
