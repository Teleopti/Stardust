import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import c3 from 'c3';
import * as moment from 'moment';

@Component({
	selector: 'plans-intraday',
	templateUrl: './intraday.html'
})
export class IntradayComponent implements OnChanges {
	chart: c3.ChartAPI;
	hiddenArray: string[] = [];

	constructor(private translate: TranslateService) {}

	@Input()
	chartData: c3.Data;

	emptyChart = {
		x: 'x',
		type: 'area-spline',
		columns: [],
		empty: { label: { text: this.translate.instant('NoDataAvailable') } }
	};

	private intervalDetailsToC3Data(input): c3.Data {
		if(input && input.length>0){
			const timeStamps = input.map(item => {
				return moment.utc(item.x, "HH:mm");
			});
			const forecastedAgents = input.map(item => {
				return item.f;
			});
			const scheduledAgents = input.map(item => {
				return item.s;
			});
			return {
				x: 'x',
				xFormat: '%H:%M',
				columns: [
					['x'].concat(timeStamps),
					['Forecasted'].concat(forecastedAgents),
					['Scheduled'].concat(scheduledAgents),
				],
				type: 'area-spline',
				colors: {
					Forecasted: '#99D6FF',
					Scheduled: '#0099FF',
				},
				names: {
					Forecasted: this.translate.instant('ForecastedAgents'),
					Scheduled: this.translate.instant('ScheduledAgents'),
				},
				axes: {
					Forecasted: 'y',
					Scheduled: 'y',
				}
			}
		} else {
			return {
				x: 'x',
				columns: [],
				type: 'area-spline',
				empty: { label: { text: this.translate.instant('NoDataAvailable') } }
			};
		}
	}
	
	ngOnChanges(changes: SimpleChanges) {
		if (changes.chartData) {
			this.initChart(this.intervalDetailsToC3Data(changes.chartData.currentValue));
		}
	}

	initChart(inData: c3.Data) {
		if (this.chart) {
			this.chart.destroy();
		}
		if (angular.isDefined(inData) && inData.columns) {
			const chartObject: c3.ChartConfiguration = {
				bindto: '#chart',
				data: inData,
				axis: {
					x: {
						type: 'timeseries',
						localtime: false,
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
							text: this.translate.instant('Agents'),
							position: 'outer-middle'
						},
						tick: {
							format: d3.format('.1f')
						}
					},
				},
				transition: {
					duration: 100
				}
			};
			this.chart = c3.generate(chartObject);
		}
	}
}
