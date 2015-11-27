(function () {
	'use strict';

	angular.module('wfm.forecasting').directive('forecastResultChart', ['$translate', forecastResultChart]);

	function forecastResultChart($translate) {
		return {
			controller: ['$scope', forecastResultChartCtrl],
			templateUrl: "js/forecasting/html/forecasting-result-chart.html",
			scope: {
				'chartId': '=',
				'refreshFn': '=',
				'selectedFn': '='
			},
			require: ['forecastResultChart'],
			link: postlink
		};

		function forecastResultChartCtrl($scope) {
			$scope.clearSelection = clearSelection;
			$scope.refreshFn = generateChart;
			$scope.selectedFn = selected;

			function clearSelection() {
				$scope.chart.unzoom();
				$scope.chart.unselect(['vc']);
			};

			function selected() {
				if ($scope.chart)
					return $scope.chart.selected();
				return [];
			}

			function generateChart(chartData) {

				$scope.chart = c3.generate({
					bindto: "#" + $scope.chartId,
					data: {
						json:
							chartData,
						keys: {
							// x: 'name', // it's possible to specify 'x' when category axis
							x: 'date',
							value: ['vtc', 'vc', 'vttt', 'vtt', 'vtacw', 'vacw', 'vcampaign', 'voverride']
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
								if (chartPoint.id === 'vttt' || chartPoint.id === 'vtacw' || chartPoint.id === 'vtc' || chartPoint.id === 'vcampaign' || chartPoint.id === 'voverride')
									return false;
								return true;
							}
						},
						names: {
							vtc: $translate.instant('TotalCallsCaret'),
							vc: $translate.instant('CallsCaret'),
							vttt: 'Total Talk time >',
							vtt: $translate.instant('TalkTimeCaret'),
							vtacw: 'Total ACW >',
							vacw: $translate.instant('AcwCaret'),
							voverride: $translate.instant('ForecastValueOverride'),
							vcampaign: $translate.instant('PartOfCampaign')
						},
						colors: {
							vtc: '#0099FF',
							vc: '#99D6FF',
							vttt: '#008000',
							vtt: '#9CCC65',
							vtacw: '#800080',
							vacw: '#F488C8',
							vcampaign: 'tomato',
							voverride: 'purple'
						},
						onclick: function (chartPoint) {
							if (chartPoint.id === 'vc' || chartPoint.id === 'vtt' || chartPoint.id === 'vacw')
								$scope.$apply();
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
