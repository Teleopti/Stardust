(function () {
	'use strict';

	angular.module('wfm.forecasting').directive('forecastResultChart', ['$translate', '$filter', forecastResultChart]);

	function forecastResultChart($translate, $filter) {
		return {
			controller: ['$scope', forecastResultChartCtrl],
			templateUrl: "js/forecasting/html/forecasting-result-chart.html",
			scope: {
				'chartId': '=',
				'refreshFn': '=',
				'selectedFn': '=',
				'selectedDayCount': '='
			},
			require: ['forecastResultChart'],
			link: postlink
		};

		function forecastResultChartCtrl($scope) {
			$scope.clearSelection = clearSelection;
			$scope.refreshFn = generateChart;
			$scope.selectedFn = selected;
			$scope.selectedDayCount = 0;

			function clearSelection() {
				$scope.chart.unzoom();
				$scope.chart.unselect(['vc']);
				$scope.selectedDayCount = 0;
			};

			function selected() {
				if ($scope.chart)
					return $scope.chart.selected();
				return [];
			}

			function generateChart(chartData) {

				if (c3.applyFixForForecast) c3.applyFixForForecast(function () {
					$scope.$evalAsync(function () {
						$scope.selectedDayCount = $filter('filter')($scope.chart.selected(), { id: 'vtc' }).length;
					});
				});
				$scope.selectedDayCount = 0;
				$scope.chart = c3.generate({
					bindto: "#" + $scope.chartId,
					data: {
						json:
							chartData,
						keys: {
							// x: 'name', // it's possible to specify 'x' when category axis
							x: 'date',
							value: ['vtc', 'vc', 'vttt', 'vtt', 'vtacw', 'vacw', 'vcampaign', 'voverride', 'vcombo']
						},
						axes: {
							vttt: 'y2',
							vtt: 'y2',
							vtacw: 'y2',
							vacw: 'y2'
						},
						selection: {
							enabled: true,
							grouped: true,
							draggable: true,
							isselectable: function (chartPoint) {
								if (chartPoint.id === 'vtt' || chartPoint.id === 'vacw' || chartPoint.id === 'vcampaign' || chartPoint.id === 'voverride' || chartPoint.id === 'vcombo')
									return false;
								return true;
							}
						},
						names: {
							vtc: $translate.instant('TotalCallsCaret'),
							vc: $translate.instant('CallsCaret'),
							vttt: $translate.instant('TotalTalkTimeCaret'),
							vtt: $translate.instant('TalkTimeCaret'),
							vtacw: $translate.instant('TotalAcwCaret'),
							vacw: $translate.instant('AcwCaret'),
							voverride: $translate.instant('ForecastValueOverride'),
							vcampaign: $translate.instant('CampaignAdded'),
							vcombo: $translate.instant('BothOverrideAndCampaignAdded')
						},
						colors: {
							vtc: '#0099FF',
							vc: '#99D6FF',
							vttt: '#77ac39',
							vtt: '#C2E085',
							vtacw: '#eb2e9e',
							vacw: '#F488C8',
							vcampaign: 'tomato',
							voverride: 'purple',
							vcombo: 'gray'
						},
						hide: ['vc', 'vtt', 'vacw'],
						onclick: function (chartPoint) {
							if (chartPoint.id === 'vtc') {
								$scope.$evalAsync(function() {
									$scope.selectedDayCount = $filter('filter')($scope.chart.selected(), { id: 'vtc' }).length;
								});
							}
						}
					},
					axis: {
						x: {
							type: 'timeseries',
							tick: {
								format: '%Y-%m-%d'
							}
						},
						y2: {
							show: true
						}
					},
					subchart: {
						show: true
					},
					tooltip: {
						format: {
							value: d3.format('.1f')
						}
					}
				});
			}
		}

		function postlink(scope, elem, attrs, ctrls) {

		};
	}
})();
