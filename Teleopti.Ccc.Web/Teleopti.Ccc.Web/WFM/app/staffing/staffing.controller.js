(function () {
	'use strict';

	angular
		.module('wfm.staffing')
		.controller('StaffingController', StaffingController);

	StaffingController.$inject = ['staffingService', 'Toggle', 'UtilService', 'ChartService', '$filter', 'NoticeService', '$translate'];
	function StaffingController(staffingService, toggleService, utilService, chartService, $filter, NoticeService, $translate) {
		var vm = this;
		vm.staffingDataAvailable = false;
		vm.selectedSkill;
		vm.selectedSkillArea;
		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedAreaChange = selectedAreaChange;
		vm.querySearchSkills = querySearchSkills;
		vm.querySearchAreas = querySearchAreas;
		vm.suggestOvertime = suggestOvertime;
		vm.addOvertime = addOvertime;
		vm.navigateToNewDay = navigateToNewDay;
		vm.hasSuggestionData = false;
		vm.hasRequestedSuggestion = false;
		vm.hasRequestedOvertime = false;
		vm.timeSerie = [];
		vm.overTimeModels = [];
		vm.selectedDate = new Date();
		vm.options = { customClass: getDayClass };
		vm.useShrinkage = false;
		vm.useShrinkageForStaffing = useShrinkageForStaffing;
		vm.generateChart = generateChart;
		vm.dynamicIcon = dynamicIcon;
		vm.compensations = [];
		vm.overtimeSettings = {};
		vm.showOverstaffSettings = false;
		vm.openImportData = false;
		vm.toggleOverstaffSettings = toggleOverstaffSettings;
		vm.validSettings = validSettings;

		var events = [];
		var allSkills = [];
		var allSkillAreas = [];
		var currentSkills;
		var staffingData = {};
		var sample = {
			date: null,
			status: 'full'
		}
		getSkills();
		getSkillAreas();
		prepareDays();
		getCompensations();

		function prepareDays() {
			for (var i = 0; i < 14; i++) {
				var newDate = new Date();
				newDate.setDate(newDate.getDate() + i);
				var insertData = angular.copy(sample);
				insertData.date = newDate;
				events.push(insertData);
			}
		}

		function toggleOverstaffSettings() {
			vm.showOverstaffSettings = !vm.showOverstaffSettings;
		}

		function getDayClass(data) {
			var date = data.date,
				mode = data.mode;
			if (mode === 'day') {
				var dayToCheck = new Date(date).setHours(0, 0, 0, 0);
				for (var i = 0; i < events.length; i++) {
					var currentDay = new Date(events[i].date).setHours(0, 0, 0, 0);
					if (dayToCheck === currentDay) {
						return events[i].status;
					}
				}
			}
			return '';
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
			generateChart(vm.selectedSkill, vm.selectedArea);
		}

		function generateChart(skill, area) {
			if (skill) {
				var query = getSkillStaffingByDate(skill.Id, vm.selectedDate, vm.useShrinkage);
			} else if (area) {
				var query = getSkillAreaStaffingByDate(area.Id, vm.selectedDate, vm.useShrinkage);
			}
			query.$promise.then(function (result) {

				if (staffingPrecheck(result.DataSeries)) {

					vm.timeSerie = result.DataSeries.Time;
					var staffingData = utilService.prepareStaffingData(result);
					staffingData.then(function (processedData) {
						generateChartForView(processedData);
					})
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

		function clearSuggestions() {
			vm.hasSuggestionData = false;
			vm.hasRequestedSuggestion = false
		}

		function navigateToNewDay() {
			if (vm.hasSuggestionData) {
				if (confirm($translate.instant('DiscardSuggestionData'))) {
					clearSuggestions();
					vm.generateChart(vm.selectedSkill, vm.selectedArea);
				}
			} else {
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
			vm.hasSuggestionData = false;
			if (vm.overTimeModels.length === 0) {
				vm.hasRequestedSuggestion = false;
				return;
			}
			vm.hasRequestedOvertime = true;
			var query = staffingService.addOvertime.save(vm.overTimeModels);
			query.$promise.then(function () {
				if (vm.selectedSkill) {
					generateChart(vm.selectedSkill, null);
				} else if (vm.selectedSkillArea) {
					generateChart(null, vm.selectedSkillArea);
				}
				vm.hasRequestedOvertime = false;
				vm.hasRequestedSuggestion = false;
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
			else{
				return true;
			}
		}

		function suggestOvertime() {
			var skillIds;
			if (currentSkills.Skills) {
				skillIds = currentSkills.Skills.map(function (skill) {
					return skill.Id;
				});
			} else {
				skillIds = [currentSkills.Id];
			}
			vm.hasRequestedSuggestion = true;
			var settings = utilService.prepareSettings(vm.overtimeSettings)

			var query = staffingService.getSuggestion.save({ SkillIds: skillIds, TimeSerie: vm.timeSerie, OvertimePreferences: settings });
			query.$promise.then(function (response) {
				vm.hasRequestedSuggestion = false;

				staffingData.time = [];
				staffingData.scheduledStaffing = [];
				staffingData.forcastedStaffing = [];
				staffingData.suggestedStaffing = [];
				staffingData.absoluteDifference = [];
				if (staffingPrecheck(response.DataSeries)) {
					if (response.StaffingHasData) {
						vm.hasSuggestionData = true;
					} else {
						vm.hasSuggestionData = false;
						vm.hasRequestedSuggestion = false;
					}
					vm.staffingDataAvailable = true;
					vm.overTimeModels = response.OverTimeModels;
					staffingData.scheduledStaffing = utilService.roundDataToOneDecimal(response.DataSeries.ScheduledStaffing);
					staffingData.forcastedStaffing = utilService.roundDataToOneDecimal(response.DataSeries.ForecastedStaffing);
					staffingData.absoluteDifference = response.DataSeries.AbsoluteDifference;
					staffingData.forcastedStaffing.unshift($translate.instant('ForecastedStaff'));
					staffingData.scheduledStaffing.unshift($translate.instant('ScheduledStaff'));
					vm.timeSerie = response.DataSeries.Time;
					angular.forEach(response.DataSeries.Time,
						function (value, key) {
							staffingData.time.push($filter('date')(value, 'shortTime'));
						},
						staffingData.time);
					staffingData.time.unshift('x');
					generateChartForView(staffingData);
				} else {
					vm.staffingDataAvailable = false;
				}
			});
		};
		function generateChartForView(data) {
			c3.generate(chartService.config(data));
		}
	}
})();