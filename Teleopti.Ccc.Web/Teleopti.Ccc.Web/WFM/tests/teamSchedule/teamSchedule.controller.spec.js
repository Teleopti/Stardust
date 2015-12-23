'use strict';

describe("TeamScheduleControllerTest", function() {
	var $q,
		rootScope,
		controller;

	beforeEach(function() {
		module('wfm.teamSchedule');
		module('externalModules');

		module(function ($provide) {
			$provide.service('CurrentUserInfo', setupMockCurrentUserInfoService);
			$provide.service('$locale',setupMockLocale);
			$provide.service('Toggle', setupMockAllTrueToggleService);
		});
	});

	beforeEach(inject(function(_$q_, _$rootScope_, _$controller_, _TeamSchedule_) {
		$q = _$q_;
		rootScope = _$rootScope_.$new();
		setupMockTeamScheduleService(_TeamSchedule_);
		controller = setUpController(_$controller_);
	}));

	it("can select and deselect one person", inject(function () {
		rootScope.$digest();
		
		var personSchedule1 = controller.groupScheduleVm.Schedules[0];
		controller.personIdSelectionDic[personSchedule1.PersonId].isSelected = true;

		var selectedPersonList = controller.getSelectedPersonIdList();

		expect(selectedPersonList.length).toEqual(1);
		expect(selectedPersonList[0]).toEqual("221B-Baker-SomeoneElse");

		controller.personIdSelectionDic[personSchedule1.PersonId].isSelected = false;
		selectedPersonList = controller.getSelectedPersonIdList();

		expect(selectedPersonList.length).toEqual(0);
	}));

	
	it("should show meridian correctly", inject(function () {
		rootScope.$digest();

		expect(controller.showMeridian).toEqual(true);
	}));


	function setUpController($controller) {

		return $controller("TeamScheduleCtrl", {
			$scope: rootScope
		});
	};

	function setupMockTeamScheduleService(teamScheduleService) {
		teamScheduleService.loadAllTeams = {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return {
					$promise: queryDeferred.promise
				};
			}
		};
		teamScheduleService.loadAbsences = {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return {
					$promise: queryDeferred.promise
				};
			}
		};
		teamScheduleService.getPermissions = {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({

				});
				return {
					$promise: queryDeferred.promise
				};
			}
		};
		teamScheduleService.searchSchedules = {
			query: function () {
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

		teamScheduleService.getAgentsPerPageSetting = {
			post: function () {
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
			isFeatureEnabled: {
				query: function (param) {
					var queryDeferred = $q.defer();
					queryDeferred.resolve({ IsEnabled: true });
					return { $promise: queryDeferred.promise }
				}
			}
		};
	}

	function setupMockCurrentUserInfoService() {
		return {
			DefaultTimeZone: "Etc/UTC",
			DateFormatLocale: "en-GB"
		};
	}

});
