import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import c3 from 'c3';
import * as moment from 'moment';
import { IntradayLatestTimeData } from '../../types';
import { IntradayChartType, IIntradayAxisLabels } from '../../types/intraday-chart-type';

@Component({
	selector: 'app-intraday-chart',
	templateUrl: './intraday-chart.html'
})
export class IntradayChartComponent implements OnChanges {
	chart: c3.ChartAPI;
	hiddenArray: string[] = [];

	constructor(private translate: TranslateService) {}

	@Input()
	chartData: c3.Data;

	@Input()
	chartType: IntradayChartType;

	@Input()
	axisLabels: IIntradayAxisLabels;

	@Input()
	latestTime: IntradayLatestTimeData | undefined;

	emptyChart = {
		x: 'x',
		xFormat: '%Y-%m-%d %H:%M',
		type: 'area-spline',
		columns: [],
		empty: { label: { text: this.translate.instant('NoDataAvailable') } }
	};

	ngOnChanges(changes: SimpleChanges) {
		if (this.isEmptyObject(this.chartData)) {
			this.initChart(this.emptyChart);
			this.chart.axis.labels({
				y: '',
				y2: '',
				x: ''
			});
		}
		if (changes.chartData && (changes.chartData.currentValue as c3.Data).x) {
			this.loadChart();
		}
	}

	private indicateLatestTime(ltd: IntradayLatestTimeData) {
		if (ltd && this.chart) {
			if (!ltd.StartTime) return;
			const end = moment(ltd.StartTime);
			const time = moment()
				.hour(end.hour())
				.minute(end.minute());

			this.chart.xgrids([
				{
					value: time.format('YYYY-MM-DD HH:mm'),
					text: this.translate.instant('Latest data'),
					class: 'time-line'
				}
			]);
		} else {
			this.chart.xgrids([]);
		}
	}

	private loadChart() {
		if (this.chart) {
			switch (this.chartType as IntradayChartType) {
				case 'traffic':
					this.initChart(this.chartData, true);
					break;
				case 'performance':
					this.initChart(this.chartData, true);
					break;
				case 'staffing':
					this.initChart(this.chartData, false);
					break;
			}
		} else {
			this.initChart(this.chartData);
		}
		this.indicateLatestTime(this.latestTime);
	}

	private isEmptyObject(data: any) {
		return Object.keys(data).length === 0 && data.constructor === Object;
	}

	toggleData(id) {
		if (this.hiddenArray.indexOf(id) > -1) {
			this.hiddenArray.splice(this.hiddenArray.indexOf(id), 1);
		} else {
			this.hiddenArray.push(id);
		}
		this.chart.show();
		this.chart.hide(this.hiddenArray);
	}

	initChart(inData: c3.Data, show_y2: boolean = true) {
		if (this.chart) {
			this.chart.destroy();
		}
		if (angular.isDefined(inData) && inData.columns) {
			const chartObject: c3.ChartConfiguration = {
				bindto: '#chart',
				data: inData,
				axis: {
					x: {
						label: {
							text: this.translate.instant('SkillTypeTime'),
							position: 'outer-center'
						},
						type: 'timeseries',
						localtime: true,
						tick: {
							culling: {
								max: 24
							},
							fit: true,
							centered: true,
							multiline: false,
							format: '%H:%M'
						}
					},
					y: {
						label: {
							text: this.axisLabels.yLabel,
							position: 'outer-middle'
						},
						tick: {
							format: d3.format('.1f')
						}
					},
					y2: show_y2
						? {
								label: {
									text: this.axisLabels.y2Label,
									position: 'outer-middle'
								},
								show: true,
								tick: {
									format: d3.format('.1f')
								}
						  }
						: undefined
				},
				legend: {
					item: {
						onclick: id => {
							this.toggleData(id);
						}
					}
				},
				transition: {
					duration: 400
				}
			};
			this.chart = c3.generate(chartObject);
		}
	}
}
