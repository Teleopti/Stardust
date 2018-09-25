﻿angular.module("wfm.forecasting").component("forecastChart", {
	template: '<div id="{{$ctrl.chartId}}" class="wfm-chart forecast-chart" ng-click=""></div>',
	controller: ForecastChartCtrl,
	bindings: {
		chartId: "=",
		onClick: "=",
		refresh: "=",
		selectable: "=",
		days: "="
	}
});
function ForecastChartCtrl($translate, $timeout, SkillTypeService) {
	var ctrl = this;
	var chart;
	ctrl.refresh = generateForecastChart;

	$timeout(function () {
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
		ctrl.onClick(ctrl.selectedDays);
		var selectedWorkload;
		if (sessionStorage.currentForecastWorkload) {
			selectedWorkload = angular.fromJson(sessionStorage.currentForecastWorkload);
		} else {
			return;
		}

		var dataName = SkillTypeService.getSkillLabels(selectedWorkload.SkillType);
		var preparedData = {
			dateSeries: ["Date"],
			averageAfterTaskTimeSeries: ["AverageAfterTaskTime"],
			tasksSeries: ["Tasks"],
			totalAverageAfterTaskTimeSeries: ["TotalAverageAfterTaskTime"],
			totalTasksSeries: ["TotalTasks"],
			averageTaskTimeSeries: ["AverageTaskTime"],
			totalAverageTaskTimeSeries: ["TotalAverageTaskTime"],
			campaignSeries: ["Campaign"],
			overrideSeries: ["Override"]
		};

		for (var i = 0; i < days.length; i++) {
			preparedData.dateSeries.push(moment(days[i].Date).format("L"));
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
		}

		chart = c3.generate({
			bindto: "#" + chartId,
			data: {
				x: "Date",
				columns: [
					preparedData.totalTasksSeries,
					preparedData.tasksSeries,
					preparedData.totalAverageTaskTimeSeries,
					preparedData.averageTaskTimeSeries,
					preparedData.totalAverageAfterTaskTimeSeries,
					preparedData.averageAfterTaskTimeSeries,
					preparedData.dateSeries,
					preparedData.campaignSeries,
					preparedData.overrideSeries
				],
				colors: {
					TotalTasks: "#0099FF",
					Tasks: "#99D6FF",
					TotalAverageTaskTime: "#77ac39",
					AverageTaskTime: "#C2E085",
					TotalAverageAfterTaskTime: "#eb2e9e",
					AverageAfterTaskTime: "#F488C8",
					Override: "#9C27B0",
					Campaign: "#EF5350",
					CampaignAndOverride: "#888"
				},
				axes: {
					TotalAverageTaskTime: "y2",
					AverageTaskTime: "y2",
					TotalAverageAfterTaskTime: "y2",
					AverageAfterTaskTime: "y2"
				},
				names: {
					TotalTasks: dataName.TotalTasks + "←",
					Tasks: dataName.Tasks + "←",
					AverageAfterTaskTime: dataName.ATW + "→",
					TotalAverageTaskTime: dataName.TotalTaskTime + "→",
					AverageTaskTime: dataName.TaskTime + "→",
					TotalAverageAfterTaskTime: dataName.TotalATW + "→",
					Campaign: $translate.instant("Campaign"),
					Override: $translate.instant("Override")
				},
				hide: [
					"Tasks",
					"AverageTaskTime",
					"AverageAfterTaskTime"
				],
				selection: {
 					draggable: true,
					grouped: true,
					enabled: true
				},
				onselected: function (d) {
					var temp = moment(this.internal.config.axis_x_categories[d.x], "L").format("YYYY-MM-DDT00:00:00");

					if (selectedItems.indexOf(temp) === -1) {
						selectedItems.push(temp);
						ctrl.onClick(selectedItems);
					}
				},
				onunselected: function (d) {
					var temp = moment(this.internal.config.axis_x_categories[d.x], "L").format("YYYY-MM-DDT00:00:00");

					if (selectedItems.indexOf(temp) !== -1) {
						selectedItems.splice(selectedItems.indexOf(temp), 1);
						ctrl.onClick(selectedItems);
					}
				}
			},
			legend: {
				hide: [
					"Override",
					"Campaign"
				]
			},
			subchart: {
				show: true
			},
			axis: {
				x: {
					type: "category",
					tick: {
						culling: {
							max: 10,
							format: "%Y-%m-%d"
						},
						multiline: false
					}},
				y: {
					label: {
						text: $translate.instant("Count"),
						position: "outer-middle"
					},
					padding: {top: 10 ,bottom: 10}
				},

				y2: {
					show: true,
					label: {
						text: $translate.instant("TimeInSecond"),
						position: "outer-middle"
					},
					padding: {top: 10 ,bottom: 10}
				}
			}
		});
	}
}
