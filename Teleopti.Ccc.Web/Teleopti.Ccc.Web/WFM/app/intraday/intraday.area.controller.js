(function() {
	'use strict';
	angular.module('wfm.intraday').controller('IntradayAreaController', intradayController);
	intradayController.$inject = [
		'$scope',
		'$state',
		'intradayService',
		'SkillGroupSvc',
		'$filter',
		'NoticeService',
		'$interval',
		'$timeout',
		'$translate',
		'intradayTrafficService',
		'intradayPerformanceService',
		'intradayMonitorStaffingService',
		'intradayLatestTimeService',
		'Toggle',
		'skillIconService',
		'CurrentUserInfo',
		'$log',
		'$rootScope'
	];
	function intradayController(
		$scope,
		$state,
		intradayService,
		SkillGroupSvc,
		$filter,
		NoticeService,
		$interval,
		$timeout,
		$translate,
		intradayTrafficService,
		intradayPerformanceService,
		intradayMonitorStaffingService,
		intradayLatestTimeService,
		toggleSvc,
		skillIconService,
		currentUserInfo,
		$log,
		$rootScope
	) {
		var vm = this;
		var timeoutPromise;
		var polling;
		var pollingTimeout = 60000;
		var loadingSkill = true;
		var loadingSkillArea = true;

		//TODO: #47051 - Remove this and its uses
		var message = $translate
			.instant('WFMReleaseNotificationWithoutOldModuleLink')
			.replace('{0}', $translate.instant('Intraday'))
			.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
			.replace('{2}', '</a>');
		var prevSkill;

		(vm.toggles = {}), (vm.DeleteSkillAreaModal = false);
		vm.activeTab = 0;
		vm.getSkillIcon = skillIconService.get;
		vm.viewObj;
		vm.chosenOffset = { value: 0 };
		vm.filterSkills = [];

		vm.changeChosenOffset = function(value, dontPoll) {
			$interval.cancel(polling);
			cancelTimeout();
			var d = angular.copy(vm.chosenOffset);
			d.value = value;
			vm.chosenOffset = d;
			if (!dontPoll) {
				pollActiveTabDataByDayOffset(vm.activeTab, value);
			}
			if (value === 0) {
				poll();
			}
		};

		vm.checkIfFilterSkill = function(skill) {
			if (vm.filterSkills.indexOf(skill) !== -1) {
				return 'mdi mdi-check';
			}
			return vm.getSkillIcon(skill);
		};

		vm.chipClass = function(skill) {
			if (!skill.DoDisplayData) {
				return 'chip-warning';
			}

			if (vm.filterSkills.indexOf(skill) !== -1) {
				return 'chip-success';
			}

			return;
		};

		vm.clearSkillHelper = function() {
			clearSkillSelection();
		};

		vm.clearSkillAreaHelper = function() {
			clearSkillAreaSelection();
		};

		vm.configMode = function() {
			$state.go('intraday.skill-area-config', { isNewSkillArea: false });
		};

		vm.deleteSkillArea = function(skillArea) {
			cancelTimeout();
			SkillGroupSvc.deleteSkillGroup({ Id: skillArea.Id }).then(function() {
				vm.skillAreas.splice(vm.skillAreas.indexOf(skillArea), 1);
				vm.selectedItem = null;
				vm.hasMonitorData = false;
				clearSkillAreaSelection();
				notifySkillAreaDeletion();
			});
			vm.toggleModal();
		};

		vm.exportIntradayData = function() {
			if (vm.selectedItem !== null && angular.isDefined(vm.selectedItem) && !vm.exporting) {
				vm.exporting = true;

				if (vm.selectedItem.Skills) {
					intradayService.getIntradayExportForSkillArea(
						angular.toJson({
							id: vm.selectedItem.Id,
							dayOffset: vm.chosenOffset.value
						}),
						saveData,
						errorSaveData
					);
				} else {
					intradayService.getIntradayExportForSkill(
						angular.toJson({
							id: vm.selectedItem.Id,
							dayOffset: vm.chosenOffset.value
						}),
						saveData,
						errorSaveData
					);
				}
			}
		};

		vm.getLocalDate = function(offset) {
			var userDate = getUserDateTime();
			var offset = userDate.add(offset, 'days').format('dddd, LL');

			return offset.charAt(0).toUpperCase() + offset.substr(1);
		};

		vm.getSkillGroupText = function() {
			return vm.toggles['WFM_Unified_Skill_Group_Management_45417']
				? $translate.instant('SelectSkillGroup')
				: $translate.instant('SelectSkillArea');
		};

		vm.onStateChanged = function(evt, to, params, from) {
			if (to.name !== 'intraday.area') return;
			if (params.isNewSkillArea === true) {
				reloadSkillAreas(true);
			} else reloadSkillAreas(false);
		};

		vm.openSkillGroupManager = function() {
			$state.go('intraday.skill-area-manager', {
				isNewSkillArea: false,
				selectedGroup: vm.selectedSkillArea
			});
		};

		vm.pollActiveTabDataHelper = function(activeTab) {
			pollData(activeTab);
			if (vm.chosenOffset.value === 0) {
				poll();
			}
		};

		vm.querySearch = function(query, myArray) {
			var results = query ? myArray.filter(createFilterFor(query)) : myArray,
				deferred;
			return results;
		};

		vm.selectSkillOrSkillArea = function(item) {
			if (angular.isUndefined(item) || item === null) {
				vm.selectedItem = null;
				return;
			}
			//Is it a skillgroup?
			vm.filterSkills = [];
			if (angular.isDefined(item.Skills)) {
				vm.skillAreaSelected(item);
				pollData(vm.activeTab);
				vm.prevArea = vm.selectedItem;
				item.UnsupportedSkills = [];
				checkUnsupported(item);
			} else {
				//Or a skill
				if (item.DoDisplayData) {
					vm.skillSelected(item);
					pollActiveTabDataByDayOffset(vm.activeTab, vm.chosenOffset.value);
					if (prevSkill) {
						if (!(prevSkill === vm.selectedSkill)) {
							clearPrev();
						}
					}
				} else {
					clearSkillSelection();
					UnsupportedSkillNotice();
				}
			}
		};

		vm.skillAreaSelected = function(item) {
			if (!item) return;
			vm.selectedItem = item;
			vm.selectedSkillArea = item;
			clearSkillSelection();
		};

		vm.skillSelected = function(item) {
			clearSkillAreaSelection();
			vm.selectedItem = item;
			vm.selectedSkill = item;
		};

		vm.toggleModal = function() {
			vm.DeleteSkillAreaModal = !vm.DeleteSkillAreaModal;
		};

		vm.saveState = function() {
			var state = {
				chosenOffset: vm.chosenOffset,
				selectedItem: vm.selectedItem,
				filterSkills: vm.filterSkills,
				activeTab: vm.activeTab
			};
			intradayService.saveIntradayState(state);
		};

		vm.loadState = function() {
			var state = intradayService.loadIntradayState();
			if (!state) return;
			if (angular.isDefined(state.selectedItem) && angular.isArray(vm.skills) && state.selectedItem !== null) {
				vm.selectSkillOrSkillArea(state.selectedItem);
				if (state.chosenOffset) {
					vm.changeChosenOffset(state.chosenOffset.value, state.chosenOffset.value !== 0);
				}
				if (angular.isDefined(state.selectedItem.Skills)) {
					vm.preselectedItem = { skillAreaId: state.selectedItem.Id };
				} else {
					vm.preselectedItem = { skillIds: [state.selectedItem.Id] };
				}
			}
			vm.filterSkills = state.filterSkills;
			vm.activeTab = state.activeTab;
		};

		function cancelTimeout() {
			if (timeoutPromise) {
				$timeout.cancel(timeoutPromise);
				timeoutPromise = undefined;
			}
		}

		function checkUnsupported(item) {
			if (!item.Skills || !vm.skills) return;
			for (var i = 0; i < item.Skills.length; i++) {
				for (var j = 0; j < vm.skills.length; j++) {
					if (item.Skills[i].Id === vm.skills[j].Id && vm.skills[j].DoDisplayData === false) {
						item.UnsupportedSkills.push(vm.skills[j]);
						item.Skills[i].DoDisplayData = false;
					} else if (item.Skills[i].Id === vm.skills[j].Id && vm.skills[j].DoDisplayData === true) {
						item.Skills[i].DoDisplayData = true;
					}
				}
			}
			if (item.UnsupportedSkills.length > 0) {
				vm.skillAreaMessage = $translate
					.instant('UnsupportedSkills')
					.replace('{0}', item.UnsupportedSkills.length);
			} else {
				vm.skillAreaMessage = '';
			}
		}

		function clearPrev() {
			vm.prevArea = false;
			prevSkill = false;
		}

		function clearSkillAreaSelection() {
			vm.selectedSkillArea = null;
		}

		function clearSkillSelection() {
			vm.selectedSkill = null;
		}

		function createFilterFor(query) {
			var lowercaseQuery = angular.lowercase(query);
			return function filterFn(item) {
				var lowercaseName = angular.lowercase(item.Name);
				return lowercaseName.indexOf(lowercaseQuery) === 0;
			};
		}

		function errorSaveData(data, status, headers, config) {
			vm.exporting = false;
		}

		function isSupported(skill) {
			return skill.DoDisplayData === true;
		}

		function notifySkillAreaDeletion() {
			var message = $translate.instant('Deleted');
			NoticeService.success(message, 5000, true);
		}

		function poll() {
			$interval.cancel(polling);
			polling = $interval(function() {
				pollData(vm.activeTab);
			}, pollingTimeout);
		}

		function pollActiveTabData(activeTab) {
			if (angular.isUndefined(activeTab)) return;

			var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];
			if (vm.selectedItem !== null && angular.isDefined(vm.selectedItem)) {
				if (vm.selectedItem.Skills) {
					services[activeTab].pollSkillAreaData(vm.selectedItem, vm.toggles);
					var timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
				} else {
					services[activeTab].pollSkillData(vm.selectedItem, vm.toggles);
					var timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
				}
				vm.viewObj = services[activeTab].getData();
				vm.latestActualInterval = timeData;
				vm.hasMonitorData = vm.viewObj.hasMonitorData;
			} else {
				timeoutPromise = $timeout(function() {
					pollActiveTabData(vm.activeTab);
				}, 1000);
			}
		}

		function pollActiveTabDataByDayOffset(activeTab, dayOffset) {
			if (angular.isUndefined(activeTab)) return;

			var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];
			var timeData;
			if (vm.selectedItem !== null && angular.isDefined(vm.selectedItem)) {
				if (vm.selectedItem.Skills) {
					services[activeTab].pollSkillAreaDataByDayOffset(vm.selectedItem, vm.toggles, dayOffset);
					if (dayOffset === 0) {
						timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
					}
				} else {
					services[activeTab].pollSkillDataByDayOffset(vm.selectedItem, vm.toggles, dayOffset);
					if (dayOffset === 0) {
						timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
					}
				}
				vm.viewObj = services[activeTab].getData();
				vm.latestActualInterval = timeData;
				vm.hasMonitorData = vm.viewObj.hasMonitorData;
			} else {
				timeoutPromise = $timeout(function() {
					pollActiveTabDataByDayOffset(vm.activeTab, dayOffset);
				}, 10000);
			}
		}

		function pollData(activeTab) {
			if (vm.toggles['WFM_Intraday_Show_For_Other_Days_43504']) {
				pollActiveTabDataByDayOffset(activeTab, vm.chosenOffset.value);
			} else {
				pollActiveTabData(activeTab);
			}
		}

		function reloadSkillAreas(isNew) {
			SkillGroupSvc.getSkillGroups().then(function(result) {
				vm.skillAreas = $filter('orderBy')(result.data.SkillAreas, 'Name');
				if (isNew) {
					vm.latest = $filter('orderBy')(result.data.SkillAreas, 'created_at', true);
					vm.latest = $filter('orderBy')(result.data.SkillAreas, 'Name');
				}
				vm.HasPermissionToModifySkillArea = result.data.HasPermissionToModifySkillArea;

				SkillGroupSvc.getSkills().then(function(result) {
					vm.skills = result.data;
					if (vm.skillAreas.length === 0) {
						vm.selectedItem = vm.skills.find(isSupported);
					}
					if (vm.skillAreas.length > 0) {
						if (isNew) {
							vm.selectedItem = vm.latest[0];
						} else {
							vm.selectedItem = vm.skillAreas[0];
						}
					}
					if (vm.toggles['WFM_Remember_My_Selection_In_Intraday_47254']) vm.loadState();
				});
			});
		}

		function saveData(data, status, headers, config) {
			var blob = new Blob([data]);
			vm.exporting = false;
			saveAs(blob, 'IntradayExportedData ' + moment().format('YYYY-MM-DD') + '.xlsx');
		}

		function selectedSkillInAreaChange(skill) {
			if (skill) {
				if (skill.DoDisplayData) {
					pollActiveTabDataByDayOffset(vm.activeTab, vm.chosenOffset.value);
				} else {
					vm.selectedItem = vm.selectedSkillArea;
					UnsupportedSkillNotice();
				}
			}
		}

		function UnsupportedSkillNotice() {
			var notPhoneMessage = $translate.instant('UnsupportedSkillMsg');
			NoticeService.warning(notPhoneMessage, 5000, true);
		}

		function getUserDateTime() {
			return moment.tz(moment(), currentUserInfo.CurrentUserInfo().DefaultTimeZone);
		}

		$scope.$on('$destroy', function(event) {
			$interval.cancel(polling);
			cancelTimeout();
			if (vm.toggles['WFM_Remember_My_Selection_In_Intraday_47254']) vm.saveState();
		});

		$scope.$on('$locationChangeStart', function() {
			cancelTimeout();
		});

		$scope.$on('$stateChangeSuccess', vm.onStateChanged);

		toggleSvc.togglesLoaded.then(function() {
			vm.toggles = toggleSvc;
		});

		if (vm.latestActualInterval === '--:--') {
			vm.hasMonitorData = false;
		}

		NoticeService.info(message, null, true);

		// $timeout(
		// 	function() {
		// 		vm.loadState();
		// 	},
		// 	0,
		// 	false
		// );

		poll();
	}
})();
