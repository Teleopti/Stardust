'use strict';

describe("TeamScheduleControllerTest", function() {
	var $q,
		rootScope,
		controller,
		grouScheduleFactory,
		$mdComponentRegistry,
		$mdSidenav,
		$mdUtil,
		$locale,
		mockCurrentUserInfo,
		mockLocale;

	beforeEach(function() {
		module('wfm.teamSchedule');
		module('externalModules');

		module(function($provide) {
			$provide.service('CurrentUserInfo', function() {
				return {
					DefaultTimeZone: "Etc/UTC",
					DateFormatLocale: "en-GB"
				};
			});

			$provide.service('$locale', function () {
				return {
					DATETIME_FORMATS: {
						shortTime: "h:mm a"
					}
				};
			});


			var mockAllTrueToggleService = {
				isFeatureEnabled: {
					query: function (param) {
						var queryDeferred = $q.defer();
						queryDeferred.resolve({ IsEnabled: true });
						return { $promise: queryDeferred.promise }
					}
				}
			};

			$provide.service('Toggle', function () { return mockAllTrueToggleService });
		});
	});


	

	
	beforeEach(inject(function(_CurrentUserInfo_, _$locale_) {
		mockCurrentUserInfo = _CurrentUserInfo_;
		moment.locale(mockCurrentUserInfo.DateFormatLocale);
		mockLocale = _$locale_;
	}));

	//var mockToggleService = {
	//	isFeatureEnabled: {
	//		query: function() {
	//			var queryDeferred = $q.defer();
	//			queryDeferred.resolve({ IsEnabled: true });
	//			return {
	//				$promise: queryDeferred.promise
	//			};
	//		}
	//	}
	//};

	//var mockTeamScheduleService = {
	//	loadAllTeams: {
	//		query: function() {
	//			var queryDeferred = $q.defer();
	//			queryDeferred.resolve({});
	//			return {
	//				$promise: queryDeferred.promise
	//			};
	//		}
	//	},
	//	loadAbsences: {
	//		query: function() {
	//			var queryDeferred = $q.defer();
	//			queryDeferred.resolve({});
	//			return {
	//				$promise: queryDeferred.promise
	//			};
	//		}
	//	},
	//	getPermissions: {
	//		query: function() {
	//			var queryDeferred = $q.defer();
	//			queryDeferred.resolve({

	//			});
	//			return {
	//				$promise: queryDeferred.promise
	//			};
	//		}
	//	},
	//	searchSchedules: {
	//		query: function() {
	//			var today = "2015-10-26";
	//			var queryDeferred = $q.defer();
	//			queryDeferred.resolve({
	//				Schedules: [
	//					{
	//						"PersonId": "221B-Baker-SomeoneElse",
	//						"Name": "SomeoneElse",
	//						"Date": today,
	//						"Projection": [
	//							{
	//								"Color": "#80FF80",
	//								"Description": "Email",
	//								"Start": today + " 07:00",
	//								"Minutes": 480
	//							}
	//						],
	//						"IsFullDayAbsence": false,
	//						"DayOff": null
	//					},
	//					{
	//						"PersonId": "221B-Sherlock",
	//						"Name": "Sherlock Holmes",
	//						"Date": today,
	//						"Projection": [
	//							{
	//								"Color": "#80FF80",
	//								"Description": "Email",
	//								"Start": today + " 08:00",
	//								"Minutes": 480
	//							}
	//						],
	//						"IsFullDayAbsence": false,
	//						"DayOff": null
	//					}
	//				],
	//				Total: 2,
	//				Keyword: ""
	//			});

	//			return {
	//				$promise: queryDeferred.promise
	//			};
	//		}
	//	}
	//};

	function setupMockTeamScheduleService(teamScheduleService) {
		teamScheduleService.loadAllTeams = {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return {
					$promise: queryDeferred.promise
				};
			}
		};
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
		teamScheduleService.searchSchedules= {
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
		},
		getAgentsPerPageSetting: {
			post: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Agents: 50 });
				return { $promise: queryDeferred.promise };
			}
		}
	};

	function setUpController($controller) {
		//rootScope = $rootScope.$new();

		return $controller("TeamScheduleCtrl", {
			$scope: rootScope,
			//Toggle: mockToggleService,
			//$q: $q,
			$locale: mockLocale,
			//TeamSchedule: mockTeamScheduleService,
			CurrentUserInfo: mockCurrentUserInfo
			//GroupScheduleFactory: grouScheduleFactory,
			//$mdComponentRegistry: $mdComponentRegistry,
			//$mdSidenav: $mdSidenav,
			//$mdUtil: $mdUtil
		});
	};

	beforeEach(inject(function(_$q_, _$rootScope_, _$controller_, _$locale_, _GroupScheduleFactory_,
		_$mdComponentRegistry_, _$mdSidenav_, _$mdUtil_, _TeamSchedule_) {
		$q = _$q_;
		rootScope = _$rootScope_.$new();
		//grouScheduleFactory = _GroupScheduleFactory_;
		//$mdComponentRegistry = _$mdComponentRegistry_;
		//$mdSidenav = _$mdSidenav_;
		//$mdUtil = _$mdUtil_;
		//$locale = _$locale_;
		setupMockTeamScheduleService(_TeamSchedule_);
		controller = setUpController(_$controller_);
	}));

	it("can select and deselect one person", inject(function() {
		//var scope = $rootScope.$new();
		//scope.$digest(); // this is needed to resolve the promise
		rootScope.$digest();

		var personSchedules = controller.groupScheduleVm.Schedules;

		personSchedules[0].IsSelected = true;
		controller.updateSelection(personSchedules[0]);

		var selectedPersonList = controller.selectedPersonIdList;

		expect(selectedPersonList.length).toEqual(1);
		expect(selectedPersonList[0]).toEqual("221B-Baker-SomeoneElse");

		personSchedules[0].IsSelected = false;
		controller.updateSelection(personSchedules[0]);

		expect(selectedPersonList.length).toEqual(0);
	}));

	it("can select and deselect current page", inject(function() {
		//var scope = $rootScope.$new();
		rootScope.$digest(); // this is needed to resolve the promiseddd

		controller.isAllInCurrentPageSelected = true;
		controller.toggleAllSelectionInCurrentPage();

		var selectedPersonList = controller.selectedPersonIdList;

		expect(selectedPersonList.length).toEqual(2);
		expect(selectedPersonList[0]).toEqual("221B-Baker-SomeoneElse");
		expect(selectedPersonList[1]).toEqual("221B-Sherlock");

		controller.isAllInCurrentPageSelected = false;
		controller.toggleAllSelectionInCurrentPage();

		expect(selectedPersonList.length).toEqual(0);
	}));

	it("can display person selection status correctly", inject(function() {
		controller.selectedPersonIdList = ["221B-Baker-SomeoneElse", "221B-Baker-SomeoneElse1"];
		//var scope = $rootScope.$new();
		rootScope.$digest(); // this is needed to resolve the promiseddd

		var personSchedules = controller.groupScheduleVm.Schedules;

		expect(personSchedules[0].IsSelected).toEqual(true);
		expect(personSchedules[1].IsSelected).toEqual(false);
		expect(controller.isAllInCurrentPageSelected).toEqual(false);
	}));

	it("can display current page selection status correctly when all people in current page are selected", inject(function() {
		controller.selectedPersonIdList = ["221B-Baker-SomeoneElse", "221B-Baker-SomeoneElse1", "221B-Sherlock"];
		//var scope = $rootScope.$new();
		rootScope.$digest(); // this is needed to resolve the promiseddd

		var personSchedules = controller.groupScheduleVm.Schedules;

		expect(personSchedules[0].IsSelected).toEqual(true);
		expect(personSchedules[1].IsSelected).toEqual(true);
		expect(controller.isAllInCurrentPageSelected).toEqual(true);
	}));

	it("should show meridian correctly", inject(function () {
		//var scope = $rootScope.$new();
		rootScope.$digest(); // this is needed to resolve the promiseddd

		expect(controller.showMeridian).toEqual(true);
	}));
});
