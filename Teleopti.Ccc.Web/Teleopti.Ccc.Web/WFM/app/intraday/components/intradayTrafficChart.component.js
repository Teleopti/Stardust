(function() {
	angular.module('wfm.intraday').component('intradayTrafficChart', {
		templateUrl: 'app/intraday/components/intradayTrafficChart.html',
		controller: theComponent,
		bindings: {
			chartData: '<'
		}
	});

	theComponent.$inject = ['$translate', '$log'];

	function theComponent($translate, $log) {
		var ctrl = this;
		var hiddenArray = [];

		var initChart = function(inData) {
			if (
				angular.isDefined(inData) &&
				angular.isDefined(inData.forecastedCallsObj) &&
				angular.isDefined(inData.actualCallsObj) &&
				angular.isDefined(inData.forecastedAverageHandleTimeObj) &&
				angular.isDefined(inData.actualAverageHandleTimeObj) &&
				angular.isDefined(inData.timeSeries) &&
				inData.timeSeries.length > 0
			) {
				var config = {
					bindto: '#trafficChart',
					data: {
						x: 'x',
						columns: [
							inData.timeSeries,
							inData.forecastedCallsObj.series,
							inData.actualCallsObj.series,
							inData.forecastedAverageHandleTimeObj.series,
							inData.actualAverageHandleTimeObj.series
							// inData.currentInterval
						],
						hide: hiddenArray,
						types: {
							Current: 'bar'
						},
						colors: {
							Forecasted_calls: '#99D6FF',
							Calls: '#0099FF',
							Forecasted_AHT: '#FFC285',
							AHT: '#FB8C00'
						},
						names: {
							Forecasted_calls: $translate.instant('ForecastedVolume') + ' ←',
							Calls: $translate.instant('ActualVolume') + ' ←',
							Forecasted_AHT: $translate.instant('ForecastedAverageHandlingTime') + ' →',
							AHT: $translate.instant('ActualAverageHandlingTime') + ' →'
						},
						axes: {
							Forecasted_AHT: 'y2',
							AHT: 'y2',
							Calls: 'y',
							Forecasted_calls: 'y'
						}
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
						},
						y: {
							label: {
								text: $translate.instant('Volume'),
								position: 'outer-middle'
							},
							tick: {
								format: d3.format('.1f')
							}
						},
						y2: {
							label: {
								text: $translate.instant('AverageHandlingTime'),
								position: 'outer-middle'
							},
							show: true,
							tick: {
								format: d3.format('.1f')
							}
						}
					},
					legend: {
						item: {
							onclick: function(id) {
								if (hiddenArray.indexOf(id) > -1) {
									hiddenArray.splice(hiddenArray.indexOf(id), 1);
								} else {
									hiddenArray.push(id);
								}
								initChart(ctrl.chartData);
							}
						}
					},
					transition: {
						duration: 500
					}
				};
				ctrl.trafficChart = c3.generate(config);
			}
		};

		ctrl.$onChanges = function(changesObj) {
			initChart(changesObj.chartData.currentValue);
		};
	}
})();
