angular.module('wfm.forecasting').component('forecastChart', {
	templateUrl: 'app/forecasting/refact/forecast-chart.html',
	controller: ForecastChartCtrl,
	bindings: {
		chartId: '=',
		onClick: '=',
		refresh: '=',
		selectable: '=',
		days: '='
	}
});

function ForecastChartCtrl($translate, $filter, $timeout) {
	var ctrl = this;
	var chart;
	var selectedItems = [];
	ctrl.refresh = generateForecastChart;

	function checkSelection(arr, item) {
		var i = arr.length;
		while (i--) {
			if (arr[i] === item) {
				return true;
			}
		}
		return false;
	}

	$timeout(function() {
		if (ctrl.days != null && ctrl.days.length > 0) {
			generateForecastChart(ctrl.chartId, ctrl.days);
			chart.unzoom();
		}
	});

	function generateForecastChart(chartId, days) {
		if ((!chartId || days.length < 1) && chart) {
			chart.unload();
			return;
		}

		var selectedItems = [];
		var preparedData = {};
		ctrl.onClick(ctrl.selectedDays);

		var preparedData = {
			dateSeries: ['Date'],
			averageAfterTaskTimeSeries: ['AverageAfterTaskTime'],
			tasksSeries: ['Tasks'],
			totalAverageAfterTaskTimeSeries: ['TotalAverageAfterTaskTime'],
			totalTasksSeries: ['TotalTasks'],
			averageTaskTimeSeries: ['AverageTaskTime'],
			totalAverageTaskTimeSeries: ['TotalAverageTaskTime'],
			campaignSeries: ['Campaign'],
			overrideSeries: ['Override'],
			campaignAndOverrideSeries: ['CampaignAndOverride']
		};
		for (var i = 0; i < days.length; i++) {
			preparedData.dateSeries.push(moment(days[i].Date).format('DD/MM/YYYY'));
			preparedData.averageAfterTaskTimeSeries.push(days[i].AverageAfterTaskTime);
			preparedData.tasksSeries.push(days[i].Tasks);
			preparedData.totalAverageAfterTaskTimeSeries.push(days[i].TotalAverageAfterTaskTime);
			preparedData.totalTasksSeries.push(days[i].TotalTasks);
			preparedData.averageTaskTimeSeries.push(days[i].AverageTaskTime);
			preparedData.totalAverageTaskTimeSeries.push(days[i].TotalAverageTaskTime);

			if (days[i].HasCampaign) {
				preparedData.campaignSeries.push(1);
			} else {
				preparedData.campaignSeries.push(null);
			}

			if (days[i].HasOverride) {
				preparedData.overrideSeries.push(1);
			} else {
				preparedData.overrideSeries.push(null);
			}

			if (days[i].HasOverride && days[i].HasCampaign) {
				preparedData.campaignAndOverrideSeries.push(1);
			} else {
				preparedData.campaignAndOverrideSeries.push(null);
			}
		}

		chart = c3.generate({
			bindto: '#' + chartId,
			data: {
				x: 'Date',
				columns: [
					preparedData.totalTasksSeries,
					preparedData.tasksSeries,

					preparedData.totalAverageAfterTaskTimeSeries,
					preparedData.averageAfterTaskTimeSeries,

					preparedData.totalAverageTaskTimeSeries,
					preparedData.averageTaskTimeSeries,

					preparedData.dateSeries,
					preparedData.campaignSeries,
					preparedData.overrideSeries,
					preparedData.campaignAndOverrideSeries
				],
				names: {
					TotalTasks: $translate.instant('TotalCalls' + ' ←'),
					Campaign: $translate.instant('Campaign'),
					Override: $translate.instant('Override'),
					CampaignAndOverride: $translate.instant('BothOverrideAndCampaignAdded'),
					Tasks: $translate.instant('Calls' + ' ←'),
					TotalAverageTaskTime: $translate.instant('TotalTalkTime' + ' ←'),
					AverageTaskTime: $translate.instant('TalkTime' + ' ←'),
					TotalAverageAfterTaskTime: $translate.instant('TotalACW' + ' ←'),
					AverageAfterTaskTime: $translate.instant('ACW' + ' ←')
				},
				colors: {
					TotalTasks: '#0099FF',
					Tasks: '#99D6FF',
					TotalAverageTaskTime: '#77ac39',
					AverageTaskTime: '#C2E085',
					TotalAverageAfterTaskTime: '#eb2e9e',
					AverageAfterTaskTime: '#F488C8',
					Override: '#9C27B0',
					Campaign: '#EF5350',
					CampaignAndOverride: '#888'
				},
				hide: ['Tasks', 'AverageTaskTime', 'AverageAfterTaskTime'],
				selection: {
					enabled: ctrl.selectable,
					draggable: true,
					grouped: true
				},
				onselected: function(d) {
					var temp = moment(this.internal.config.axis_x_categories[d.x], 'DD/MM/YYYY').format(
						'YYYY-MM-DDT00:00:00'
					);

					if (selectedItems.indexOf(temp) == -1) {
						selectedItems.push(temp);
						ctrl.onClick(selectedItems);
					}
				},
				onunselected: function(d) {
					var temp = moment(this.internal.config.axis_x_categories[d.x], 'DD/MM/YYYY').format(
						'YYYY-MM-DDT00:00:00'
					);
					if (selectedItems.indexOf(temp) !== -1) {
						selectedItems.splice(selectedItems.indexOf(temp), 1);
						ctrl.onClick(selectedItems);
					}
				}
			}, //end of data
			point: {
				focus: {
					expand: {
						enabled: false
					}
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
				y2: {
					show: true
				},
				TotalAverageTaskTime: 'y2',
				AverageTaskTime: 'y2',
				TotalAverageAfterTaskTime: 'y2',
				AverageAfterTaskTime: 'y2',
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
