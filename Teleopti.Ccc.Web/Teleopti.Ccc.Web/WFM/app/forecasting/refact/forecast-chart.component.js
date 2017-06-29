angular.module('wfm.forecasting')
.component('forecastChart',
{
	templateUrl: 'app/forecasting/refact/forecast-chart.html',
	controller: ForecastChartCtrl,
	bindings: {
		chartId: '=',
		onClick: '=',
		refresh: '=',
		selectable: '='
	}
});

function ForecastChartCtrl($translate, $filter) {
	var ctrl = this;
	var chart;
	var selection = 0;
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

	function generateForecastChart(chartId, days) {
		console.log('Start generate');
		if (!chartId || days.length === 0 ) {
			console.log('Could not generate forcast chart');
			return;
		}

		var selection = 0;
		var preparedData = {};
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

		for (var i = 0; i < days.length; i++) {
			preparedData.dateSeries.push(moment(days[i].date).format('L'));
			preparedData.vacwSeries.push(days[i].vacw);
			preparedData.vcSeries.push(days[i].vc);
			preparedData.vtacwSeries.push(days[i].vtacw);
			preparedData.vtcSeries.push(days[i].vtc);
			preparedData.vttSeries.push(days[i].vtt);
			preparedData.vtttSeries.push(days[i].vttt);
		}

		chart = c3.generate({
			bindto: '#' + chartId,
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
					enabled: ctrl.selectable,
					draggable: true,
					grouped: true
				},
				onselected: function(){
					selection = chart.selected();
					ctrl.onClick(selection);
				},
				onunselected: function () {
					selection = chart.selected();
					ctrl.onClick(selection);
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
