﻿(function () {
	'use strict';

	angular
		.module('wfm.staffing')
		.controller('StaffingController', StaffingController);

	StaffingController.$inject = ['staffingService', 'Toggle', '$filter', 'NoticeService', '$translate'];
	function StaffingController(staffingService, toggleService, $filter, NoticeService, $translate) {
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
		vm.toggleDraggable = toggleDraggable;
		vm.triggerResourceCalc = triggerResourceCalc;
		vm.timeSerie = [];
		vm.overTimeModels = [];

		var allSkills = [];
		var allSkillAreas = [];
		getSkills();
		getSkillAreas();
		var currentSkills;

		function getSkillStaffing(skillId) {
			if (!skillId) return;
			return staffingService.getSkillStaffing.get({ id: skillId });
		}

		var getSkillsForArea = getSkillAreaStaffing();
		var staffingData = {};
		vm.devTogglesEnabled = false;
		checkToggles();
		////////////////
		function checkToggles() {
			toggleService.togglesLoaded.then(function () {
				vm.devTogglesEnabled = toggleService.WfmStaffing_AllowActions_42524;
			});
		}

		function generateChart(skillId, areaId) {
			if (skillId) {
				var query = getSkillStaffing(skillId);
			} else if (areaId) {
				var query = getSkillAreaStaffing(areaId);
			}
			query.$promise.then(function (result) {
				staffingData.time = [];
				staffingData.scheduledStaffing = [];
				staffingData.forcastedStaffing = [];
				staffingData.suggestedStaffing = [];
				if (staffingPrecheck(result.DataSeries)) {
					staffingData.scheduledStaffing = result.DataSeries.ScheduledStaffing;
					staffingData.forcastedStaffing = result.DataSeries.ForecastedStaffing;
					staffingData.forcastedStaffing.unshift($translate.instant('ForecastedStaff'));
					staffingData.scheduledStaffing.unshift($translate.instant('ScheduledStaff'));
					vm.timeSerie = result.DataSeries.Time;
					angular.forEach(result.DataSeries.Time,
						function (value, key) {
							staffingData.time.push($filter('date')(value, 'shortTime'));
						},
						staffingData.time);
					staffingData.time.unshift('x');
					generateChartForView();
				} else {
					vm.staffingDataAvailable = false;
				}
			});
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
				selectSkillOrArea(skills[0])
				generateChart(skills[0].Id, null);
				allSkills = skills;
			})
		}

		function getSkillAreas() {
			var query = staffingService.getSkillAreas.get();
			query.$promise.then(function (response) {
				allSkillAreas = response.SkillAreas;
			})
		}

		function getSkillAreaStaffing(areaId) {
			if (!areaId) return;
			return staffingService.getSkillAreaStaffing.get({ id: areaId });
		}

		function selectedSkillChange(skill) {
			if (skill == null) return;
			generateChart(skill.Id, null);
			selectSkillOrArea(skill, null);
		}
		function selectedAreaChange(area) {
			if (area == null) return;
			generateChart(null, area.Id);
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
			if (vm.overTimeModels.length === 0) return;
			var query = staffingService.addOvertime.save(vm.overTimeModels);
			query.$promise.then(function () {
				if (vm.selectedSkill) {
					generateChart(vm.selectedSkill.Id, null);
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

		function toggleDraggable() {
			vm.draggable = !vm.draggable
			generateChartForView();
		};

		function triggerResourceCalc() {
			staffingService.triggerResourceCalculate.get();
			NoticeService.success('ResourceCalculation Triggered', 5000, true);
		};

		function generateChartForView() {
			c3.generate({
				bindto: '#staffingChart',
				data: {
					selection: {
						enabled: vm.draggable,
						draggable: vm.draggable,
						multiple: vm.draggable,
						grouped: vm.draggable

					},
					x: "x",
					columns: [
						staffingData.time,
						staffingData.forcastedStaffing,
						staffingData.scheduledStaffing,
						staffingData.suggestedStaffing
					],
				},
				axis: {
					x: {
						label: {
							text: $translate.instant('SkillTypeTime'),
							position: 'outer-center'
						},
						type: 'category',
						tick: {
							culling: {
								max: 24
							},
							fit: true,
							centered: true,
							multiline: false
						}
					}
				},
				zoom: {
					enabled: false,
				},
			});

		}
	}
})();