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
		var labelOriginalTask = $translate.instant('OriginalPhoneCalls');
		var labelValidatedTask = $translate.instant('ValidatedPhoneCalls');

		var preparedData = {
			dateSeries: [labelDate],
			originalSeries: [labelOriginalTask],
			validatedSeries: [labelValidatedTask]
		};

		for (var i = 0; i < days.length; i++) {
			var date = moment(days[i].Date);
			preparedData.dateSeries.push(date.format('L'));
			preparedData.originalSeries.push(days[i].OriginalTasks);
			preparedData.validatedSeries.push(days[i].ValidatedTasks);
		}

		chart = c3.generate({
			bindto: '#' + chartId,
			data: {
				x: labelDate,
				columns: [preparedData.dateSeries, preparedData.originalSeries, preparedData.validatedSeries],
				names: {
					Date: labelDate,
					OriginalTasks: labelOriginalTask,
					ValidatedTasks: labelValidatedTask
				},
				colors: {
					ValidatedTasks: '#66c2ff',
					OriginalTasks: '#ee8f7d'
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
				}
			}
		});
	}
}
