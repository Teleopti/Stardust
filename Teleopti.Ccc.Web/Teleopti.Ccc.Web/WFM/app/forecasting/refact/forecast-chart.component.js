angular.module('wfm.forecasting')
.component('forecastChart',
{
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

	$timeout(function () {
		if (ctrl.days != null && ctrl.days.length > 0) {
			generateForecastChart(ctrl.chartId,  ctrl.days);
			chart.unzoom();
		}
	});

	function generateForecastChart(chartId, days) {
		console.log('Start generate');
		if (!chartId || days.length === 0 ) {
			console.log('Could not generate forcast chart');
			return;
		}

		var selectedItems = [];
		var preparedData = {};
		ctrl.onClick(ctrl.selectedDays);

		var preparedData = {
			dateSeries: ['date'],
			vacwSeries: ['vacw'],
			vcSeries: ['vc'],
			vtacwSeries: ['vtacw'],
			vtcSeries: ['vtc'],
			vttSeries: ['vtt'],
			vtttSeries: ['vttt'],
			vcamSeries: ['vcampaign'],
			voverrideSeries: ['voverride'],
			vcomboSeries: ['vcombo']
		}

		for (var i = 0; i < days.length; i++) {
			preparedData.dateSeries.push(moment(days[i].date).format('DD/MM/YYYY'));
			preparedData.vacwSeries.push(days[i].vacw);
			preparedData.vcSeries.push(days[i].vc);
			preparedData.vtacwSeries.push(days[i].vtacw);
			preparedData.vtcSeries.push(days[i].vtc);
			preparedData.vttSeries.push(days[i].vtt);
			preparedData.vtttSeries.push(days[i].vttt);

			if (days[i].vcampaign) {
				preparedData.vcamSeries.push(days[i].vcampaign);
			} else{
				preparedData.vcamSeries.push(null);
			}

			if (days[i].voverride) {
				preparedData.voverrideSeries.push(days[i].voverride);
			} else{
				preparedData.voverrideSeries.push(null);
			}

			if (days[i].vcombo) {
				preparedData.vcomboSeries.push(days[i].vcombo);
			} else{
				preparedData.vcomboSeries.push(null);
			}
		}

		chart = c3.generate({
			bindto: '#' + chartId,
			data: {
				x: 'date',
				columns: [
					preparedData.dateSeries,
					preparedData.vcamSeries,
					preparedData.voverrideSeries,
					preparedData.vcomboSeries,

					preparedData.vtcSeries,
					preparedData.vcSeries,

					preparedData.vtacwSeries,
					preparedData.vacwSeries,

					preparedData.vtttSeries,
					preparedData.vttSeries
				],
				names: {
					vtc: $translate.instant('TotalCallsCaret'),
					vcampaign: $translate.instant('Campaign'),
					voverride: $translate.instant('Override'),
					vcombo: $translate.instant('BothOverrideAndCampaignAdded'),
					vc: $translate.instant('CallsCaret'),
					vttt: $translate.instant('TotalTalkTimeCaret'),
					vtt: $translate.instant('TalkTimeCaret'),
					vtacw: $translate.instant('TotalAcwCaret'),
					vacw: $translate.instant('AcwCaret'),
				},
				colors: {
					vtc: '#0099FF',
					vc: '#99D6FF',
					vttt: '#77ac39',
					vtt: '#C2E085',
					vtacw: '#eb2e9e',
					vacw: '#F488C8',
					voverride: '#9C27B0',
					vcampaign: '#EF5350',
					vcombo: '#888'
				},
				hide: ['vc', 'vtt', 'vacw'],
				selection: {
					enabled: ctrl.selectable,
					draggable: true,
					grouped: true
				},
				onselected: function(d){
					var temp = moment(this.internal.config.axis_x_categories[d.x], 'DD/MM/YYYY').format('YYYY-MM-DDTHH:MM:SSZ');

					if (selectedItems.indexOf(temp) == -1) {
						selectedItems.push({date:temp});
					}
					ctrl.onClick(selectedItems);
				},
				onunselected: function (d) {
					var temp = moment(this.internal.config.axis_x_categories[d.x], 'DD/MM/YYYY').format('YYYY-MM-DDTHH:MM:SSZ');
					if (selectedItems.indexOf(temp) == -1) {
						selectedItems.splice(selectedItems.indexOf(temp),1);
					}
					ctrl.onClick(selectedItems);
				}
			},//end of data
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
				vttt: 'y2',
				vtt: 'y2',
				vtacw: 'y2',
				vacw: 'y2',
				x: {
					type: 'category',
					tick: {
						culling: {
							max: preparedData.dateSeries.length/4
						},
						multiline: false
					}
				}
			}
		});
	}

}
