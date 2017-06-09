angular.module('wfm.forecasting')
.component('forecastChart',
{
	templateUrl: 'app/forecasting/refact/forecast-chart.html',
	controller: ForecastChartCtrl,
	bindings: {
		chartId: '=',
		days: '=',
		onClick: '=',
		refresh: '='
	}
});

function ForecastChartCtrl($translate, $filter) {
	var ctrl = this;

	ctrl.selectedDays = [];
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

		generateForecastChart();

	function generateForecastChart() {
		if (!ctrl.chartId || ctrl.days.length === 0 ) {
			console.log('Could not generate forcast chart');
			return;
		}

		ctrl.selectedDays = [];
		ctrl.onClick(ctrl.selectedDays);

		var preparedData = {
			dateSeries: ['date'],
			vacwSeries: ['vacw'],
			vcSeries: ['vc'],
			vtacwSeries: ['vtacw'],
			vtcSeries: ['vtc'],
			vttSeries: ['vtt'],
			vtttSeries: ['vttt']
		}

		for (var i = 0; i < ctrl.days.length; i++) {
			preparedData.dateSeries.push(moment(ctrl.days[i].date).format('DD/MM/YYYY'));
			preparedData.vacwSeries.push(ctrl.days[i].vacw);
			preparedData.vcSeries.push(ctrl.days[i].vc);
			preparedData.vtacwSeries.push(ctrl.days[i].vtacw);
			preparedData.vtcSeries.push(ctrl.days[i].vtc);
			preparedData.vttSeries.push(ctrl.days[i].vtt);
			preparedData.vtttSeries.push(ctrl.days[i].vttt);
		}

		var chart = c3.generate({
			bindto: '#' + ctrl.chartId,
			data: {
				x: 'date',
				columns: [
					preparedData.dateSeries,

					preparedData.vtcSeries,
					preparedData.vcSeries,

					preparedData.vtacwSeries,
					preparedData.vacwSeries,

					preparedData.vtttSeries,
					preparedData.vttSeries
				],
				names: {
					vtc: $translate.instant('TotalCallsCaret'),
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
					vacw: '#F488C8'
				},
				hide: ['vc', 'vtt', 'vacw'],
				selection: {
					enabled: true,
					draggable: true,
					grouped: true
				},
				onselected: function(){
					var selection = chart.selected();
					for (var i = 0; i < selection.length; i++) {
						if (!checkSelection(ctrl.selectedDays, selection[i].index)) {
							ctrl.selectedDays.push(selection[i].index);
						} else{
							var index = ctrl.selectedDays.indexOf(selection[i].index)
							if (index > -1) {
								ctrl.selectedDays.splice(index, 1);
							}
						}
					}
					ctrl.onClick(ctrl.selectedDays);
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
