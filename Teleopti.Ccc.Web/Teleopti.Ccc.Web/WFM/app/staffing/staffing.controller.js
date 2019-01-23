(function() {
	'use strict';

	angular.module('wfm.staffing').controller('StaffingController', StaffingController);

	StaffingController.$inject = [
		'staffingService',
		'$state',
		'Toggle',
		'UtilService',
		'ChartService',
		'$filter',
		'NoticeService',
		'$translate',
		'$scope',
		'$window'
	];
	function StaffingController(
		staffingService,
		$state,
		toggleService,
		utilService,
		chartService,
		$filter,
		NoticeService,
		$translate,
		$scope,
		$window
	) {
		var vm = this;

		vm.skills;
		vm.selectedSkill;
		vm.selectedSkillArea;
		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedAreaChange = selectedAreaChange;
		vm.querySearchSkills = querySearchSkills;
		vm.querySearchAreas = querySearchAreas;
		vm.suggestOvertime = suggestOvertime;
		vm.addOvertime = addOvertime;
		vm.navigateToNewDay = navigateToNewDay;
		vm.useShrinkageForStaffing = useShrinkageForStaffing;
		vm.generateChart = generateChart;
		vm.dynamicIcon = dynamicIcon;
		vm.toggleOverstaffSettings = toggleOverstaffSettings;
		vm.isOvertimeSuggestionEnabled = isOvertimeSuggestionEnabled;
		vm.validSettings = validSettings;
		vm.isRunning = isRunning;
		vm.isNoSuggestion = isNoSuggestion;
		vm.goToImportExports = goToImportView;
		vm.testSessionStorage = testSessionStorage;
		vm.exportStaffingData = exportStaffingData;
		vm.getDisplayDate = getDisplayDate;

		vm.showOverstaffSettings = false;
		vm.openImportData = false;
		vm.useShrinkage = false;
		initializeShrinkageCheckBoxFromSession();
		vm.staffingDataAvailable = false;
		vm.hasSuggestionData = false;
		vm.showBpoInterface = false;
		vm.hasPermissionForBpoExchange = false;
		vm.overtimeForm = {};

		vm.compensations = [];
		vm.selectedDate = new Date();
		vm.exportStaffingDataDate = {
			startDate: moment()
				.utc()
				.toDate(),
			endDate: moment()
				.utc()
				.add(6, 'days')
				.toDate()
		};
		vm.HasPermissionToModifySkillArea = false;
		vm.importedBboInfos = [];

		vm.isSearchRequested = false;

		var events = [];
		var allSkills = [];
		var allSkillAreas = [];
		var overTimeModels = [];
		var timeSerie = [];
		var currentSkills;
		var staffingData = {};
		var suggestedStaffingData = {};
		var isRequestingSuggestions = false;
		var isRequestingOvertime = false;
		var hasRequestedSuggestions = false;
		var sample = {
			date: null,
			status: 'full'
		};

		getSkills();
		getSkillAreas();
		getCompensations();
		getStaffingSettings();

		vm.onStateChanged = function(evt, to, params, from) {
			getSkillAreas();
		};

		$scope.$on('$stateChangeSuccess', vm.onStateChanged);

		vm.configMode = function() {
			$state.go('staffingModule.skill-area-config', {
				isNewSkillArea: false
			});
		};

		function isOvertimeSuggestionEnabled() {
			return toggleService.WfmStaffing_AddOvertime_42524;
		}

		function toggleOverstaffSettings() {
			return (vm.showOverstaffSettings = !vm.showOverstaffSettings);
		}

		function goToImportView() {
			$state.transitionTo('bpo');
		}

		function selectFirstCompensation(compensations) {
			vm.overtimeForm = {};
			vm.overtimeForm.Compensation = compensations[0].Id;
			setDefaultValueForOvertimeRange();
		}

		function setDefaultValueForOvertimeRange() {
			vm.overtimeForm.MinMinutesToAdd = new Date();
			vm.overtimeForm.MinMinutesToAdd.setHours(0, 15, 0);
			vm.overtimeForm.MaxMinutesToAdd = new Date();
			vm.overtimeForm.MaxMinutesToAdd.setHours(1, 0, 0);
		}

		function isRunning() {
			return isRequestingSuggestions || isRequestingOvertime;
		}

		function isNoSuggestion() {
			return !vm.hasSuggestionData && hasRequestedSuggestions;
		}

		function clearSuggestions() {
			vm.hasSuggestionData = false;
			suggestedStaffingData = {};
			hasRequestedSuggestions = false;
		}
		function getStaffingSettings() {
			var data = staffingService.staffingSettings.get();
			data.$promise.then(function(response) {
				vm.showBpoInterface = response.isLicenseAvailable;
				vm.hasPermissionForBpoExchange = response.HasPermissionForBpoExchange;
			});
		}

		function getSkillStaffingByDate(skillId, date, shrinkage) {
			var data = { SkillId: skillId, DateTime: date, UseShrinkage: shrinkage };
			return staffingService.getSkillStaffingByDate.get(data);
		}

		function getSkillAreaStaffingByDate(skillAreaId, date, shrinkage) {
			var data = { SkillAreaId: skillAreaId, DateTime: date, UseShrinkage: shrinkage };
			return staffingService.getSkillAreaStaffingByDate.get(data);
		}

		function useShrinkageForStaffing() {
			sessionStorage.staffingUseShrinkage = angular.toJson(vm.useShrinkage);
			return generateChart(vm.selectedSkill, vm.selectedArea);
		}

		function generateChart(skill, area) {
			vm.chartQuery = null;
			if (skill) {
				vm.isSearchRequested = true;
				clearSuggestions();
				vm.chartQuery = getSkillStaffingByDate(
					skill.Id,
					moment(vm.selectedDate).format('YYYY-MM-DD'),
					vm.useShrinkage
				);
			} else if (area) {
				vm.isSearchRequested = true;
				clearSuggestions();
				vm.chartQuery = getSkillAreaStaffingByDate(
					area.Id,
					moment(vm.selectedDate).format('YYYY-MM-DD'),
					vm.useShrinkage
				);
			}
			if (vm.chartQuery) {
				vm.chartQuery.$promise
					.then(function(result) {
						if (staffingPrecheck(result.DataSeries)) {
							timeSerie = result.DataSeries.Time;
							staffingData = utilService.prepareStaffingData(result);
							generateChartForView(staffingData);
							vm.importedBboInfos = result.ImportBpoInfoList;
						} else {
							vm.staffingDataAvailable = false;
						}
						//just in case
						vm.isSearchRequested = false;
					})
					.finally(function() {
						vm.isSearchRequested = false;
					});
			}
		}

		function staffingPrecheck(data) {
			if (!angular.equals(data, {}) && data != null) {
				if (data.Time.length > 0 && data.ScheduledStaffing && data.ForecastedStaffing) {
					vm.staffingDataAvailable = true;
					return true;
				}
			}
			vm.staffingDataAvailable = false;
			return false;
		}

		function updateExportDate() {
			vm.exportStaffingDataDate = {
				startDate: moment(vm.selectedDate)
					.utc()
					.toDate(),
				endDate: moment(vm.selectedDate)
					.utc()
					.add(6, 'days')
					.toDate()
			};
		}

		function navigateToNewDay(date) {
			if (!date) return;

			$window.sessionStorage.staffingSelectedDate = moment(date).format('YYYY-MM-DD');
			if (vm.hasSuggestionData) {
				if (confirm($translate.instant('DiscardSuggestionData'))) {
					clearSuggestions();
					vm.showOverstaffSettings = false;
					vm.generateChart(vm.selectedSkill, vm.selectedSkillArea);
				}
			} else {
				vm.showOverstaffSettings = false;
				vm.generateChart(vm.selectedSkill, vm.selectedSkillArea);
			}
			updateExportDate();
		}

		function extracted(area) {
			currentSkills = area;
			vm.selectedArea = area;
			$window.sessionStorage.staffingSelectedArea = angular.toJson(area);
			delete $window.sessionStorage.staffingSelectedSkill;
			vm.selectedSkill = null;
		}

		function selectSkillOrArea(skill, area) {
			clearSuggestions();
			if (!skill) {
				extracted(area);
				vm.selectedSkillArea = area;
			} else {
				currentSkills = skill;
				vm.selectedSkill = currentSkills;
				$window.sessionStorage.staffingSelectedSkill = angular.toJson(skill);
				delete $window.sessionStorage.staffingSelectedArea;
				vm.selectedArea = null;
			}
		}

		function getSkills() {
			if ($window.sessionStorage.staffingSelectedDate)
				vm.selectedDate = new Date($window.sessionStorage.staffingSelectedDate);
			var query = staffingService.getSkills.query();
			query.$promise.then(function(skills) {
				if ($window.sessionStorage.staffingSelectedSkill) {
					manageSkillSessionStorage();
					manageDateSessionStorage();
					manageShrinkageSessionStorage();
				} else if (!$window.sessionStorage.staffingSelectedArea) {
					selectSkillOrArea(skills[0]);
				}
				allSkills = skills;
				vm.allSkills = skills;
			});
		}

		function checkArea(area) {
			return area.Id === angular.fromJson($window.sessionStorage.staffingSelectedArea).Id;
		}

		function getSkillAreas() {
			var query = staffingService.getSkillAreas.get();
			query.$promise.then(function(response) {
				vm.HasPermissionToModifySkillArea = response.HasPermissionToModifySkillArea;

				if ($window.sessionStorage.staffingSelectedArea) {
					if (response.SkillAreas.find(checkArea)) {
						vm.selectedSkillArea = response.SkillAreas.find(checkArea);
					} else {
						manageAreaSessionStorage();
					}

					manageDateSessionStorage();
					manageShrinkageSessionStorage();
				}
				allSkillAreas = response.SkillAreas;
				vm.allSkillAreas = response.SkillAreas;
			});
		}

		function selectedSkillChange(skill) {
			if (document.getElementById('skill-tooltip')) {
				document.getElementById('skill-tooltip').remove();
			}

			if (skill == null) return;
			generateChart(skill, null);
			selectSkillOrArea(skill, null);
		}

		function selectedAreaChange(area) {
			if (area == null) return;
			generateChart(null, area);
			selectSkillOrArea(null, area);
		}

		function querySearchSkills(query) {
			var results = query ? allSkills.filter(createFilterFor(query)) : allSkills,
				deferred;
			return results;
		}

		function querySearchAreas(query) {
			var results = query ? allSkillAreas.filter(createFilterFor(query)) : allSkillAreas,
				deferred;
			return results;
		}

		function dynamicIcon(skill) {
			if (!skill.DoDisplayData) {
				return 'mdi mdi-alert';
			}
			if (skill.IsMultisiteSkill) {
				return 'mdi mdi-hexagon-multiple';
			}
			switch (skill.SkillType) {
				case 'SkillTypeChat':
					return 'mdi mdi-message-text-outline';
				case 'SkillTypeEmail':
					return 'mdi mdi-email-outline';
				case 'SkillTypeInboundTelephony':
					return 'mdi mdi-phone';
				case 'SkillTypeRetail':
					return 'mdi mdi-credit-card';
				case 'SkillTypeBackoffice':
					return 'mdi mdi-archive';
				default:
					return 'mdi mdi-creation';
			}
		}

		function createFilterFor(query) {
			var lowercaseQuery = angular.lowercase(query);
			return function filterFn(item) {
				var lowercaseName = angular.lowercase(item.Name);
				return lowercaseName.indexOf(lowercaseQuery) === 0;
			};
		}

		function addOvertime() {
			if (overTimeModels.length === 0) {
				return;
			}
			isRequestingOvertime = true;
			var query = staffingService.addOvertime.save(overTimeModels);
			query.$promise.then(function() {
				if (vm.selectedSkill) {
					generateChart(vm.selectedSkill, null);
				} else if (vm.selectedSkillArea) {
					generateChart(null, vm.selectedSkillArea);
				}
				clearSuggestions();
				isRequestingOvertime = false;
			});
		}

		function getCompensations() {
			var query = staffingService.getCompensations.query();
			query.$promise.then(function(data) {
				vm.compensations = data;
				selectFirstCompensation(data);
			});
		}
		function validSettings(input) {
			if (input.MinMinutesToAdd > input.MaxMinutesToAdd) {
				return false;
			} else {
				return true;
			}
		}

		function suggestOvertime(form) {
			hasRequestedSuggestions = false;
			isRequestingSuggestions = true;
			var skillIds;
			var settings = utilService.prepareSettings(form);
			if (currentSkills.Skills) {
				skillIds = currentSkills.Skills.map(function(skill) {
					return skill.Id;
				});
			} else {
				skillIds = [currentSkills.Id];
			}

			var query = staffingService.getSuggestion.save({
				SkillIds: skillIds,
				TimeSerie: timeSerie,
				OvertimePreferences: settings
			});
			query.$promise.then(function(response) {
				isRequestingSuggestions = false;
				hasRequestedSuggestions = true;
				if (staffingPrecheck(response.DataSeries)) {
					if (response.StaffingHasData) {
						vm.hasSuggestionData = true;
						//compare data and set to 0 if equal
						suggestedStaffingData = utilService.prepareSuggestedStaffingData(staffingData, response);
					} else {
						vm.hasSuggestionData = false;
						return;
					}
					vm.staffingDataAvailable = true;
					overTimeModels = response.OverTimeModels;
					generateChartForView(suggestedStaffingData, true);
				} else {
					vm.staffingDataAvailable = false;
				}
			});
		}

		function generateChartForView(data, isSuggestedData) {
			c3.generate(chartService.staffingChartConfig(data, isSuggestedData));
		}

		function exportStaffingData() {
			if (vm.exportStaffingDataDate.startDate === null || vm.exportStaffingDataDate.endDate === null) {
				vm.ErrorMessage = $translate.instant('SelectStartDateAndEndDate');
				return;
			}
			if (vm.selectedSkill === null) {
				vm.ErrorMessage = $translate.instant('BpoExportYouMustSelectASkill');
				return;
			}
			vm.exporting = true;
			var request = staffingService.exportStaffingData.get({
				skillId: vm.selectedSkill.Id,
				stringExportStartDate: moment(vm.exportStaffingDataDate.startDate).format('YYYY-MM-DD'),
				stringExportEndDate: moment(vm.exportStaffingDataDate.endDate).format('YYYY-MM-DD'),
				useShrinkage: vm.useShrinkage
			});
			request.$promise.then(function(response) {
				vm.ErrorMessage = response.ErrorMessage;
				vm.exporting = false;
				if (vm.ErrorMessage !== '') {
					return;
				} else {
					vm.exportModal = false;
					utilService.saveToFs(response.Content, vm.selectedSkill.Name + '.csv', 'text/csv');
				}
			});
		}

		function getDisplayDate() {
			return moment(vm.selectedDate).format('YYYY-MM-DD');
		}

		function manageAreaSessionStorage() {
			if ($window.sessionStorage.staffingSelectedArea) {
				vm.selectedArea = null;
				selectedAreaChange(angular.fromJson($window.sessionStorage.staffingSelectedArea));
			}
		}

		function manageSkillSessionStorage() {
			if ($window.sessionStorage.staffingSelectedSkill) {
				vm.selectedSkill = null;
				selectedSkillChange(angular.fromJson($window.sessionStorage.staffingSelectedSkill));
			}
		}

		function manageShrinkageSessionStorage() {
			if ($window.sessionStorage.staffingUseShrinkage) {
				vm.useShrinkage = angular.fromJson($window.sessionStorage.staffingUseShrinkage);
				vm.useShrinkageForStaffing();
			}
		}

		function initializeShrinkageCheckBoxFromSession() {
			if ($window.sessionStorage.staffingUseShrinkage) {
				vm.useShrinkage = angular.fromJson($window.sessionStorage.staffingUseShrinkage);
			}
		}

		function manageDateSessionStorage() {
			if ($window.sessionStorage.staffingSelectedDate) {
				vm.selectedDate = new Date($window.sessionStorage.staffingSelectedDate);
				vm.navigateToNewDay(vm.selectedDate);
			}
		}

		function testSessionStorage(test) {
			if ((test = 1)) manageAreaSessionStorage();
			if ((test = 2)) manageSkillSessionStorage();
			if ((test = 3)) manageShrinkageSessionStorage();
			if ((test = 4)) manageDateSessionStorage();
		}
	}
})();
