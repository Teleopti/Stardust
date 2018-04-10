'use strict';

describe("teamschedule controller tests", function () {
	var $q,
		$mdSidenav,
		rootScope,
		controller,
		searchScheduleCalledTimes,
		personSelection,
		scheduleMgmt,
		teamScheduleService,
		staffingConfigStorageService,
		fakeStateParams,
		$controller;

	beforeEach(function () {
		module('externalModules');
		module('wfm.notice');
		module('shortcutsService');
		module('wfm.teamSchedule');

		module(function ($provide) {
			fakeStateParams = setupMockStateParams();
			$provide.service('CurrentUserInfo', setupMockCurrentUserInfoService);
			$provide.service('$locale', setupMockLocale);
			$provide.service('$stateParams', function () { return fakeStateParams });
			$provide.service('Toggle', setupMockAllTrueToggleService);
			$provide.service('groupPageService', setUpMockGroupPagesService);
			$provide.service('$mdSidenav', setUpMockMdSideNav);
		});
	});

	beforeEach(inject(function (_$q_, _$mdSidenav_, _$rootScope_, _$controller_, _TeamSchedule_, _PersonSelection_,
		_ScheduleManagement_, _TeamsStaffingConfigurationStorageService_) {
		$q = _$q_;
		$mdSidenav = _$mdSidenav_;
		rootScope = _$rootScope_.$new();
		personSelection = _PersonSelection_;
		scheduleMgmt = _ScheduleManagement_;
		setupMockTeamScheduleService(_TeamSchedule_);
		teamScheduleService = _TeamSchedule_;
		$controller = _$controller_;

		staffingConfigStorageService = _TeamsStaffingConfigurationStorageService_;
		staffingConfigStorageService.clearConfig();
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

	it("should be hide the select all people on every page link if the command panel is active", function () {
		controller.scheduleDate = new Date("2018-04-10");
		rootScope.$digest();

		personSelection.personInfo['person-emptySchedule'] = {
			Checked: true
		};
		controller.loadSchedules();
		rootScope.$digest();

		controller.triggerCommand('addAbsence', true);
		rootScope.$digest();

		expect(controller.commandPanelClosed()).toBe(false);
	});

	it("should keep the activity selection when schedule reloaded", function () {
		controller.scheduleDate = new Date("2015-10-26");
		rootScope.$digest();
		personSelection.personInfo['221B-Baker-SomeoneElse'] = {
			SelectedActivities: [
				{
					shiftLayerId: "activity1",
					date: "2015-10-26"
				}],
			SelectedAbsences: []
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

	it("should clear person selection after click search button", function () {
		controller.scheduleDate = new Date("2015-10-26");

		controller.loadSchedules();
		controller.selectAllForAllPages();

		var personSchedule1 = scheduleMgmt.groupScheduleVm.Schedules[0];
		expect(personSchedule1.IsSelected).toEqual(true);

		controller.onKeyWordInSearchInputChanged();

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

	it("should  set show only absence value correctly when click showOnlyAbsence", function () {
		controller.scheduleDate = new Date("2015-10-26");
		controller.searchOptions = {
			focusingSearch: true
		};
		controller.currentSettings.onlyLoadScheduleWithAbsence = true;
		controller.toggleShowAbsenceOnly();
		expect(teamScheduleService.currentInput().IsOnlyAbsences).toEqual(true);

		controller.currentSettings.onlyLoadScheduleWithAbsence = false;
		controller.toggleShowAbsenceOnly();
		expect(teamScheduleService.currentInput().IsOnlyAbsences).toEqual(false);
	});

	it("should remember skill selection when skill changed",
		function () {
			controller.scheduleDate = new Date("2015-10-26");
			controller.staffingEnabled = true;

			controller.onSelectedSkillChanged({ Id: 'XYZ' });

			var config = staffingConfigStorageService.getConfig();
			expect(!!config).toEqual(true);
			expect(config.skillId).toEqual('XYZ');
			expect(!!config.skillGroupId).toEqual(false);
		});

	it("should read preselect skill when show staffing is enabled",
		function () {
			controller.scheduleDate = new Date("2015-10-26");
			staffingConfigStorageService.setSkill('mySkill');
			controller.staffingEnabled = true;

			controller.showStaffing();
			expect(!!controller.preselectedSkills).toEqual(true);
			expect(controller.preselectedSkills.skillIds.length).toEqual(1);
			expect(controller.preselectedSkills.skillIds[0]).toEqual('mySkill');
			expect(!!controller.preselectedSkills.skillAreaId).toEqual(false);
		});

	it("should remember skill group selection when skill group changed",
		function () {
			controller.scheduleDate = new Date("2015-10-26");
			controller.staffingEnabled = true;

			controller.onSelectedSkillChanged(null, { Id: 'skillGroup' });

			var config = staffingConfigStorageService.getConfig();
			expect(!!config).toEqual(true);
			expect(config.skillGroupId).toEqual('skillGroup');
			expect(!!config.skillId).toEqual(false);
		});

	it("should read preselect skill group when show staffing is enabled and there is valid config",
		function () {
			controller.scheduleDate = new Date("2015-10-26");
			staffingConfigStorageService.setSkill(null, 'skillGroup');
			controller.staffingEnabled = true;

			controller.showStaffing();
			expect(!!controller.preselectedSkills).toEqual(true);
			expect(!!controller.preselectedSkills.skillIds).toEqual(false);
			expect(controller.preselectedSkills.skillAreaId).toEqual('skillGroup');
		});

	it("should read preselect skill group when show staffing is enabled and there is invalid config",
		function () {
			controller.scheduleDate = new Date("2015-10-26");
			controller.staffingEnabled = true;
			controller.showStaffing();

			expect(!!controller.preselectedSkills.skillIds).toEqual(false);
			expect(!!controller.preselectedSkills.skillAreaId).toEqual(false);
		});

	it("should remember use shrinkage status when change the use shrinkage status", function () {
		controller.scheduleDate = new Date("2015-10-26");
		controller.staffingEnabled = true;

		controller.onUseShrinkageChanged(true);
		var config = staffingConfigStorageService.getConfig();
		expect(!!config).toEqual(true);
		expect(config.useShrinkage).toEqual(true);

	});

	it("should read prechecked status for use shrinkage when show staffing is enabled and there is valid config", function () {
		staffingConfigStorageService.setShrinkage(true);
		controller = setUpController($controller);
		controller.scheduleDate = new Date("2015-10-26");
		expect(controller.useShrinkage).toEqual(true);
	});

	it("should read prechecked status for use shrinkage when show staffing is enabled and there is invalid config", function () {
		controller.scheduleDate = new Date("2015-10-26");
		controller.staffingEnabled = true;

		controller.showStaffing();
		expect(!!controller.useShrinkage).toEqual(false);
	});

	it("should init all settings state as same as the route state", function () {
		staffingConfigStorageService.setSkill(null, 'groupPage');
		controller = setUpController($controller);

		expect(controller.searchOptions.keyword).toEqual(fakeStateParams.keyword);
		expect(controller.scheduleDate).toEqual(fakeStateParams.selectedDate);
		expect(controller.teamNameMap).toEqual(fakeStateParams.teamNameMap);
		expect(controller.sortOption).toEqual(fakeStateParams.selectedSortOption);
		if (fakeStateParams.selectedTeamIds) {
			expect(controller.selectedGroups.groupIds).toEqual(fakeStateParams.selectedTeamIds);
		} else {
			expect(controller.selectedGroups.groupPageId).toEqual(fakeStateParams.selectedGroupPage.pageId);
			expect(controller.selectedGroups.groupIds).toEqual(fakeStateParams.selectedGroupPage.groupIds);
		}
		expect(controller.staffingEnabled).toEqual(fakeStateParams.staffingEnabled);

		expect(!!controller.preselectedSkills.skillIds).toEqual(false);
		expect(controller.preselectedSkills.skillAreaId).toEqual('groupPage');
	});

	function setUpController($controller) {
		return $controller("TeamScheduleController", {
			$scope: rootScope,
			personSelectionSvc: personSelection
		});
	};

	function setupMockTeamScheduleService(teamScheduleService) {
		var currentInput;

		teamScheduleService.currentInput = function () {
			return currentInput;
		}

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

		teamScheduleService.hierarchy = function () {
			return $q(function (resolve) {
				resolve({ Children: [] });
			});
		};

		teamScheduleService.searchSchedules = function (input) {
			currentInput = input;
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
						"IsSelected": false,
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
						"IsSelected": false,
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
						"IsSelected": false,
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
				then: function (callback) {
					callback(response);
				}
			}
		}


		teamScheduleService.getSchedules = function (date, agents) {
			return {
				then: function (cb) {
					searchScheduleCalledTimes = searchScheduleCalledTimes + 1;
				}
			}
		}

		teamScheduleService.getAgentsPerPageSetting = {
			post: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Agents: 50 });
				return { $promise: queryDeferred.promise };
			}
		}

		teamScheduleService.getScheduleAuditTrailSetting = function () {
			return {
				then: function () { }
			};
		}
	};

	function setupMockLocale() {
		return {
			DATETIME_FORMATS: {
				shortTime: "h:mm a"
			}
		};
	}

	function setupMockStateParams() {
		return {
			selectedDate: new Date('2018-02-02'),
			keyword: 'search',
			teamNameMap: '1 team selected',
			selectedSortOption: '',
			selectedTeamIds: [],
			selectedGroupPage: { pageId: '', groupIds: [] },
			staffingEnabled: true
		};
	}

	function setUpMockMdSideNav() {
		return function () {
			return {
				open: function () {
					return $q.resolve();
				},
				isOpen: function () {
					return true;
				},
				close: function () {
					return $q.resolve();
				}
			};
		};
	}

	function setupMockAllTrueToggleService() {
		return {};
	}

	function setUpMockGroupPagesService() {
		return {
			fetchAvailableGroupPages: function () {
				return {
					then: function () {

					}
				};
			}

		};

	}

	function setupMockCurrentUserInfoService() {
		return {
			CurrentUserInfo: function () {
				return {
					DefaultTimeZone: "Etc/UTC",
					DefaultTimeZoneName: "Etc/UTC",
					DateFormatLocale: "en-GB"
				};
			}
		};
	}

});
