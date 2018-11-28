(function() {
	'use strict';
	angular.module('wfm.intraday').controller('IntradayAreaController', IntradayAreaController);
	IntradayAreaController.$inject = [
		'$scope',
		'$state',
		'SkillGroupSvc',
		'intradayService',
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
		'CurrentUserInfo'
	];
	function IntradayAreaController(
		$scope,
		$state,
		SkillGroupSvc,
		intradayService,
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
		currentUserInfo
	) {
		var vm = this;
		var polling;
		var pollingTimeout = 60000;

		vm.timeoutPromise = null;
		vm.viewObj;
		vm.getSkillIcon = skillIconService.get;
		vm.toggles = {};
		vm.skillGroups = [];
		vm.waitingForData = false;

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
			if (!vm.moduleState.showIncluded || !skill) return;

			if (vm.moduleState.selectedSkill !== null && vm.moduleState.selectedSkill.Id === skill.Id) {
				return 'mdi mdi-check';
			}
			return vm.getSkillIcon(skill);
		};

		vm.chipClass = function(skill) {
			if (!skill.DoDisplayData) {
				return 'chip-warning';
			}

			if (vm.moduleState.selectedSkill !== null && vm.moduleState.selectedSkill.Id === skill.Id) {
				return 'chip-success';
			}

			return;
		};

		function skillOrGroupIsSelected() {
			return (
				(vm.moduleState.selectedSkill !== null && angular.isDefined(vm.moduleState.selectedSkill)) ||
				(vm.moduleState.selectedSkillGroup !== null && angular.isDefined(vm.moduleState.selectedSkillGroup))
			);
		}

		vm.exportIntradayData = function() {
			if (skillOrGroupIsSelected() && !vm.exporting) {
				vm.exporting = true;
				if (
					angular.isDefined(vm.moduleState.selectedSkillGroup) &&
					vm.moduleState.selectedSkillGroup !== null
				) {
					intradayService.getIntradayExportForSkillGroup(
						angular.toJson({
							id: vm.moduleState.selectedSkillGroup.Id,
							dayOffset: vm.moduleState.chosenOffset.value
						}),
						saveData,
						errorSaveData
					);
				} else if (angular.isDefined(vm.moduleState.selectedSkill) && vm.moduleState.selectedSkill !== null) {
					intradayService.getIntradayExportForSkill(
						angular.toJson({
							id: vm.moduleState.selectedSkill.Id,
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

		vm.onStateChanged = function(evt, to, params, from) {
			if (params.isNewSkillArea === true) {
				reloadSkillGroups(true);
			} else reloadSkillGroups(false);
		};

		vm.openSkillFromArea = function(skill) {
			if (!vm.isSkill(skill)) return;

			if (skill.DoDisplayData) {
				//Clicked another skill
				if (vm.moduleState.selectedSkill === null || vm.moduleState.selectedSkill.Id !== skill.Id) {
					vm.setSkill(skill);
				} else {
					//Clicked on same skill again
					vm.moduleState.selectedSkill = null;
					//vm.skillPickerText = '';
					vm.setSkillGroup(vm.moduleState.selectedSkillGroup);
				}
			} else {
				UnsupportedSkillNotice();
			}
			vm.saveState();
		};

		vm.openSkillGroupManager = function() {
			$state.go('intraday.skill-group-manager', {
				isNewSkillArea: false,
				selectedGroup: vm.moduleState.selectedSkillGroup
			});
		};

		vm.pollActiveTabDataHelper = function(activeTab) {
			pollActiveTabDataByDayOffset(activeTab, vm.moduleState.chosenOffset.value);
			if (vm.moduleState.chosenOffset.value === 0) {
				poll();
			}
		};

		vm.querySearch = function(query, myArray) {
			var results = query ? myArray.filter(createFilterFor(query)) : myArray,
				deferred;
			return results;
		};

		vm.resetFilter = function() {
			setModuleState({
				showIncluded: false
			});
		};

		vm.clickedSkillInPicker = function(skill) {
			if (skill.Id === -1) {
				vm.skillPickerOpen = false;
				return;
			}
			vm.setSkill(skill);
			setModuleState({
				showIncluded: false,
				selectedSkillGroup: null
			});
		};

		vm.clearSkillSelection = function() {
			setModuleState({
				showGroupInfo: false,
				showIncluded: false,
				selectedSkill: null,
				selectedSkillGroup: null,
				hasMonitorData: false
			});
			vm.saveState();
		};

		vm.clearSkillGroupSelection = function() {
			setModuleState({
				showGroupInfo: false,
				showIncluded: false,
				selectedSkill: null,
				selectedSkillGroup: null,
				hasMonitorData: false
			});
			vm.saveState();
		};

		vm.clickedSkillGroupInPicker = function(skillGroup) {
			vm.moduleState.selectedSkill = null;
			if (skillGroup.Id === -1) {
				vm.skillGroupPickerOpen = false;
				return;
			}

			vm.setSkillGroup(skillGroup);
		};

		vm.setSkill = function(skill) {
			if (angular.isUndefined(skill) || skill === null) return;
			vm.skillPickerOpen = false;
			vm.skillPickerText = skill.Name;
			if (skill.DoDisplayData) {
				vm.moduleState.selectedSkill = skill;
				pollActiveTabDataByDayOffset(vm.moduleState.activeTab, vm.moduleState.chosenOffset.value);
			} else {
				UnsupportedSkillNotice();
			}
		};

		vm.setSkillGroup = function(skillGroup) {
			if (angular.isUndefined(skillGroup) || skillGroup === null) return;
			vm.skillGroupPickerOpen = false;
			setModuleState({
				showIncluded: true,
				showGroupInfo: true,
				selectedSkillGroup: skillGroup
			});
			pollActiveTabDataByDayOffset(
				vm.moduleState.activeTab,
				vm.moduleState.chosenOffset.value,
				gotDataFromService
			);
		};

		vm.saveState = function() {
			intradayService.saveIntradayState(vm.moduleState);
		};

		vm.isSkill = function(item) {
			if (!item) return false;
			if (angular.isUndefined(item.Skills)) return true;
			return false;
		};

		vm.loadState = function() {
			setModuleState(intradayService.loadIntradayState());
			if (!vm.moduleState) return;
			if (vm.moduleState.chosenOffset) {
				vm.changeChosenOffset(vm.moduleState.chosenOffset.value, vm.moduleState.chosenOffset.value !== 0);
			}

			if (vm.moduleState.showIncluded && vm.moduleState.selectedSkill && vm.moduleState.selectedSkillGroup) {
				vm.setSkill(vm.moduleState.selectedSkill);
				vm.checkIfFilterSkill(vm.moduleState.selectedSkill);
				return;
			}

			if (
				angular.isDefined(vm.moduleState.selectedSkillGroup) &&
				vm.moduleState.selectedSkillGroup !== null &&
				!vm.moduleState.selectedSkill
			) {
				vm.clickedSkillGroupInPicker(vm.moduleState.selectedSkillGroup);
				return;
			}
			if (
				angular.isDefined(vm.moduleState.selectedSkill) &&
				vm.moduleState.selectedSkill !== null &&
				!vm.moduleState.selectedSkillGroup
			) {
				vm.clickedSkillInPicker(vm.moduleState.selectedSkill);
				return;
			}
		};

		function cancelTimeout() {
			if (vm.timeoutPromise) {
				$timeout.cancel(vm.timeoutPromise);
				vm.timeoutPromise = undefined;
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
				vm.skillGroupMessage = $translate
					.instant('UnsupportedSkills')
					.replace('{0}', item.UnsupportedSkills.length);
			} else {
				vm.skillGroupMessage = '';
			}
		}

		function createFilterFor(query) {
			var lowercaseQuery = angular.lowercase(query);
			return function filterFn(item) {
				var lowercaseName = angular.lowercase(item.Name);
				return lowercaseName.indexOf(lowercaseQuery) === 0;
			};
		}


		function isSupported(skill) {
			return skill.DoDisplayData === true;
		}

		function poll() {
			$interval.cancel(polling);
			polling = $interval(function() {
				pollActiveTabDataByDayOffset(vm.moduleState.activeTab, vm.moduleState.chosenOffset.value);
			}, pollingTimeout);
		}

		function gotDataFromService(data) {
			vm.viewObj = Object.assign({}, data);
			vm.moduleState.hasMonitorData = vm.viewObj.hasMonitorData;
			vm.waitingForData = false;
			if (data.error) {
				cancelTimeout();
				if (data.error.status === 403) location.reload();
			}
		}

		function gotTimeData(data) {
			vm.latestActualInterval = data;
		}

		function pollActiveTabDataByDayOffset(activeTab, dayOffset) {
			if (angular.isUndefined(activeTab)) return;
			if (angular.isUndefined(dayOffset)) dayOffset = 0;

			var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];
			if (skillOrGroupIsSelected()) {
				if (vm.moduleState.selectedSkill) {
					vm.waitingForData = true;
					services[activeTab].pollSkillDataByDayOffset(
						vm.moduleState.selectedSkill,
						vm.toggles,
						dayOffset,
						gotDataFromService
					);
					if (dayOffset === 0) {
						intradayLatestTimeService.pollTime(vm.moduleState.selectedSkill, gotTimeData);
					}
				} else if (vm.moduleState.selectedSkillGroup) {
					vm.waitingForData = true;
					services[activeTab].pollSkillGroupDataByDayOffset(
						vm.moduleState.selectedSkillGroup,
						vm.toggles,
						dayOffset,
						gotDataFromService
					);
					if (dayOffset === 0) {
						intradayLatestTimeService.pollTime(vm.moduleState.selectedSkillGroup, gotTimeData);
					}
				}
			} else {
				vm.timeoutPromise = $timeout(function() {
					pollActiveTabDataByDayOffset(vm.moduleState.activeTab, dayOffset);
				}, 10000);
			}
		}

		function reloadSkillGroups(isNew) {
			SkillGroupSvc.getSkillGroups().then(function(result) {
				vm.skillGroups = $filter('orderBy')(result.data.SkillAreas, 'Name');
				if (isNew) {
					vm.latest = $filter('orderBy')(result.data.SkillAreas, 'created_at', true);
					vm.latest = $filter('orderBy')(result.data.SkillAreas, 'Name');
				}
				vm.HasPermissionToModifySkillGroup = result.data.HasPermissionToModifySkillArea;
				if (angular.isUndefined(vm.skillGroups)) vm.skillGroups = [];
				if (vm.skillGroups.length === 0) {
					vm.skillGroups.push({
						Name: $translate.instant('NoSkillGroupsFound'),
						Id: -1
					});
				}
				SkillGroupSvc.getSkills().then(function(result) {
					vm.skills = result.data;
					vm.loadState();
				});
				if (angular.isUndefined(vm.skills)) vm.skills = [];
				if (vm.skills.length === 0) {
					vm.skills.push({
						Name: $translate.instant('NoSkillFound'),
						Id: -1
					});
				}
			});
		}

		function saveData(data, status, headers, config) {
			var blob = new Blob([data]);
			vm.exporting = false;
			saveAs(blob, 'IntradayExportedData ' + moment().format('YYYY-MM-DD') + '.xlsx');
		}
	
		function errorSaveData(data, status, headers, config) {
			NoticeService.warning(
				"<span class='test-alert'></span>" + $translate.instant('FailedExportToExcel'),
				null,
				false
			);
			vm.exporting = false;
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
			setModuleState({
				activeTab: 0,
				chosenOffset: { value: 0 },
				showGroupInfo: false,
				showIncluded: false,
				selectedSkill: null,
				selectedSkillGroup: null,
				hasMonitorData: false
			});
		}

		$scope.$on('$destroy', function(event) {
			$interval.cancel(polling);
			cancelTimeout();
			vm.saveState();
		});

		$scope.$on('$locationChangeStart', function() {
			cancelTimeout();
		});

		$scope.$on('$stateChangeSuccess', function(evt, to, params, from) {
			vm.onStateChanged(evt, to, params, from);
		});

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
