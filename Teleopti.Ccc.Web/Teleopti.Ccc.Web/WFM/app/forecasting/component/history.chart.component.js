angular.module('wfm.forecasting').component('forecastHistoryChart', {
	template: '<div id="{{$ctrl.chartId}}" class="wfm-chart forecast-history-chart" ng-click=""></div>',
	controller: ForecastHistoryChartController,
	bindings: {
		chartId: '=',
		selectable: '=',
		days: '='
	}
});

function ForecastHistoryChartController($translate, $filter, $timeout, SkillTypeService) {
	var ctrl = this;
	var chart;
	ctrl.refresh = drawForecastHistoryChart;

	$timeout(function() {
		if (ctrl.days != null && ctrl.days.length > 0) {
			drawForecastHistoryChart(ctrl.chartId, ctrl.days);
			chart.unzoom();
		}
	});

	function drawForecastHistoryChart(chartId, days) {
		if ((!chartId || days.length < 1) && chart) {
			chart.unload();
			return;
		}

		var preparedData = {
			dateSeries: ['Date'],
			originalSeries: ['OriginalTasks'],
			validatedSeries: ['ValidatedTasks']
		};


		var selectItems = [];
		//ctrl.onClick(ctrl.selectedDays);
		var selectedWorkload;
		if (sessionStorage.currentForecastWorkload) {
			selectedWorkload = angular.fromJson(sessionStorage.currentForecastWorkload);
		} else {
			return;
		}
		var dataName = SkillTypeService.getSkillLabels(selectedWorkload.SkillType);


		for (var i = 0; i < days.length; i++) {
			var date = moment(days[i].Date);
			preparedData.dateSeries.push(date.format('L'));
			preparedData.originalSeries.push(days[i].OriginalTasks);
			preparedData.validatedSeries.push(days[i].ValidatedTasks);
		}

		chart = c3.generate({
			bindto: '#' + chartId,
			data: {
				x: 'Date',
				columns: [preparedData.dateSeries, preparedData.originalSeries, preparedData.validatedSeries],

				names: {
					Date: $translate.instant('Date'),
					OriginalTasks: dataName.OriginalTasks,
					ValidatedTasks: dataName.ValidatedTasks
					
				},
				colors: {
					OriginalTasks: '#EE8F7D',
					ValidatedTasks: '#0099FF'
				}
			},
			subchart: {
				show: true
			},
			tooltip: {
				format: {
					value: d3.format('.1f')
				}
			},
			axis: {
				x: {
					type: 'category',
					tick: {
						culling: {
							max: 10
						},
						multiline: false
					}
				},
				y: {
					label: {
						text: $translate.instant('Count'),
						position: 'outer-middle'
					}
				}
			}
		});
	}
}
