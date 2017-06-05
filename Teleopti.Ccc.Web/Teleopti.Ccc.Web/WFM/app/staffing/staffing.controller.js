(function () {
	'use strict';

	angular
		.module('wfm.staffing')
		.controller('StaffingController', StaffingController);

	StaffingController.$inject = ['staffingService', 'Toggle', 'UtilService', 'ChartService', '$filter', 'NoticeService', '$translate'];
	function StaffingController(staffingService, toggleService, utilService, chartService, $filter, NoticeService, $translate) {
		var vm = this;
		vm.staffingDataAvailable = true;
		vm.selectedSkill;
		vm.selectedSkillArea;
		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedAreaChange = selectedAreaChange;
		vm.querySearchSkills = querySearchSkills;
		vm.querySearchAreas = querySearchAreas;
		vm.suggestOvertime = suggestOvertime;
		vm.addOvertime = addOvertime;
		vm.hasSuggestionData = false;
		vm.hasRequestedSuggestion = false;
		vm.draggable = false;
		vm.triggerResourceCalc = triggerResourceCalc;
		vm.timeSerie = [];
		vm.overTimeModels = [];
		vm.selectedDate = new Date();
		vm.options = { customClass: getDayClass };
		vm.events = [];
		vm.useShrinkage = false;
		vm.useShrinkageForStaffing = useShrinkageForStaffing;
		vm.generateChart = generateChart;

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
		setPrepareDays();

		function setPrepareDays() {
			for (var i = 0; i < 14; i++) {
				var newDate = new Date();
				newDate.setDate(newDate.getDate() + i);
				var insertData = angular.copy(sample);
				insertData.date = newDate;
				vm.events.push(insertData);
			}
		}

		function getDayClass(data) {
			var date = data.date,
				mode = data.mode;
			if (mode === 'day') {
				var dayToCheck = new Date(date).setHours(0, 0, 0, 0);
				for (var i = 0; i < vm.events.length; i++) {
					var currentDay = new Date(vm.events[i].date).setHours(0, 0, 0, 0);
					if (dayToCheck === currentDay) {
						return vm.events[i].status;
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
				staffingData.time = [];
				staffingData.scheduledStaffing = [];
				staffingData.forcastedStaffing = [];
				staffingData.suggestedStaffing = [];
				staffingData.relativeDifference = [];
				if (staffingPrecheck(result.DataSeries)) {
					staffingData.scheduledStaffing = roundDataToOneDecimal(result.DataSeries.ScheduledStaffing);
					staffingData.forcastedStaffing = roundDataToOneDecimal(result.DataSeries.ForecastedStaffing);
					staffingData.relativeDifference = result.DataSeries.RelativeDifference;
					staffingData.forcastedStaffing.unshift($translate.instant('ForecastedStaff'));
					staffingData.scheduledStaffing.unshift($translate.instant('ScheduledStaff'));
					vm.timeSerie = result.DataSeries.Time;
					angular.forEach(result.DataSeries.Time,
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
		}
		function roundDataToOneDecimal(input) {
			input = utilService.roundArrayContents(input, 1)

			return input;
		}

		function staffingPrecheck(data) {
			if (!angular.equals(data, {}) && data != null) {
				if (data.Time && data.ScheduledStaffing && data.ForecastedStaffing) {
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

		function selectSkillOrArea(skill, area) {
			clearSuggestions()
			if (!skill) {
				currentSkills = area;
				vm.selectedSkillArea = area;
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
			var query = staffingService.addOvertime.save(vm.overTimeModels);
			query.$promise.then(function () {
				if (vm.selectedSkill) {
					generateChart(vm.selectedSkill, null);
				} else if (vm.selectedSkillArea) {
					generateChart(null, vm.selectedSkillArea);
				}
				vm.hasRequestedSuggestion = false;
			});

		};

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
			var query = staffingService.getSuggestion.save({ SkillIds: skillIds, TimeSerie: vm.timeSerie });
			query.$promise.then(function (response) {
				staffingData.suggestedStaffing = response.SuggestedStaffingWithOverTime;
				vm.overTimeModels = response.OverTimeModels;
				staffingData.suggestedStaffing.unshift("Suggested Staffing");
				generateChartForView();
				vm.hasSuggestionData = true;
			});

		};

		function triggerResourceCalc() {
			staffingService.triggerResourceCalculate.get();
			NoticeService.success('ResourceCalculation Triggered', 5000, true);
		};

		function generateChartForView(data) {
			c3.generate(chartService.config(data));
		}
	}
})();