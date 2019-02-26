import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import c3 from 'c3';
import * as moment from 'moment';

@Component({
	selector: 'plans-intraday',
	templateUrl: './intraday.component.html',
	styleUrls: ['./intraday.component.scss']
})
export class IntradayComponent implements OnChanges {
	chart: c3.ChartAPI;
	date;

	@Input()
	chartData;
	@Input()
	skill: string;
	
	constructor(private translate: TranslateService) {}


	private intervalDetailsToC3Data(input): c3.Data {
		if(input && input.length>0){
			
			let timeStamps = [];
			let forecastedAgents = [];
			let scheduledAgents = [];
			let overStaffing = [];
			let staffingScaffold = [];
			let underStaffing = [];
			input.forEach(item => {
				timeStamps.push(moment.utc(item.x, "HH:mm"));
				forecastedAgents.push(item.f);
				scheduledAgents.push(item.s);
				let diff = item.s-item.f;
				staffingScaffold.push(diff>0?item.f:item.s);
				overStaffing.push(diff>0?diff:0);
				underStaffing.push(diff<0?-diff:0);
			});
			
			return {
				x: 'x',
				columns: [
					['x'].concat(timeStamps),
					['Forecasted'].concat(forecastedAgents),
					['Scheduled'].concat(scheduledAgents),
					['StaffingScaffold'].concat(staffingScaffold),
					['OverStaffing'].concat(overStaffing),
					['UnderStaffing'].concat(underStaffing)
				],
				order: 'null',
				type: 'bar',
				types: {
					'Forecasted': "line",
					'Scheduled': "line"
				},
				colors: {
					'StaffingScaffold': '#FFFFFF',
					'OverStaffing': '#0a84d6',
					'UnderStaffing': '#D32F2F'
				},
				names: {
					Forecasted: this.translate.instant('ForecastedAgents'),
					Scheduled: this.translate.instant('ScheduledAgents'),
				},
				groups: [['StaffingScaffold', 'OverStaffing', 'UnderStaffing']]
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
			this.date = moment(changes.chartData.currentValue.Date);
			this.initChart(this.intervalDetailsToC3Data(changes.chartData.currentValue.IntervalDetails));
		}
	}


	private initChart(inData: c3.Data) {
		if (this.chart) {
			this.chart.destroy();
		}
		if (angular.isDefined(inData) && inData.columns) {
			const chartObject: c3.ChartConfiguration = {
				bindto: '#chart',
				data: inData,
				point: {
					show: false
				},
				legend: {
					hide: ['StaffingScaffold']
				},
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
						tick: {
							format: d3.format('.1f')
						}
					},
				}
			};
			this.chart = c3.generate(chartObject);
		}
	}
}
