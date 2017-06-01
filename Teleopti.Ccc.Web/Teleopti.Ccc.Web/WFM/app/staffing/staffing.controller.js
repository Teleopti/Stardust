(function () {
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
		vm.triggerResourceCalc = triggerResourceCalc;
		vm.timeSerie = [];
		vm.overTimeModels = [];
		vm.selectedDate = new Date();
		vm.options = { customClass: getDayClass };
		vm.events = [];
		vm.devTogglesEnabled = false;
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
		checkToggles();
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

		function checkToggles() {
			toggleService.togglesLoaded.then(function () {
				vm.devTogglesEnabled = toggleService.WfmStaffing_AllowActions_42524;
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

		function generateColorObject(absoluteObj) {
			var colors = {};
			var scafoldColorKey = staffingData.scheduledStaffing[0];
			var staffingColorKey = staffingData.scheduledStaffingActual[0];
			var absoluteColorKey = absoluteObj.data[0]
			colors[scafoldColorKey] = '#fff';
			colors[absoluteColorKey] = '#66C2FF';
			colors[staffingColorKey] = '#8c8282';
			return colors;
		}

		function generateOverUnderStaffing() {
			var staffing = {};
			staffing.over = [];
			staffing.under = [];
			staffing.over.unshift('Overstaffing');
			staffing.under.unshift('Understaffing');
			for (var index = 1; index <= staffingData.scheduledStaffing.length; index++) {
				var value = staffingData.scheduledStaffing[index] - staffingData.forcastedStaffing[index];

				if (value < 0) {
					staffing.under.push(Math.abs(value));
					staffing.over.push(0);
				} else {
					staffing.over.push(value);
					staffing.under.push(0);
				}

			}
			return staffing
		}

		function generateTypeObject() {
			var types = {};
			var forcastingTypeKey = staffingData.forcastedStaffing[0];
			var staffingTypeKey = staffingData.scheduledStaffing[0];
			types[forcastingTypeKey] = 'line';
			types[staffingTypeKey] = 'line';
			return types;
		}

		function generateScaffold() {
			var scaffold = {};
			scaffold.under = staffingData.scheduledStaffing.concat();
			scaffold.under.shift();
			scaffold.under.unshift('UnderStaffScaffold');

			scaffold.over = staffingData.forcastedStaffing.concat();
			scaffold.over.shift();
			scaffold.over.unshift('OverStaffScaffold');
			return scaffold;
		}
		function generateColorObjectV2(staffingObj, scaffoldObj) {
			var colors = {};
			var overstaffColorKey = staffingObj.over[0];
			var understaffColorKey = staffingObj.under[0];
			var overScaffoldKey = scaffoldObj.over[0];
			var underScaffoldKey = scaffoldObj.under[0];

			colors[overstaffColorKey] = '#4286f4';
			colors[understaffColorKey] = '#f44141';
			colors[underScaffoldKey] = '#fff';
			colors[overScaffoldKey] = '#fff';
			return colors;

        }

        function tooltip_contents(d, defaultTitleFormat, defaultValueFormat, color) {
            var $$ = this, config = $$.config, CLASS = $$.CLASS,
                titleFormat = config.tooltip_format_title || defaultTitleFormat,
                nameFormat = config.tooltip_format_name || function (name) { return name; },
                valueFormat = config.tooltip_format_value || defaultValueFormat,
                text, i, title, value, name, bgcolor;

            // You can access all of data like this:
            //console.log($$.data.targets);

            for (i = 0; i < d.length; i++) {
                if (!(d[i] && (d[i].value || d[i].value === 0))) { continue; }

                if (d[i].name === 'OverStaffScaffold' || d[i].name === 'UnderStaffScaffold') { continue; }

                if (!text) {
                    title = 'Staffing'
                    text = "<table class='" + CLASS.tooltip + "'>" + (title || title === 0 ? "<tr><th colspan='2'>" + title + "</th></tr>" : "");
                }

                name = nameFormat(d[i].name);
                value = valueFormat(d[i].value, d[i].ratio, d[i].id, d[i].index);
                bgcolor = $$.levelColor ? $$.levelColor(d[i].value) : color(d[i].id);

                text += "<tr class='" + CLASS.tooltipName + "-" + d[i].id + "'>";
                text += "<td class='name'><span style='background-color:" + bgcolor + "'></span>" + name + "</td>";
                text += "<td class='value'>" + value + "</td>";
                text += "</tr>";
            }
            return text + "</table>";
        }

		function generateChartForView() {
			var staffing = generateOverUnderStaffing();
			var scaffold = generateScaffold();
			var types = generateTypeObject();
			var chartColors = generateColorObjectV2(staffing, scaffold);

			c3.generate({
				bindto: '#staffingChart',
				point: {
					show: false
				},
				legend:{
				hide:[scaffold.under[0], scaffold.over[0]],
				},
				data: {
					colors: chartColors,
					order: 'null',
					type: 'bar',
					x: "x",
					types: types,
					columns: [
						staffingData.time,
						staffingData.forcastedStaffing,
						staffingData.scheduledStaffing,
						scaffold.over,
						scaffold.under,
						staffing.over,
						staffing.under

					],
					groups: [
						[scaffold.over[0], staffing.over[0]],
						[scaffold.under[0], staffing.under[0]]
					]
                },
                tooltip: {
                    contents: tooltip_contents
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