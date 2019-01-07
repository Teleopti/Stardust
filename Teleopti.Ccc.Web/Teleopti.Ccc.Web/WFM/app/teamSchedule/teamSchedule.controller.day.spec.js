(function () {
	'use strict';
	describe("teamschedule controller tests", function () {
		var $q,
			$mdSidenav,
			rootScope,
			controller,
			searchScheduleCalledTimes = 0,
			personSelection,
			scheduleMgmt,
			teamScheduleService,
			staffingConfigStorageService,
			$controller,
			$httpBackend,
			viewStateKeeper,
			fakeToggle;

		beforeEach(function () {
			module('externalModules');
			module('wfm.notice');
			module('shortcutsService');
			module('wfm.teamSchedule');

			module(function ($provide) {
				fakeToggle = {};
				$provide.service('$locale', setupMockLocale);
				$provide.service('Toggle', function () {
					return fakeToggle;
				});
				$provide.service('groupPageService', setUpMockGroupPagesService);
				$provide.service('$mdSidenav', setUpMockMdSideNav);
			});
		});

		beforeEach(inject(function (_$q_, _$mdSidenav_, _$rootScope_, _$controller_, _TeamSchedule_, _PersonSelection_,
			_ScheduleManagement_, _TeamsStaffingConfigurationStorageService_, _ViewStateKeeper_, _$httpBackend_, CurrentUserInfo) {
			$q = _$q_;
			$mdSidenav = _$mdSidenav_;
			$controller = _$controller_;
			rootScope = _$rootScope_.$new();
			personSelection = _PersonSelection_;
			scheduleMgmt = _ScheduleManagement_;
			setupMockTeamScheduleService(_TeamSchedule_);
			teamScheduleService = _TeamSchedule_;
			viewStateKeeper = _ViewStateKeeper_;
			$httpBackend = _$httpBackend_;
			CurrentUserInfo.SetCurrentUserInfo({
				DefaultTimeZone: "Etc/UTC",
				DefaultTimeZoneName: "Etc/UTC",
				DateFormatLocale: "en-GB"
			});

			staffingConfigStorageService = _TeamsStaffingConfigurationStorageService_;
			staffingConfigStorageService.clearConfig();
			controller = setUpController(_$controller_);
			searchScheduleCalledTimes = 0;
			
		}));

		it("can display person selection status correctly when turning pages", function () {
			controller.scheduleDate = "2015-10-26";
			rootScope.$digest();

			personSelection.personInfo['person-emptySchedule'] = { Checked: true };
			controller.loadSchedules();
			rootScope.$digest();

			var schedules = scheduleMgmt.groupScheduleVm.Schedules;
			expect(schedules[2].IsSelected).toEqual(true);
			expect(schedules[1].IsSelected).toEqual(false);
			expect(schedules[0].IsSelected).toEqual(false);
		});

		it("should be hide the select all people on every page link if the command panel is active", function () {
			controller.scheduleDate = "2018-04-10";
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
			controller.scheduleDate = "2015-10-26";
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
			controller.scheduleDate = "2015-10-26";

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
			controller.scheduleDate = "2015-10-26";

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
			controller.scheduleDate = "2015-10-26";

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
			controller.scheduleDate = "2015-10-26";

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
			controller.scheduleDate = "2015-10-26";

			controller.searchOptions = {
				focusingSearch: false
			};

			controller.onSelectedTeamsChanged(['empty Team']);

			expect(controller.searchOptions.focusingSearch).toEqual(true);
		});

		it("should deactive search status after selected date changed", function () {
			controller.scheduleDate = "2015-10-26";

			controller.searchOptions = {
				focusingSearch: true
			};

			controller.onScheduleDateChanged();

			expect(controller.searchOptions.focusingSearch).toEqual(false);
		});

		it("should deactive search status after search text changed", function () {
			controller.scheduleDate = "2015-10-26";

			controller.searchOptions = {
				focusingSearch: true
			};

			controller.onKeyWordInSearchInputChanged();

			expect(controller.searchOptions.focusingSearch).toEqual(false);
		});

		it("should deactive search status after selected favorite changed", function () {
			controller.scheduleDate = "2015-10-26";

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
			controller.scheduleDate = "2015-10-26";
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

		it("should get warnings with correct data when validate warning is enabled", function () {
			controller.scheduleDate = "2018-11-02";

			controller.searchOptions = {
				focusingSearch: true
			};
			controller.currentSettings.validateWarningEnabled = true;
			controller.loadSchedules();
			controller.selectAllForAllPages();

			controller.checkValidationWarningForCurrentPage();
			$httpBackend.whenPOST('../api/TeamScheduleData/FetchRuleValidationResult', {
				Date: '2018-11-02',
				PersonIds: ['221B-Baker-SomeoneElse', '221B-Sherlock']
			}).respond([]);
			$httpBackend.flush();
		});

		it("should update warnings for the targets of a command with correct data", function () {
			controller.scheduleDate = "2018-11-02";

			controller.searchOptions = {
				focusingSearch: true
			};
			controller.currentSettings.validateWarningEnabled = true;
			controller.loadSchedules();
			controller.selectAllForAllPages();

			controller.checkValidationWarningForCommandTargets(['221B-Baker-SomeoneElse', '221B-Sherlock']);
			$httpBackend.whenPOST('../api/TeamScheduleData/FetchRuleValidationResult', {
				Date: '2018-11-02',
				PersonIds: ['221B-Baker-SomeoneElse', '221B-Sherlock']
			}).respond([]);
			$httpBackend.flush();
		});

		it("should remember skill selection when skill changed",
			function () {
				controller.scheduleDate = "2015-10-26";
				controller.staffingEnabled = true;

				controller.onSelectedSkillChanged({ Id: 'XYZ' });

				var config = staffingConfigStorageService.getConfig();
				expect(!!config).toEqual(true);
				expect(config.skillId).toEqual('XYZ');
				expect(!!config.skillGroupId).toEqual(false);
			});

		it("should read preselect skill when show staffing is enabled",
			function () {
				controller.scheduleDate = "2015-10-26";
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
				controller.scheduleDate = "2015-10-26";
				controller.staffingEnabled = true;

				controller.onSelectedSkillChanged(null, { Id: 'skillGroup' });

				var config = staffingConfigStorageService.getConfig();
				expect(!!config).toEqual(true);
				expect(config.skillGroupId).toEqual('skillGroup');
				expect(!!config.skillId).toEqual(false);
			});

		it("should read preselect skill group when show staffing is enabled and there is valid config",
			function () {
				controller.scheduleDate = "2015-10-26";
				staffingConfigStorageService.setSkill(null, 'skillGroup');
				controller.staffingEnabled = true;

				controller.showStaffing();
				expect(!!controller.preselectedSkills).toEqual(true);
				expect(!!controller.preselectedSkills.skillIds).toEqual(false);
				expect(controller.preselectedSkills.skillAreaId).toEqual('skillGroup');
			});

		it("should read preselect skill group when show staffing is enabled and there is invalid config",
			function () {
				controller.scheduleDate = "2015-10-26";
				controller.staffingEnabled = true;
				controller.showStaffing();

				expect(!!controller.preselectedSkills.skillIds).toEqual(false);
				expect(!!controller.preselectedSkills.skillAreaId).toEqual(false);
			});

		it("should remember use shrinkage status when change the use shrinkage status", function () {
			controller.scheduleDate = "2015-10-26";
			controller.staffingEnabled = true;

			controller.onUseShrinkageChanged(true);
			var config = staffingConfigStorageService.getConfig();
			expect(!!config).toEqual(true);
			expect(config.useShrinkage).toEqual(true);

		});

		it("should read prechecked status for use shrinkage when show staffing is enabled and there is valid config", function () {
			var state = {
				staffingEnabled: true
			};
			viewStateKeeper.save(state);
			staffingConfigStorageService.setShrinkage(true);

			controller = setUpController($controller);

			controller.scheduleDate = "2015-10-26";
			expect(controller.useShrinkage).toEqual(true);
		});

		it("should read prechecked status for use shrinkage when show staffing is enabled and there is invalid config", function () {
			controller.scheduleDate = "2015-10-26";
			controller.staffingEnabled = true;

			controller.showStaffing();
			expect(!!controller.useShrinkage).toEqual(false);
		});

		it("should init all settings state", function () {
			staffingConfigStorageService.setSkill(null, 'groupPage');

			var state = {
				selectedDate: '2018-05-28',
				keyword: 'jon',
				teamNameMap: '1 team selected',
				selectedSortOption: 'Name DESC',
				selectedGroupPage: { pageId: '', groupIds: ['group id 1'] },
				staffingEnabled: true,
				timezone: 'Europe/Berlin'
			};
			viewStateKeeper.save(state);

			controller = setUpController($controller);

			expect(controller.searchOptions.keyword).toEqual('jon');
			expect(controller.scheduleDate).toEqual('2018-05-28');
			expect(controller.teamNameMap).toEqual('1 team selected');
			expect(controller.sortOption).toEqual('Name DESC');
			expect(controller.selectedGroups.groupIds).toEqual(['group id 1']);
			expect(controller.selectedGroups.groupPageId).toEqual('');
			expect(controller.staffingEnabled).toEqual(true);
			expect(controller.currentTimezone).toEqual('Europe/Berlin');

			expect(!!controller.preselectedSkills.skillIds).toEqual(false);
			expect(controller.preselectedSkills.skillAreaId).toEqual('groupPage');
		});

		it('should clear person selection after schedule changed', function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = false;

			controller.scheduleDate = "2015-10-26";
			controller.loadSchedules();
			controller.selectAllForAllPages();

			var personSchedule1 = scheduleMgmt.groupScheduleVm.Schedules[0];
			expect(personSchedule1.IsSelected).toEqual(true);

			controller.onPersonScheduleChanged(["221B-Baker-SomeoneElse"]);

			expect(scheduleMgmt.groupScheduleVm.Schedules[0].IsSelected).toEqual(false);
			expect(scheduleMgmt.groupScheduleVm.Schedules[1].IsSelected).toEqual(false);
			expect(scheduleMgmt.groupScheduleVm.Schedules[2].IsSelected).toEqual(false);
			expect(Object.keys(personSelection.personInfo).length).toEqual(0);
		});

		it("should not update schedule when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;

			controller = setUpController($controller);
			controller.scheduleDate = "2018-12-17";
			controller.loadSchedules();
			rootScope.$digest();

			controller.onPersonScheduleChanged(["221B-Baker-SomeoneElse"]);
			expect(searchScheduleCalledTimes).toBe(0);
		});

		it("should update schedule when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is off", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = false;

			controller = setUpController($controller);
			controller.scheduleDate = "2018-12-17";
			controller.loadSchedules();
			rootScope.$digest();

			controller.onPersonScheduleChanged(["221B-Baker-SomeoneElse"]);
			expect(searchScheduleCalledTimes).toBe(1);
		});

		it("should show refresh button and disable on init when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;

			controller = setUpController($controller);
			controller.scheduleDate = "2018-12-17";

			expect(controller.isRefreshButtonVisible).toBeTruthy();
			expect(controller.havingScheduleChanged).toBeFalsy();
		});

		it("should enable refresh button when schedule is changed and WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;

			controller = setUpController($controller);
			controller.scheduleDate = "2018-12-17";

			controller.onPersonScheduleChanged(["221B-Baker-SomeoneElse"]);
			expect(controller.havingScheduleChanged).toBeTruthy();
		});

		it("should reload schedules for people having schedule changed after clicking refresh button when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;

			controller = setUpController($controller);
			controller.scheduleDate = "2018-12-17";

			controller.onPersonScheduleChanged(["221B-Baker-SomeoneElse", "221B-Sherlock"]);
			controller.onPersonScheduleChanged(["221B-Baker-SomeoneElse", "221B-Sherlock"]);

			controller.onRefreshButtonClicked();

			expect(searchScheduleCalledTimes).toBe(1);
			expect(teamScheduleService.currentAgentsForGettingSchedules()).toEqual(["221B-Baker-SomeoneElse", "221B-Sherlock"]);
			expect(controller.havingScheduleChanged).toBeFalsy();
		});

		xit("should disable the favorite button when editing shift editor", function () {
			controller = setUpController($controller);
			controller.scheduleDate = "2018-12-19";


		});

		function setUpController($controller) {
			return $controller("TeamScheduleController", {
				$scope: rootScope,
				personSelectionSvc: personSelection
			});
		};

		function setupMockTeamScheduleService(teamScheduleService) {
			var currentInput;
			var currentAgentsForGettingSchedules = [];

			teamScheduleService.currentInput = function () {
				return currentInput;
			}

			teamScheduleService.currentAgentsForGettingSchedules = function () {
				return currentAgentsForGettingSchedules;
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
									"StartInUtc": today + " 07:00",
									"Minutes": 480
								},
								{
									"ShiftLayerIds": ["activity2"],
									"Color": "#80FF80",
									"Description": "Email",
									"Start": today + " 15:00",
									"StartInUtc": today + " 15:00",
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
									"StartInUtc": today + " 08:00",
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
						cb({ data: {}});
						searchScheduleCalledTimes = searchScheduleCalledTimes + 1;
						currentAgentsForGettingSchedules = agents;
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
	});

	describe('#teamschedule controller tests for stateParams#', function () {
		var controller;

		beforeEach(function () {
			module('wfm.teamSchedule');

			module(function ($provide) {
				$provide.service('$stateParams', function () {
					return {
						personId: 'b46a2588-8861-42e3-ab03-9b5e015b257c',
						selectedDate: '2018-06-13'
					};
				});
				$provide.service('groupPageService', setUpMockGroupPagesService);
			});
		});

		beforeEach(inject(function (_$controller_, _$rootScope_) {
			controller = _$controller_("TeamScheduleController", {
				$scope: _$rootScope_.$new()
			});
		}));

		it('should get personIds and selected date from stateParams', function () {
			expect(controller.preSelectPersonIds[0]).toEqual('b46a2588-8861-42e3-ab03-9b5e015b257c');
			expect(moment(controller.scheduleDate).format('YYYY-MM-DD')).toEqual('2018-06-13');
		});
	});

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

})();

