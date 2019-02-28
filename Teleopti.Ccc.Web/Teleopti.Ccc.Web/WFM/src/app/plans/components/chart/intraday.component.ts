import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import c3 from 'c3';
import * as moment from 'moment';
import {IntradayHelper} from "../../shared";

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


	private intervalDetailsToC3Data(day): c3.Data {
		let intervalDetails = day.IntervalDetails;
		if(intervalDetails && intervalDetails.length>0){
			let timeStamps = [];
			let forecastedAgents = [];
			let scheduledAgents = [];
			let overStaffing = [];
			let staffingScaffold = [];
			let underStaffing = [];
			let criticalUnderStaffing = [];

			intervalDetails.forEach(interval => {
				timeStamps.push(moment.utc(interval.x, "HH:mm"));
				forecastedAgents.push(interval.f);
				scheduledAgents.push(interval.s);
				let diff = interval.s-interval.f;
				staffingScaffold.push(diff>0?interval.f:interval.s);
				overStaffing.push(diff>0?diff:0);
				if(IntradayHelper.isCritical(interval, day.average, day.RelativeDifference)){
					criticalUnderStaffing.push(diff<0?-diff:0);
					underStaffing.push(0);
				}else{
					criticalUnderStaffing.push(0);
					underStaffing.push(diff<0?-diff:0);
				}
			});
			
			return {
				x: 'x',
				columns: [
					['x'].concat(timeStamps),
					['Forecasted'].concat(forecastedAgents),
					['Scheduled'].concat(scheduledAgents),
					['StaffingScaffold'].concat(staffingScaffold),
					['Overstaffing'].concat(overStaffing),
					['Understaffing'].concat(underStaffing),
					['CriticalInterval'].concat(criticalUnderStaffing),
				],
				order: 'null',
				type: 'bar',
				types: {
					'Forecasted': "line",
					'Scheduled': "line"
				},
				colors: {
					'StaffingScaffold': '#FFFFFF',
					'Overstaffing': '#0a84d6',
					'Understaffing': '#D32F2F',
					'CriticalInterval': '#FF0000',
				},
				names: {
					'Forecasted': this.translate.instant('ForecastedAgents'),
					'Scheduled': this.translate.instant('ScheduledAgents'),
					'Overstaffing': this.translate.instant('Overstaffing'),
					'Understaffing': this.translate.instant('Understaffing'),
					'CriticalInterval': this.translate.instant('CriticalInterval'),
				},
				groups: [['StaffingScaffold', 'Overstaffing', 'Understaffing', 'CriticalInterval']]
			}
		} else {
			return {
				x: 'x',
				columns: [],
				type: 'bar',
				empty: { label: { text: this.translate.instant('NoDataAvailable') } }
			};
		}
	}
	
	ngOnChanges(changes: SimpleChanges) {
		if (changes.chartData) {
			this.date = moment(changes.chartData.currentValue.Date);
			this.initChart(this.intervalDetailsToC3Data(changes.chartData.currentValue));
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
