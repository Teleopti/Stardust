(function() {
	angular.module('wfm.intraday').component('intradayPerformanceChart', {
		templateUrl: 'app/intraday/components/intradayPerformanceChart.html',
		controller: theComponent,
		bindings: {
			chartData: '<'
		}
	});

	theComponent.$inject = ['$translate'];

	function theComponent($translate) {
		var ctrl = this;
		var hiddenArray = [];

		var initChart = function(inData) {
			if (
				angular.isDefined(inData) &&
				angular.isDefined(inData.averageSpeedOfAnswerObj) &&
				angular.isDefined(inData.abandonedRateObj) &&
				angular.isDefined(inData.serviceLevelObj) &&
				angular.isDefined(inData.estimatedServiceLevelObj) &&
				angular.isDefined(inData.timeSeries) &&
				inData.timeSeries.length > 0
			) {
				var columns = [
					inData.timeSeries,
					inData.averageSpeedOfAnswerObj.series,
					inData.serviceLevelObj.series,
					inData.estimatedServiceLevelObj.series
				];

				if (inData.abandonedRateObj.series.length > 0) {
					columns.push(inData.abandonedRateObj.series);
				}
				if (inData.currentInterval.length > 0) {
					columns.push(inData.currentInterval);
				}

				var config = {
					bindto: '#performanceChart',
					data: {
						x: 'x',
						columns: columns,
						hide: hiddenArray,
						type: 'line',
						types: {
							Current: 'bar'
						},
						colors: {
							ASA: '#0099FF',
							Abandoned_rate: '#E91E63',
							Service_level: '#FB8C00',
							ESL: '#F488C8'
						},
						names: {
							ASA: $translate.instant('AverageSpeedOfAnswer') + ' ←',
							Abandoned_rate: $translate.instant('AbandonedRate') + ' →',
							Service_level: $translate.instant('ServiceLevel') + ' →',
							ESL: $translate.instant('ESL') + ' →'
						},
						axes: {
							Service_level: 'y2',
							Abandoned_rate: 'y2',
							ESL: 'y2'
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
								text: $translate.instant('SecondShort'),
								position: 'outer-middle'
							},
							tick: {
								format: d3.format('.1f')
							}
						},
						y2: {
							label: {
								text: $translate.instant('%'),
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
				ctrl.performanceChart = c3.generate(config);
			}
		};

		ctrl.$onChanges = function(changesObj) {
			initChart(changesObj.chartData.currentValue);
		};
	}
})();
