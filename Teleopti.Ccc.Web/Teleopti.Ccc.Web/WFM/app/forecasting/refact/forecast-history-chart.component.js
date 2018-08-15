angular.module('wfm.forecasting').component('forecastHistoryChart', {
	templateUrl: 'app/forecasting/refact/forecast-history-chart.html',
	controller: ForecastHistoryChartCtrl,
	bindings: {
		chartId: '=',
		selectable: '=',
		days: '='
	}
});

function ForecastHistoryChartCtrl($translate, $filter, $timeout) {
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
			tasksSeries: ['Tasks'],
			outlierTasks: ['OutlierTasks']
		};

		for (var i = 0; i < days.length; i++) {
			preparedData.dateSeries.push(moment(days[i].Date).format('DD/MM/YYYY'));
			preparedData.tasksSeries.push(days[i].Tasks);
			preparedData.outlierTasks.push(days[i].OutlierTasks);
		}

		console.log(preparedData);

		chart = c3.generate({
			bindto: '#' + chartId,
			data: {
				x: 'Date',
				columns: [preparedData.dateSeries, preparedData.tasksSeries, preparedData.outlierTasks],
				names: {
					Tasks: $translate.instant('Tasks'),
					OutlierTasks: $translate.instant('OutlierTasks')
				},
				colors: {
					Tasks1: '#99D6FF',
					OutlierTasks: '#77ac39'
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
							max: preparedData.dateSeries.length / 28
						},
						multiline: false
					}
				}
			}
		});
	}
}
