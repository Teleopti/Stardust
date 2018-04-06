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

		vm.viewObj;
		vm.getSkillIcon = skillIconService.get;
		vm.toggles = {};
		vm.skillAreas = [];

		vm.changeChosenOffset = function(value, dontPoll) {
			$interval.cancel(polling);
			cancelTimeout();
			var d = angular.copy(vm.moduleState.chosenOffset);
			d.value = value;
			vm.moduleState.chosenOffset = d;
			if (!dontPoll) {
				pollActiveTabDataByDayOffset(vm.moduleState.activeTab, value);
			}
			if (value === 0) {
				poll();
			}
			vm.saveState();
		};

		vm.checkIfFilterSkill = function(skill) {
			if (vm.moduleState.filterSkills.indexOf(skill) !== -1) {
				return 'mdi mdi-check';
			}
			return vm.getSkillIcon(skill);
		};

		vm.chipClass = function(skill) {
			if (!skill.DoDisplayData) {
				return 'chip-warning';
			}

			if (vm.moduleState.filterSkills.indexOf(skill) !== -1) {
				return 'chip-success';
			}

			return;
		};

		vm.moduleState = function() {
			$state.go('intraday.skill-area-config', { isNewSkillArea: false });
		};

		vm.deleteSkillArea = function(skillArea) {
			cancelTimeout();
			SkillGroupSvc.deleteSkillGroup({ Id: skillArea.Id }).then(function() {
				vm.skillAreas.splice(vm.skillAreas.indexOf(skillArea), 1);
				setModuleState({
					selectedItem: null,
					hasMonitorData: false
				});
			});
			vm.toggleModal();
		};

		vm.exportIntradayData = function() {
			if (
				vm.moduleState.selectedItem !== null &&
				angular.isDefined(vm.moduleState.selectedItem) &&
				!vm.exporting
			) {
				vm.exporting = true;

				if (vm.moduleState.selectedItem.Skills) {
					intradayService.getIntradayExportForSkillArea(
						angular.toJson({
							id: vm.moduleState.selectedItem.Id,
							dayOffset: vm.moduleState.chosenOffset.value
						}),
						saveData,
						errorSaveData
					);
				} else {
					intradayService.getIntradayExportForSkill(
						angular.toJson({
							id: vm.moduleState.selectedItem.Id,
							dayOffset: vm.moduleState.chosenOffset.value
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

		vm.openSkillFromArea = function(skill) {
			if (skill.DoDisplayData) {
				var skillIndex = vm.moduleState.filterSkills.indexOf(skill);
				if (skillIndex === -1) {
					vm.moduleState.filterSkills = [skill];
					vm.selectSkillOrSkillArea(skill, true);
				} else {
					vm.moduleState.filterSkills = [];
					vm.selectSkillOrSkillArea(skill, true);
					vm.moduleState.filterSkills.splice(skillIndex, 1);
				}
			} else {
				UnsupportedSkillNotice();
			}
			vm.saveState();
		};

		vm.openSkillGroupManager = function() {
			$state.go('intraday.skill-area-manager', {
				isNewSkillArea: false,
				selectedGroup: vm.isSkill(vm.moduleState.selectedItem) ? null : vm.moduleState.selectedItem
			});
		};

		vm.pollActiveTabDataHelper = function(activeTab) {
			pollData(activeTab);
			if (vm.moduleState.chosenOffset.value === 0) {
				poll();
			}
		};

		vm.querySearch = function(query, myArray) {
			var results = query ? myArray.filter(createFilterFor(query)) : myArray,
				deferred;
			return results;
		};

		vm.selectSkillOrSkillArea = function(item, showSkillChips) {
			if (!item) {
				vm.moduleState.selectedItem = null;
				return;
			}

			if (angular.isUndefined(item.Name) || angular.isUndefined(item)) {
				vm.moduleState.selectedItem = null;
				return;
			}

			if (showSkillChips && vm.moduleState.filterSkills.length === 0) {
				item = vm.moduleState.currentArea;
			}

			if (vm.isSkill(item)) {
				setModuleState({
					showIncluded: showSkillChips,
					showGroupInfo: showSkillChips,
					filterSkills: []
				});
				if (showSkillChips === false) {
					vm.moduleState.currentArea = null;
				}
				vm.setSkill(item);
			} else {
				vm.setSkillGroup(item);
			}
			vm.saveState();
		};

		vm.setSkill = function(skill) {
			if (angular.isUndefined(skill) || skill === null) return;

			vm.moduleState.preselectedItem = { skillIds: [skill.Id] };
			if (skill.DoDisplayData) {
				vm.moduleState.selectedItem = skill;
				pollActiveTabDataByDayOffset(vm.moduleState.activeTab, vm.moduleState.chosenOffset.value);
			} else {
				UnsupportedSkillNotice();
			}
		};

		vm.setSkillGroup = function(skillGroup) {
			if (angular.isUndefined(skillGroup) || skillGroup === null) return;

			setModuleState({
				showIncluded: true,
				showGroupInfo: true,
				currentArea: skillGroup,
				selectedItem: skillGroup
			});
			pollActiveTabDataByDayOffset(vm.moduleState.activeTab, vm.moduleState.chosenOffset.value);
			vm.moduleState.preselectedItem = { skillAreaId: skillGroup.Id };
			// skillGroup.UnsupportedSkills = [];
			// checkUnsupported(skillGroup);
		};

		vm.toggleModal = function() {
			vm.moduleState.DeleteSkillAreaModal = !vm.moduleState.DeleteSkillAreaModal;
		};

		vm.saveState = function() {
			if (!vm.toggles['WFM_Remember_My_Selection_In_Intraday_47254']) return;
			intradayService.saveIntradayState(vm.moduleState);
		};

		vm.isSkill = function(item) {
			if (!item) return false;
			if (angular.isUndefined(item.Skills)) return true;
			return false;
		};

		vm.loadState = function() {
			if (vm.toggles['WFM_Remember_My_Selection_In_Intraday_47254']) {
				setModuleState(intradayService.loadIntradayState());
			} else {
				resetModuleState();
			}
			if (!vm.moduleState) return;
			if (vm.moduleState.chosenOffset) {
				vm.changeChosenOffset(vm.moduleState.chosenOffset.value, vm.moduleState.chosenOffset.value !== 0);
			}
			if (vm.isSkill(vm.moduleState.selectedItem)) {
				vm.setSkill(vm.moduleState.selectedItem);
			} else {
				vm.setSkillGroup(vm.moduleState.selectedItem);
			}
			if (vm.moduleState.currentArea && vm.moduleState.currentArea !== null) {
				vm.openSkillFromArea(vm.moduleState.selectedItem);
			}
			// else {
			// 	vm.selectSkillOrSkillArea(vm.moduleState.selectedItem, false);
			// }
		};

		function cancelTimeout() {
			if (timeoutPromise) {
				$timeout.cancel(timeoutPromise);
				timeoutPromise = undefined;
			}
		}

		function checkUnsupported(item) {
			if (vm.isSkill(item) || !vm.skills) return;
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

		function poll() {
			$interval.cancel(polling);
			polling = $interval(function() {
				pollData(vm.moduleState.activeTab);
			}, pollingTimeout);
		}

		function pollActiveTabDataByDayOffset(activeTab, dayOffset) {
			if (angular.isUndefined(activeTab)) return;

			var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];
			var timeData;
			var promise;
			if (vm.moduleState.selectedItem !== null && angular.isDefined(vm.moduleState.selectedItem)) {
				if (vm.isSkill(vm.moduleState.selectedItem)) {
					promise = services[activeTab].pollSkillDataByDayOffset(
						vm.moduleState.selectedItem,
						vm.toggles,
						dayOffset
					);
					if (dayOffset === 0) {
						timeData = intradayLatestTimeService.getLatestTime(vm.moduleState.selectedItem);
					}
				} else {
					promise = services[activeTab].pollSkillAreaDataByDayOffset(
						vm.moduleState.selectedItem,
						vm.toggles,
						dayOffset
					);
					if (dayOffset === 0) {
						timeData = intradayLatestTimeService.getLatestTime(vm.moduleState.selectedItem);
					}
				}

				promise.then(function(data) {
					vm.viewObj = Object.assign({}, data);
					vm.moduleState.hasMonitorData = vm.viewObj.hasMonitorData;
					vm.latestActualInterval = timeData;
					$log.log('vm.viewObj', vm.viewObj);
				});
			} else {
				timeoutPromise = $timeout(function() {
					pollActiveTabDataByDayOffset(vm.moduleState.activeTab, dayOffset);
				}, 10000);
			}
		}

		function pollData(activeTab) {
			if (vm.toggles['WFM_Intraday_Show_For_Other_Days_43504']) {
				pollActiveTabDataByDayOffset(activeTab, vm.moduleState.chosenOffset.value);
			} else {
				pollActiveTabDataByDayOffset(activeTab, 0);
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
						vm.moduleState.selectedItem = vm.skills.find(isSupported);
					}
					if (vm.skillAreas.length > 0) {
						if (isNew) {
							vm.moduleState.selectedItem = vm.latest[0];
						} else {
							vm.moduleState.selectedItem = vm.skillAreas[0];
						}
					}
					vm.loadState();
				});
			});
		}

		function saveData(data, status, headers, config) {
			var blob = new Blob([data]);
			vm.exporting = false;
			saveAs(blob, 'IntradayExportedData ' + moment().format('YYYY-MM-DD') + '.xlsx');
		}

		function UnsupportedSkillNotice() {
			var notPhoneMessage = $translate.instant('UnsupportedSkillMsg');
			NoticeService.warning(notPhoneMessage, 5000, true);
		}

		function getUserDateTime() {
			return moment.tz(moment(), currentUserInfo.CurrentUserInfo().DefaultTimeZone);
		}

		function setModuleState(props) {
			vm.moduleState = Object.assign({}, vm.moduleState, props);
		}

		function resetModuleState() {
			vm.moduleState = {
				DeleteSkillAreaModal: false,
				activeTab: 0,
				chosenOffset: { value: 0 },
				filterSkills: [],
				showGroupInfo: false,
				showIncluded: false,
				currentArea: null,
				selectedItem: null,
				hasMonitorData: false,
				preselectedItem: null
			};
		}

		$scope.$on('$destroy', function(event) {
			$interval.cancel(polling);
			cancelTimeout();
			vm.saveState();
		});

		$scope.$on('$locationChangeStart', function() {
			cancelTimeout();
		});

		$scope.$on('$stateChangeSuccess', vm.onStateChanged);

		toggleSvc.togglesLoaded.then(function() {
			vm.toggles = toggleSvc;
		});

		if (vm.latestActualInterval === '--:--') {
			vm.moduleState.hasMonitorData = false;
		}

		resetModuleState();

		poll();
	}
})();
