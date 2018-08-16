angular.module('wfm.forecasting').component('forecastHistoryChart', {
	templateUrl: 'app/forecasting/refact/forecast-history-chart.html',
	controller: ForecastHistoryChartController,
	bindings: {
		chartId: '=',
		selectable: '=',
		days: '='
	}
});

function ForecastHistoryChartController($translate, $filter, $timeout) {
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

		var labelDate = $translate.instant('Date');
		var labelOriginalTask = $translate.instant('Tasks');
		var labelValidatedTask = $translate.instant('OutlierTasks');

		var preparedData = {
			dateSeries: [labelDate],
			tasksSeries: [labelOriginalTask],
			outlierTasks: [labelValidatedTask]
		};

		for (var i = 0; i < days.length; i++) {
			var date = moment(days[i].Date);
			preparedData.dateSeries.push(date.format('L'));
			preparedData.tasksSeries.push(days[i].Tasks);
			preparedData.outlierTasks.push(days[i].OutlierTasks);
		}

		chart = c3.generate({
			bindto: '#' + chartId,
			data: {
				x: labelDate,
				columns: [preparedData.dateSeries, preparedData.tasksSeries, preparedData.outlierTasks],
				names: {
					Date: labelDate,
					OriginalTasks: labelOriginalTask,
					ValidatedTasks: labelValidatedTask
				},
				colors: {
					OriginalTasks: '#99D6FF',
					ValidatedTasks: '#77ac39'
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
