(function() {
	function theComponent($translate, $log) {
		var ctrl = this;

		var initChart = function(inData) {
			var hiddenArray = [];
			if (angular.isDefined(inData) && angular.isDefined(inData.forecastedStaffing)) {
				ctrl.staffingChart = c3.generate({
					bindto: '#staffingChart',
					data: {
						x: 'x',
						columns: [
							inData.timeSeries,
							inData.forecastedStaffing.series,
							inData.forecastedStaffing.updatedSeries,
							inData.actualStaffingSeries,
							inData.scheduledStaffing
						],
						type: 'line',
						hide: hiddenArray,
						names: {
							Forecasted_staffing: $translate.instant('ForecastedStaff') + ' ←',
							Updated_forecasted_staffing: $translate.instant('ReforecastedStaff') + ' ←',
							Actual_staffing: $translate.instant('RequiredStaff') + ' ←',
							Scheduled_staffing: $translate.instant('ScheduledStaff') + ' ←'
						},
						colors: {
							Forecasted_staffing: '#0099FF',
							Updated_forecasted_staffing: '#E91E63',
							Actual_staffing: '#FB8C00',
							Scheduled_staffing: '#F488C8'
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
								text: $translate.instant('Agents'),
								position: 'outer-middle'
							},
							tick: {
								format: d3.format('.1f')
							}
						},
						y2: {
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
								ctrl.initChart(ctrl.chartData);
							}
						}
					},
					transition: {
						duration: 500
					}
				});
			}
		};

		ctrl.$onChanges = function(changesObj) {
			initChart(changesObj.chartData.currentValue);
		};
	}

	angular.module('wfm.intraday').component('intradayStaffingChart', {
		templateUrl: 'app/intraday/components/intradayStaffingChart.html',
		controller: theComponent,
		bindings: {
			chartData: '<'
		}
	});

	theComponent.$inject = ['$translate', '$log'];
})();
