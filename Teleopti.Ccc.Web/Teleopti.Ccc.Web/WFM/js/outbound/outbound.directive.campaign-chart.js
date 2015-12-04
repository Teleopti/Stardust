(function () {
	'use strict';

	angular.module('wfm.outbound').directive('campaignChart', ['$filter', '$http', campaignChart]);

	function campaignChart($filter, $http) {
		return {
			controller: ['$scope', '$element',  campaignChartCtrl],
			template: '<div id="Chart_{{campaign.Id}}"></div>' +
				'<div class="chart-extra-info"><p ng-repeat="extraInfo in extraInfos"}>{{extraInfo}}</p></div>' +
				'<div class="clear-button"><i class="mdi mdi-block-helper pull-right toggle-handle" ng-click="clearSelectedDates()"><md-tooltip>Clear chart selection</md-tooltip></i></div>',
			scope: {
				'campaign': '=',				
				'dictionary': '='
			},
			require: ['campaignChart'],
			link: postlink
		};

		function campaignChartCtrl($scope, $element) {

			$scope.viewScheduleDiff = false;
			$scope.dates = $scope.campaign.graphData['dates'].slice(1);

			this.loadGraph = loadGraph;
			this.init = init;
			this.getDataIndex = getDataIndex;
			this.generateChart = generateChart;
			this.determineOpenDay = determineOpenDay;
			
			$scope.campaign.manualPlan = {};
			$scope.campaign.backlog = {};		
			$scope.campaign.selectedDates = [];
			$scope.campaign.selectedDatesClosed = [];

			$scope.extraInfos = [
				"M: " + $scope.dictionary['ManualPlan'],
				"C: " + $scope.dictionary['ClosedDay'],
				"B: " + $scope.dictionary['AddBacklog']
			];

			$scope.clearSelectedDates = function() {
				$scope.campaign.selectedDates = [];
				$scope.campaign.selectedDatesClosed = [];
				$scope.graph.unselect(['Progress']);
			};

			function graphSelectionChanged() {
				var selectedData = $scope.graph.selected().filter(function(p) {
					return p.id == $scope.dictionary['Progress'];
				});
				$scope.campaign.selectedDates = selectedData.map(function (p) {
					return $scope.dates[p.index];
				});
				$scope.campaign.selectedDatesClosed = selectedData.filter(function(p) {
					return !determineOpenDay(p.index);
				}).map(function (p) {
					return $scope.dates[p.index];
				});
			}

			function determineScheduledDay(idx) {
				return $scope.campaign.graphData.schedules[idx + 1] > 0;
			}

			function determineOpenDay(idx) {
				return ! $scope.campaign.closedDays[idx - 1];
			}

			function determineManualPlanDay(idx) {
				return $scope.campaign.rawManualPlan[idx - 1];
			}

			function determineManualBacklogDay(idx) {
				return $scope.campaign.isManualBacklog[idx - 1];
			}

			function init() {
				if (!$scope.graph) $scope.graph = generateChart();
			}

			

			function loadGraph(data) {
				if (!$scope.graph) return;
				$scope.campaign.selectedDates = [];
				$scope.campaign.selectedDatesClosed = [];
				if (data) $scope.campaign.WarningInfo = data.WarningInfo;

				var yMax = _calculateYMax();

				if (yMax >= $scope.campaign.formerOptionYMax || yMax <= $scope.campaign.formerOptionYMax * 0.8) {
					$scope.graph.axis.max(yMax);
					$scope.campaign.formerOptionYMax = yMax;
				}
				
				$scope.graph.load({
					columns: getChartData(),
					colors: _setChartOption_color()
				});
			}

			function generateChart() {
				if (c3.applyFix) c3.applyFix();
				var chartOptions = {
					bindto: '#Chart_' + $scope.campaign.Id,
					size: { height: 450 },
					data: _setChartOption_data(),
					axis: _setChartOption_axis(),
					grid: _setChartOption_verticalLines(),
					tooltip: { contents: _setChartOption_tooltip() },
					transition: { duration: null}
					
				};
				return c3.generate(chartOptions);
			}
	
			function getChartData() {
				return getChartDataKey().map(function (name) {return $scope.campaign.graphData[name];});
			}

			function getChartLabel() {
				return getChartDataKey().map(function (name) {
					return $scope.campaign.graphData[name][0];
				});
			}

			function getChartDataKey() {
				var plannedPhase = $scope.campaign.Status;
				if ($filter('showPhase')(plannedPhase) == 'Planned') {
					return ['rawBacklogs', 'unscheduledPlans', 'progress', 'overStaff'];
				} else {
					return ['rawBacklogs', 'schedules', 'unscheduledPlans', 'progress', 'overStaff'];
				}
			}

			function getDataIndex(date) {
				return $scope.dates.indexOf(date);
			}

			function _setChartOption_label() {
				var result = {};
				for (var key in $scope.campaign.graphData) {
					if (key == 'dates') result['dates'] = 'x';
					else
						result[key] = $scope.campaign.graphData[key][0];
				}				
				return result;
			}

			function _setChartOption_color() {
				var colorMap = {
					rawBacklogs: '#1F77B4',
					progress: '#2CA02C',
					unscheduledPlans: '#ffc36b',
					schedules: '#C2E085',
					overStaff: '#4FC3F7'
				};

				$scope.campaign.WarningInfo.forEach(function (e) {
					if (e.TypeOfRule == 'CampaignUnderSLA') {
						colorMap.progress = '#F44336';
					}
					if (e.TypeOfRule == 'CampaignOverstaff') {
						colorMap.progress = '#4FC3F7';
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
					groups: [getChartLabel()],
					order: 'null',				
					columns: [$scope.campaign.graphData.dates].concat(getChartData()),
					colors: _setChartOption_color(),
					types: {},
					labels: {
						format: {}													
					}
				};
				dataOption.labels.format[$scope.dictionary['Overstaff']] = function(v, id, i) {
					if ((!determineOpenDay(i)) && determineManualBacklogDay(i)) return 'C,B';
					else if (!determineOpenDay(i)) return 'C';

					if (determineManualPlanDay(i) && !determineScheduledDay(i) && determineManualBacklogDay(i)) return 'M,B';
					else if (determineManualPlanDay(i) && !determineScheduledDay(i)) return 'M';
					else if (determineManualBacklogDay(i)) return "B";
				};
				dataOption.types[$scope.dictionary['Progress']] = 'line';
					dataOption.selection = {
						enabled: true,
						grouped: true,
						draggable: true,					
					};
					dataOption.onselected = function(d) {
						$scope.$evalAsync(graphSelectionChanged);
					};
					dataOption.onunselected = function(d) {
						$scope.$evalAsync(graphSelectionChanged);
					};
				return dataOption;
			}

			this._setChartOption_axis = _setChartOption_axis;

			function _calculateYMax() {
				return (Math.max.apply(Math, $scope.campaign.graphData.rawBacklogs.slice(1)) + Math.max.apply(Math, $scope.campaign.graphData.unscheduledPlans.slice(1))) * 1.1;
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

				$scope.campaign.formerOptionYMax = _calculateYMax();
				option.y.max = $scope.campaign.formerOptionYMax;
				return option;
			}

			function _setChartOption_verticalLines() {
				var endDate = new moment($scope.campaign.CampaignSummary.EndDate.Date),
					todayDate = new moment(),
					startDate = new moment($scope.campaign.CampaignSummary.StartDate.Date);

				var hints = [];

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
						

						if (!text) {
							title = titleFormat ? titleFormat(d[i].x) : d[i].x;
							text = "<table class='" + $$.CLASS.tooltip + "'>" + (title || title === 0 ? "<tr><th colspan='2'>" + title + "</th></tr>" : "");
						}

						if (!(d[i] && (d[i].value))) { continue; }

						name = nameFormat(d[i].name);
						value = valueFormat(d[i].value, d[i].ratio, d[i].id, d[i].index);
						var valueAsString = $filter('number')(value);
						

						bgcolor = $$.levelColor ? $$.levelColor(d[i].value) : color(d[i].id);

						text += "<tr class='" + $$.CLASS.tooltipName + "-" + d[i].id + "'>";
						text += "<td class='name' style='text-align: left' ><span style='background-color:" + bgcolor + "'></span>" + name + "</td>";
						text += "<td class='value'>" + valueAsString + "</td>";
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

			scope.$on('campaign.chart.refresh', function (_s, data) {
				if (scope.campaign.Id == data.Id) {
					scope.$evalAsync(ctrl.loadGraph(data));
				}
			});

			scope.$on('campaign.chart.clear.selection', function(_s, data) {
				if (scope.campaign.Id == data.Id) {
					scope.graph.unselect();
				}
			});
		}

	};

})();