(function () {
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
		'$compile',
		'$translate',
		'intradayTrafficService',
		'intradayPerformanceService',
		'intradayMonitorStaffingService',
		'intradayLatestTimeService',
		'Toggle',
		'skillIconService',
		'$log'
	];

	function intradayController($scope,
								$state,
								intradayService,
								SkillGroupSvc,
								$filter,
								NoticeService,
								$interval,
								$timeout,
								$compile,
								$translate,
								intradayTrafficService,
								intradayPerformanceService,
								intradayMonitorStaffingService,
								intradayLatestTimeService,
								toggleSvc,
								skillIconService,
								$log) {
		var vm = this;
		var autocompleteSkill;
		var autocompleteSkillArea;
		var timeoutPromise;
		var polling;
		var pollingTimeout = 60000;
		vm.selectedSkillArea;
		vm.selectedSkill;
		vm.DeleteSkillAreaModal = false;
		vm.prevArea;
		vm.drillable;
		vm.activeTab = 0;
		vm.getSkillIcon = skillIconService.get;
		vm.toggles = {
			showOptimalStaffing: [],
			showScheduledStaffing: [],
			showEsl: [],
			showEmailSkill: [],
			showOtherDay: [],
			exportToExcel: [],
			otherSkillsLikeEmail: [],
			unifiedSkillGroupManagement: []
		};
		var message = $translate
			.instant('WFMReleaseNotificationWithoutOldModuleLink')
			.replace('{0}', $translate.instant('Intraday'))
			.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
			.replace('{2}', '</a>');
		var prevSkill;
		vm.currentInterval = [];
		vm.format = intradayService.formatDateTime;
		vm.viewObj;
		vm.chosenOffset = {value: 0};
		NoticeService.info(message, null, true);
		toggleSvc.togglesLoaded.then(function () {
			vm.toggles.showOptimalStaffing = toggleSvc.Wfm_Intraday_OptimalStaffing_40921;
			vm.toggles.showScheduledStaffing = toggleSvc.Wfm_Intraday_ScheduledStaffing_41476;
			vm.toggles.showEsl = toggleSvc.Wfm_Intraday_ESL_41827;
			vm.toggles.showEmailSkill = toggleSvc.Wfm_Intraday_SupportSkillTypeEmail_44002;
			vm.toggles.showOtherDay = toggleSvc.WFM_Intraday_Show_For_Other_Days_43504;
			vm.toggles.exportToExcel = toggleSvc.WFM_Intraday_Export_To_Excel_44892;
			vm.toggles.otherSkillsLikeEmail = toggleSvc.WFM_Intraday_SupportOtherSkillsLikeEmail_44026;
			vm.toggles.unifiedSkillGroupManagement = toggleSvc.WFM_Unified_Skill_Group_Management_45417;
		});

		var getAutoCompleteControls = function () {
			var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
			autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

			var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
			autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
		};

		vm.openSkillFromArea = function (item) {
			if (item.DoDisplayData) {
				prevSkill = item;
				vm.selectedSkill = item;
				vm.drillable = true;
			} else {
			 	UnsupportedSkillNotice();
			}
		};

		vm.openSkillAreaFromSkill = function () {
			vm.selectedSkillArea = vm.prevArea;
			vm.drillable = false;
		};

		vm.skillSelected = function (item) {
			clearSkillAreaSelection();
			vm.selectedItem = vm.selectedSkill = item;
		};

		vm.skillAreaSelected = function (item) {
			vm.selectedItem = vm.selectedSkillArea = item;
			clearSkillSelection();
		};

		vm.deleteSkillArea = function (skillArea) {
			cancelTimeout();
			SkillGroupSvc.deleteSkillGroup
				.remove({
					id: skillArea.Id
				})
				.$promise.then(function (result) {
				vm.skillAreas.splice(vm.skillAreas.indexOf(skillArea), 1);
				vm.selectedItem = null;
				vm.hasMonitorData = false;
				clearSkillAreaSelection();
				notifySkillAreaDeletion();
			});

			vm.toggleModal();
		};

		var clearPrev = function () {
			vm.drillable = false;
			vm.prevArea = false;
			prevSkill = false;
		};

		vm.selectedSkillChange = function (item) {
			if (item) {
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
			} else if(vm.selectedSkillArea === null) {
				vm.selectedItem = null;
			}
		};

		function UnsupportedSkillNotice() {
			var notPhoneMessage = $translate.instant('UnsupportedSkillMsg');
			NoticeService.warning(notPhoneMessage, 5000, true);
		}

		vm.selectedSkillAreaChange = function (item) {
			if (item) {
				vm.skillAreaSelected(item);
				pollData(vm.activeTab);

				vm.prevArea = vm.selectedItem;
				item.UnsupportedSkills = [];
				checkUnsupported(item);
			} else if (vm.selectedSkill === null){
				vm.selectedItem = null;
			}
			if (vm.drillable === true && vm.selectedItem && vm.selectedItem.skills) {
				vm.drillable = false;
			}
			
		};

		function isSupported(skill) {
			return skill.DoDisplayData === true;
		}

		var reloadSkillAreas = function (isNew) {
			SkillGroupSvc.getSkillGroups.query().$promise.then(function (result) {
				getAutoCompleteControls();
				vm.skillAreas = $filter('orderBy')(result.SkillAreas, 'Name');
				if (isNew) {
					vm.latest = $filter('orderBy')(result.SkillAreas, 'created_at', true);
					vm.latest = $filter('orderBy')(result.SkillAreas, 'Name');
					//TO DO: get a date to filter by
					// vm.latest = $filter('orderBy')(result.SkillAreas, 'created_at', true);
				}
				vm.HasPermissionToModifySkillArea = result.HasPermissionToModifySkillArea;

				intradayService.getSkills.query().$promise.then(function (result) {
					vm.skills = result;
					if (vm.skillAreas.length === 0) {
						vm.selectedItem = vm.selectedItem = vm.skills.find(isSupported);
						if (autocompleteSkill) {
							vm.selectedSkill = vm.selectedItem;
						}
					}
					if (vm.skillAreas.length > 0) {
						if (isNew) {
							vm.selectedItem = vm.latest[0];
							if (autocompleteSkillArea) vm.selectedSkillArea = vm.selectedItem;
						} else {
							vm.selectedItem = vm.skillAreas[0];
							if (autocompleteSkillArea) vm.selectedSkillArea = vm.selectedItem;
						}
					}
				});
			});
		};

		var checkUnsupported = function (item) {
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
		};

		vm.clearSkillHelper = function () {
			clearSkillSelection();
		};

		vm.clearSkillAreaHelper = function () {
			clearSkillAreaSelection();
		};

		function clearSkillSelection() {
			if (!autocompleteSkill) return;
			vm.selectedSkill = null;
			vm.searchSkillText = '';
			vm.drillable = false;
		}

		function clearSkillAreaSelection() {
			if (!autocompleteSkillArea) return;
			vm.selectedSkillArea = null;
			vm.searchSkillAreaText = '';
		}

		vm.querySearch = function (query, myArray) {
			var results = query ? myArray.filter(createFilterFor(query)) : myArray,
				deferred;
			return results;
		};

		function createFilterFor(query) {
			var lowercaseQuery = angular.lowercase(query);
			return function filterFn(item) {
				var lowercaseName = angular.lowercase(item.Name);
				return lowercaseName.indexOf(lowercaseQuery) === 0;
			};
		}

		if (vm.latestActualInterval === '--:--') {
			vm.hasMonitorData = false;
		}

		var cancelTimeout = function () {
			if (timeoutPromise) {
				$timeout.cancel(timeoutPromise);
				timeoutPromise = undefined;
			}
		};

		function poll() {
			$interval.cancel(polling);
			polling = $interval(function () {
				pollData(vm.activeTab);
			}, pollingTimeout);
		}

		poll();

		vm.pollActiveTabDataHelper = function (activeTab) {
			pollData(activeTab);
			if (vm.chosenOffset.value === 0) {
				poll();
			}
		};

		function pollData(activeTab) {
			if (vm.toggles.showOtherDay) {
				pollActiveTabDataByDayOffset(activeTab, vm.chosenOffset.value);
			} else {
				pollActiveTabData(activeTab);
			}
		}

		function pollActiveTabData(activeTab) {
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
				timeoutPromise = $timeout(function () {
					pollActiveTabData(vm.activeTab);
				}, 1000);
			}
		}

		function pollActiveTabDataByDayOffset(activeTab, dayOffset) {
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
				timeoutPromise = $timeout(function () {
					pollActiveTabDataByDayOffset(vm.activeTab, dayOffset);
				}, 1000);
			}
		}

		$scope.$on('$destroy', function (event) {
			cancelTimeout();
		});

		$scope.$on('$locationChangeStart', function () {
			cancelTimeout();
		});

		vm.configMode = function () {
			$state.go('intraday.skill-area-config', {
				isNewSkillArea: false
			});
		};

		vm.toggleModal = function () {
			vm.DeleteSkillAreaModal = !vm.DeleteSkillAreaModal;
		};

		vm.onStateChanged = function (evt, to, params, from) {
			if (to.name !== 'intraday.area') return;
			if (params.isNewSkillArea === true) {
				reloadSkillAreas(true);
			} else reloadSkillAreas(false);
		};

		$scope.$on('$stateChangeSuccess', vm.onStateChanged);

		$scope.$on('$viewContentLoaded', function () {
			pollData(vm.activeTab);
		});

		var notifySkillAreaDeletion = function () {
			var message = $translate.instant('Deleted');
			NoticeService.success(message, 5000, true);
		};

		$scope.$on('$destroy', function () {
			$interval.cancel(polling);
		});

		vm.getLocalDate = function (offset) {
			return moment()
				.add(offset, 'days')
				.format('dddd, LL');
		};

		vm.getUTCDate = function (offset) {
			var ret = moment.utc().toISOString();

			if (vm.toggles.showOtherDay) {
				ret = moment
					.utc()
					.add(offset, 'days')
					.toISOString();
			}

			return ret;
		};

		vm.changeChosenOffset = function (value, dontPoll) {
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

		vm.exportIntradayData = function () {
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

		function saveData(data, status, headers, config) {
			var blob = new Blob([data]);
			vm.exporting = false;
			saveAs(blob, 'IntradayExportedData ' + moment().format('YYYY-MM-DD') + '.xlsx');
		}

		function errorSaveData(data, status, headers, config) {
			vm.exporting = false;
		}
	}
})();
