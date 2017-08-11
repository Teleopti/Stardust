(function () {
	'use strict';

	angular
		.module('wfm.staffing')
		.controller('StaffingController', StaffingController);

	StaffingController.$inject = ['staffingService', 'Toggle', 'UtilService', 'ChartService', '$filter', 'NoticeService', '$translate'];
	function StaffingController(staffingService, toggleService, utilService, chartService, $filter, NoticeService, $translate) {
		var vm = this;

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
		vm.validSettings = validSettings;
		vm.isRunning = isRunning;
		vm.isNoSuggestion = isNoSuggestion;

		vm.showOverstaffSettings = false;
		vm.openImportData = false;
		vm.useShrinkage = false;
		vm.staffingDataAvailable = false;
		vm.hasSuggestionData = false;
		vm.showBpoInterface = false;

		vm.compensations = [];
		vm.selectedDate = new Date();

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
		}

		getSkills();
		getSkillAreas();
		getCompensations();
		getLicenseForBpo();

		function toggleOverstaffSettings() {
			return vm.showOverstaffSettings = !vm.showOverstaffSettings;
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
		//Staffing_BPOExchangeImport_45202 
		function getLicenseForBpo() {
			if (!toggleService.Staffing_BPOExchangeImport_45202) return;

			var data = staffingService.fileImportLicense.get()
			data.$promise.then(function (response) {
				vm.showBpoInterface = response.isLicenseAvailable;
			})
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
			vm.useShrinkage = !vm.useShrinkage;
			return generateChart(vm.selectedSkill, vm.selectedArea);
		}

		function generateChart(skill, area) {
			if (skill) {
				clearSuggestions();
				var query = getSkillStaffingByDate(skill.Id, vm.selectedDate, vm.useShrinkage);
			} else if (area) {
				clearSuggestions();
				var query = getSkillAreaStaffingByDate(area.Id, vm.selectedDate, vm.useShrinkage);
			}
			query.$promise.then(function (result) {
				if (staffingPrecheck(result.DataSeries)) {
					timeSerie = result.DataSeries.Time;
					staffingData = utilService.prepareStaffingData(result);
					generateChartForView(staffingData);
				} else {
					vm.staffingDataAvailable = false;
				}
			});
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

		function navigateToNewDay() {
			if (vm.hasSuggestionData) {
				if (confirm($translate.instant('DiscardSuggestionData'))) {
					clearSuggestions();
					vm.showOverstaffSettings = false;
					vm.generateChart(vm.selectedSkill, vm.selectedArea);
				}
			} else {
				vm.showOverstaffSettings = false;
				vm.generateChart(vm.selectedSkill, vm.selectedArea);
			}
		}

		function selectSkillOrArea(skill, area) {
			clearSuggestions();
			if (!skill) {
				currentSkills = area;
				vm.selectedArea = area;
				vm.selectedSkill = null;
			} else {
				currentSkills = skill;
				vm.selectedSkill = currentSkills;
				vm.selectedArea = null;
			}
		}

		function getSkills() {
			var query = staffingService.getSkills.query();
			query.$promise.then(function (skills) {
				selectSkillOrArea(skills[0]);
				allSkills = skills;
			})
		}

		function getSkillAreas() {
			var query = staffingService.getSkillAreas.get();
			query.$promise.then(function (response) {
				allSkillAreas = response.SkillAreas;
			})
		}

		function selectedSkillChange(skill) {
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
		};

		function querySearchAreas(query) {
			var results = query ? allSkillAreas.filter(createFilterFor(query)) : allSkillAreas,
				deferred;
			return results;
		};

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
				default:
					return 'mdi mdi-creation'
			}
		}

		function createFilterFor(query) {
			var lowercaseQuery = angular.lowercase(query);
			return function filterFn(item) {
				var lowercaseName = angular.lowercase(item.Name);
				return (lowercaseName.indexOf(lowercaseQuery) === 0);
			};
		};

		function addOvertime() {
			if (overTimeModels.length === 0) {
				return;
			}
			isRequestingOvertime = true;
			var query = staffingService.addOvertime.save(overTimeModels);
			query.$promise.then(function () {
				if (vm.selectedSkill) {
					generateChart(vm.selectedSkill, null);
				} else if (vm.selectedSkillArea) {
					generateChart(null, vm.selectedSkillArea);
				}
				clearSuggestions();
				isRequestingOvertime = false;
			});
		};

		function getCompensations() {
			var query = staffingService.getCompensations.query();
			query.$promise.then(function (data) {
				vm.compensations = data;
			});
		}
		function validSettings(input) {
			if (input.MinMinutesToAdd > input.MaxMinutesToAdd) {
				return false;
			}
			else {
				return true;
			}
		}

		function suggestOvertime(form) {
			hasRequestedSuggestions = false;
			isRequestingSuggestions = true;
			var skillIds;
			var settings = utilService.prepareSettings(form);
			if (currentSkills.Skills) {
				skillIds = currentSkills.Skills.map(function (skill) {
					return skill.Id;
				});
			} else {
				skillIds = [currentSkills.Id];
			}

			var query = staffingService.getSuggestion.save({ SkillIds: skillIds, TimeSerie: timeSerie, OvertimePreferences: settings });
			query.$promise.then(function (response) {
				isRequestingSuggestions = false;
				hasRequestedSuggestions = true;
				if (staffingPrecheck(response.DataSeries)) {
					if (response.StaffingHasData) {
						vm.hasSuggestionData = true;
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
		};

		function generateChartForView(data, isSuggestedData) {
			c3.generate(chartService.staffingChartConfig(data, isSuggestedData));
		}
	}
})();
