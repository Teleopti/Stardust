﻿(function () {
	'use strict';

	angular
		.module('wfm.staffing')
		.controller('StaffingController', StaffingController);

	StaffingController.$inject = ['staffingService', '$state', 'Toggle', 'UtilService', 'ChartService', '$filter', 'NoticeService', '$translate', '$scope'];
	function StaffingController(staffingService, $state, toggleService, utilService, chartService, $filter, NoticeService, $translate, $scope) {
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
		vm.isOvertimeSuggestionEnabled = isOvertimeSuggestionEnabled;
		vm.validSettings = validSettings;
		vm.isRunning = isRunning;
		vm.isNoSuggestion = isNoSuggestion;
		vm.goToImportExports = goToImportView;

		vm.showOverstaffSettings = false;
		vm.openImportData = false;
		vm.useShrinkage = false;
		vm.staffingDataAvailable = false;
		vm.hasSuggestionData = false;
		vm.showBpoInterface = false;
		vm.hasPermissionForBpoExchange = false;
		vm.overtimeForm = {};

		vm.compensations = [];
		vm.selectedDate = new Date();

		vm.unifiedSkillGroupManagement = false;
		vm.HasPermissionToModifySkillArea = false;

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
		getStaffingSettings();
		isUnifiedSkillGroupManagementEnabled();

		vm.onStateChanged = function(evt, to, params, from) {
			if(to.name !== 'staffing'){
				return;
			}
			
			if(params.isNewSkillArea === true){
				getSkillAreas();
			}
		};
	  
		$scope.$on('$stateChangeSuccess', vm.onStateChanged);

		vm.configMode = function() {
			$state.go('staffing.skill-area-config', {
			  isNewSkillArea: false
			});
		};

		function isOvertimeSuggestionEnabled() {
			return toggleService.WfmStaffing_AddOvertime_42524;
		}

		function isUnifiedSkillGroupManagementEnabled(){
			vm.unifiedSkillGroupManagement = toggleService.WFM_Unified_Skill_Group_Management_45417;
		}

		function toggleOverstaffSettings() {
			return vm.showOverstaffSettings = !vm.showOverstaffSettings;
		}

		function goToImportView() {
			$state.transitionTo('bpo-gatekeeper')
		}

		function selectFirstCompensation(compensations) {
			vm.overtimeForm.Compensation = compensations[0].Id;
			setDefaultValueForOvertimeRange();
		}

		function setDefaultValueForOvertimeRange() {
			vm.overtimeForm.MinMinutesToAdd = new Date();
			vm.overtimeForm.MinMinutesToAdd.setHours(0, 15, 0);
			vm.overtimeForm.MaxMinutesToAdd = new Date();
			vm.overtimeForm.MaxMinutesToAdd.setHours(1, 0, 0)
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
			data.$promise.then(function (response) {
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

		function extracted(area) {
			currentSkills = area;
			vm.selectedArea = area;
			vm.selectedSkill = null;
		}

		function selectSkillOrArea(skill, area) {
			clearSuggestions();
			if (!skill) {
				extracted(area);
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
				vm.HasPermissionToModifySkillArea = response.HasPermissionToModifySkillArea;
				allSkillAreas = response.SkillAreas;
			})
		}

		function selectedSkillChange(skill) {
			if(document.getElementById("skill-tooltip")){
				document.getElementById("skill-tooltip").remove();
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
				selectFirstCompensation(data);
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
		};

		function generateChartForView(data, isSuggestedData) {
			c3.generate(chartService.staffingChartConfig(data, isSuggestedData));
		}
	}
})();
