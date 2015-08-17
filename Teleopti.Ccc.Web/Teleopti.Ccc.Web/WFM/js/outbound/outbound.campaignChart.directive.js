(function () {
	'use strict';

	angular.module('wfm.outbound').directive('campaignChart', ['$filter', campaignChart]);

	function campaignChart($filter) {
		return {
			controller: ['$scope', '$element',  campaignChartCtrl],
			template: '<md-switch class="campaign-chart-switch" ng-click="onToggleViewScheduleDiff()" ng-if="hidePlannedAnalyzeButton(campaign.Status)" ng-init="campaign.switchSwitch=false" ng-disabled="campaign.switchSwitch">Analyze Schedule</md-switch><div id="Chart_{{campaign.Id}}"></div>',
			scope: {
				'campaign': '=',
				'graphData': '=',
				'dictionary': '='
			},
			require: ['campaignChart'],
			link: postlink
		};

		function campaignChartCtrl($scope, $element) {

			$scope.$watch(function() {
				return $scope.graphData;
			}, function (newVal, oldVal) {
				if (newVal != oldVal) {
					toggleViewScheduleDiff();
				}
			},true);

			$scope.viewScheduleDiff = false;
			$scope.dates = $scope.graphData['dates'].slice(1);

			this.toggleViewScheduleDiff = toggleViewScheduleDiff;
			this.init = init;
			this.getDataIndex = getDataIndex;
			this.generateChart = generateChart;

			$scope.campaign.manualPlan = {};
			$scope.campaign.backlog = {};

			$scope.campaign.selectedDates = [];

			function graphSelectionChanged() {
				$scope.campaign.selectedDates = $scope.graph.selected().filter(function(p) {
					return p.id == $scope.dictionary['Progress'];
				}).map(function (p) {
					return $scope.dates[p.index];
				});
			}

			function init() {
				if (!$scope.graph) $scope.graph = generateChart();
			}

			function toggleViewScheduleDiff() {
				if (!$scope.graph) return;
				$scope.graph.load({
					columns: getDataGroupsData($scope.viewScheduleDiff),
					colors: _setChartOption_color(),
					unload: getDataGroupsLabel(!$scope.viewScheduleDiff)
				});

			}

			function generateChart() {

				var chartOptions = {
					bindto: '#Chart_' + $scope.campaign.Id,
					size: { height: 450 },
					data: _setChartOption_data(),
					axis: _setChartOption_axis(),
					grid: _setChartOption_verticalLines(),
					tooltip: { contents: _setChartOption_tooltip() }
				};
				return c3.generate(chartOptions);
			}
	
			function getDataGroupsData(viewScheduleDiff) {
				return getDataGroupsKey(viewScheduleDiff).map(function(name) {
					return $scope.graphData[name];
				});
			}

			function getDataGroupsLabel(viewScheduleDiff) {
				return getDataGroupsKey(viewScheduleDiff).map(function(name) { return $scope.graphData[name][0]; });			
			}

			function getDataGroupsKey(viewScheduleDiff) {
				var plannedPhase = $scope.campaign.Status;
				if (viewScheduleDiff) {
					if ($filter('showPhase')(plannedPhase) == 'Planned') {
						return ['calculatedBacklogs', 'plans'];
					} else {
						return ['calculatedBacklogs', 'underDiffs', 'overDiffs', 'plans'];
					}
				} else {
					if ($filter('showPhase')(plannedPhase) == 'Planned') {
						return ['rawBacklogs', 'unscheduledPlans', 'progress'];
					} else {
						return ['rawBacklogs', 'schedules', 'unscheduledPlans', 'progress'];
					}
				}
			}

			function getDataIndex(date) {
				return $scope.dates.indexOf(date);
			}

			function _setChartOption_label() {
				var result = {};
				for (var key in $scope.graphData) {
					if (key == 'dates') result['dates'] = 'x';
					else
						result[key] = $scope.graphData[key][0];
				}				
				return result;
			}

			function _setChartOption_color() {
				var colorMap = {
					rawBacklogs: '#1F77B4',
					calculatedBacklogs: '#1F77B4',
					progress: '#2CA02C',
					plans: '#66C2FF',
					unscheduledPlans: '#66C2FF',
					schedules: '#26C6DA',
					underDiffs: '#9467BD',
					overDiffs: '#f44336'
				};

				$scope.campaign.WarningInfo.forEach(function (e) {
					if (e.TypeOfRule == 'OutboundUnderSLARule') {
						colorMap.progress = '#F44336';
					}
					if (e.TypeOfRule == 'OutboundOverstaffRule') {
						colorMap.progress = '#FF7F0E';
					}
				});
				var dataColor = {};
				var labels = _setChartOption_label();

				for (var name in colorMap) {
					dataColor[labels[name]] = colorMap[name];
				}
				return dataColor;
			}

			this._setChartOption_data = _setChartOption_data;
			function _setChartOption_data() {
				var dataOption = {
					x: 'x',
					type: 'bar',
					groups: [getDataGroupsLabel(true), getDataGroupsLabel(false)],
					order: 'null',				
					columns: [$scope.graphData.dates].concat(getDataGroupsData($scope.viewScheduleDiff)),					
					colors: _setChartOption_color(),
					types: {}
				};
				dataOption.types[$scope.dictionary['Progress']] = 'line';
					dataOption.selection = {
						enabled: true,
						grouped: true,
						draggable: true
					};
					dataOption.onselected = function(d) {
						$scope.$evalAsync(graphSelectionChanged);
					};
					dataOption.onunselected = function(d) {
						$scope.$evalAsync(graphSelectionChanged);
					};
				return dataOption;
			}

			function _setChartOption_axis() {

				var option = {
					x: {
						type: 'timeseries',
						tick: {
							format: '%m-%d'
						}
					},
					y: {
						label: {
							text: $scope.dictionary['NeededPersonHours'],
							position: 'outer-top'
						},
						min: 0,
						padding: { bottom: 0 }
					}
				};

				option.y.max = (Math.max.apply(Math, $scope.graphData.rawBacklogs.slice(1)) + Math.max.apply(Math, $scope.graphData.plans.slice(1))) * 1.1;
				return option;
			}

			function _setChartOption_verticalLines() {
				var endDate = new moment($scope.campaign.EndDate.Date),
					todayDate = new moment(),
					startDate = new moment($scope.campaign.StartDate.Date);

				var hints = [
					{ value: endDate.format("YYYY-MM-DD"), text: $scope.dictionary['EndDate'] },
					{ value: startDate.format("YYYY-MM-DD"), text: $scope.dictionary['Start'] }
				];

				if (todayDate >= startDate && todayDate <= endDate) {
					hints.push({ value: todayDate.format("YYYY-MM-DD"), text: $scope.dictionary['Today'] });
				}
				return { x: { lines: hints } };
			}

			function _setChartOption_tooltip() {
				return function (d, defaultTitleFormat, defaultValueFormat, color) {
					var $$ = this, config = $$.config,
						titleFormat = config.tooltip_format_title || defaultTitleFormat,
						nameFormat = config.tooltip_format_name || function (name) { return name; },
						valueFormat = config.tooltip_format_value || defaultValueFormat,
						text, i, title, value, name, bgcolor;
					for (i = d.length - 1 ; i >= 0; i--) {
						if (!(d[i] && (d[i].value))) { continue; }

						if (!text) {
							title = titleFormat ? titleFormat(d[i].x) : d[i].x;
							text = "<table class='" + $$.CLASS.tooltip + "'>" + (title || title === 0 ? "<tr><th colspan='2'>" + title + "</th></tr>" : "");
						}

						name = nameFormat(d[i].name);
						value = valueFormat(d[i].value, d[i].ratio, d[i].id, d[i].index);
						bgcolor = $$.levelColor ? $$.levelColor(d[i].value) : color(d[i].id);

						text += "<tr class='" + $$.CLASS.tooltipName + "-" + d[i].id + "'>";
						text += "<td class='name' style='text-align: left' ><span style='background-color:" + bgcolor + "'></span>" + name + "</td>";
						text += "<td class='value'>" + value + "</td>";
						text += "</tr>";
					}
					if (text)
						return text + "</table>";
					else return '';
				}
			}

		}

		function postlink(scope, elem, attrs, ctrls) {
			var ctrl = ctrls[0];

			scope.$evalAsync(ctrl.init);
			
			scope.onToggleViewScheduleDiff = function () {
				if (scope.campaign.switchSwitch) return;
				scope.viewScheduleDiff = !scope.viewScheduleDiff;
				ctrl.toggleViewScheduleDiff();
			}
			scope.hidePlannedAnalyzeButton = function (d) {
				return ($filter('showPhase')(d) == 'Planned') ? false : true;
			};
			scope.$watch(function() {
				return scope.campaign.selectedDates;
			}, function(newVal, oldVal) {
				if (!scope.graph) return;
				scope.graph.select(null, newVal.map(ctrl.getDataIndex), true);

			}, true);
		}

	};





})();