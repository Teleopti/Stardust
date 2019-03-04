import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import c3 from 'c3';
import * as moment from 'moment';
import { IntradayHelper } from '../../shared';

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
	
	constructor(private translate: TranslateService) {
	}

	private intervalDetailsToC3Data(day): c3.Data {
		const intervalDetails = day.IntervalDetails;
		if (intervalDetails && intervalDetails.length > 0) {
			const timeStamps = [];
			const forecastedAgents = [];
			const scheduledAgents = [];
			const overStaffing = [];
			const staffingScaffold = [];
			const underStaffing = [];
			const criticalUnderStaffing = [];

			intervalDetails.forEach(interval => {
				timeStamps.push(interval.x);
				forecastedAgents.push(interval.f);
				scheduledAgents.push(interval.s);
				const diff = interval.s - interval.f;
				staffingScaffold.push(diff > 0 ? interval.f : interval.s);
				overStaffing.push(diff > 0 ? diff : 0);
				if (day.hasCritical && IntradayHelper.isCritical(interval, day.RelativeDifference)) {
					criticalUnderStaffing.push(diff < 0 ? -diff : 0);
					underStaffing.push(0);
				} else {
					criticalUnderStaffing.push(0);
					underStaffing.push(diff < 0 ? -diff : 0);
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
					Forecasted: 'line',
					Scheduled: 'line'
				},
				colors: {
					StaffingScaffold: '#FFFFFF',
					Overstaffing: '#0a84d6',
					Understaffing: '#D32F2F',
					CriticalInterval: '#FF0000',
				},
				names: {
					Forecasted: this.translate.instant('ForecastedAgents'),
					Scheduled: this.translate.instant('ScheduledAgents'),
					Overstaffing: this.translate.instant('Overstaffing'),
					Understaffing: this.translate.instant('Understaffing'),
					CriticalInterval: this.translate.instant('CriticalInterval'),
				},
				groups: [['StaffingScaffold', 'Overstaffing', 'Understaffing', 'CriticalInterval']]
			};
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

	private tooltip_contents(data, defaultTitleFormat, defaultValueFormat, color): string {
		const root: any = this;
		const config = root.config;
		const CLASS = root.CLASS;
		const titleFormat = config.tooltip_format_title || defaultTitleFormat;
		const nameFormat =
			config.tooltip_format_name ||
			function(name) {
				return name;
			};
		const valueFormat = config.tooltip_format_value || defaultValueFormat;
		let text: string = null;

		let criticalInterval: number = 0;
		for (let d of data) {
			if (d.id === 'CriticalInterval') {
				if (d.value !== 0) {
					criticalInterval = d.value;
				}
			}
		}

		const scheduled = data.filter(d => d.id === 'Scheduled')[0];
		const forecasted = data.filter(d => d.id === 'Forecasted')[0];
		const relativeDifferenceInterval =(scheduled.value-forecasted.value)/forecasted.value;
		for (let d of data) {
			if (
				d.id === 'StaffingScaffold' ||
				d.id === 'CriticalInterval' ||
				(d.id === 'Overstaffing' && d.value === 0)
			) {
				continue;
			}

			if (d.id === 'Understaffing') {
				if (d.value === 0) {
					if (criticalInterval !== 0) {
						d.value = criticalInterval;
					} else {
						continue;
					}
				}
			}

			if (!text) {
				let title = titleFormat(config.axis_x_categories[d.index]);
				text =
					"<table class='" +
					CLASS.tooltip +
					"'>" +
					(title ? "<tr><th colspan='2'>" + title + '</th></tr>' : '');
			}

			let name = nameFormat(d.name);
			let value = valueFormat(d.value, d.ratio, d.id, d.index);
			let bgcolor = root.levelColor ? root.levelColor(d.value) : color(d.id);

			text += "<tr class='" + CLASS.tooltipName + '-' + d.id + "'>";
			text += "<td class='name'><span style='background-color:" + bgcolor + "'></span>" + name + '</td>';
			text += "<td class='value'>" + value + '</td>';
			text += '</tr>';
			if (d.id === 'Understaffing' || d.id === 'Overstaffing') {
				text += "<tr>";
				text += "<td class='name'"+(criticalInterval!==0?"style='color:#FF0000'":'')+"><span style='background-color:" + bgcolor + "'></span>" + 'Relative Understaffing' + '</td>';
				text += "<td class='value'"+(criticalInterval!==0?"style='color:#FF0000'":'')+">" + ' '+(Math.abs(relativeDifferenceInterval)*100).toFixed(1)+'% '+'</td>';
				text += '</tr>';
			}
		}
		return text + '</table>';
	}

	private initChart(inData: c3.Data) {
		if (this.chart) {
			this.chart.destroy();
		}
		if (angular.isDefined(inData) && inData.columns) {
			const chartObject: c3.ChartConfiguration = {
				bindto: '#chart',
				data: inData,
				tooltip: {
					contents: this.tooltip_contents
				},
				point: {
					show: false
				},
				legend: {
					hide: ['StaffingScaffold', 'RelativeDifferenceInterval']
				},
				axis: {
					x: {
						type: 'category',
						tick: {
							culling: {
								max: 24
							},
							fit: true,
							centered: true,
							multiline: false
						}
					},
					y: {
						tick: {
							format: d3.format('.1f')
						}
					}
				}
			};
			this.chart = c3.generate(chartObject);
		}
	}
}
