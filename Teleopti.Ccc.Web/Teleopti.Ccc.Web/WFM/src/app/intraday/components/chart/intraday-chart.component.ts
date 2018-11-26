import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import c3 from 'c3';
import { IntradayChartType } from '../../types/intraday-chart-type';

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

	ngOnChanges(changes: SimpleChanges) {
		if (changes.chartData) {
			this.loadChart();
		}
	}

	private loadChart() {
		if (this.chart) {
			switch (this.chartType as IntradayChartType) {
				case 'traffic':
					this.loadTrafficChart(this.chartData);
					break;
				case 'performance':
					this.loadPerformanceChart(this.chartData);
					break;
				case 'staffing':
					this.loadStaffingChart(this.chartData);
					break;
			}
		} else {
			this.initChart(this.chartData);
		}
	}

	private loadTrafficChart(value: c3.Data) {
		if (value.columns && value.columns.slice(1).some(item => item.length > 0)) {
			if (!this.chart.axis) {
				this.initChart(this.chartData);
			}
			this.chart.axis.labels({
				y: this.translate.instant('Volume'),
				y2: this.translate.instant('AverageHandlingTime')
			});
			if (value.axes) {
				this.chart.load({
					columns: value.columns,
					axes: value.axes,
					names: value.names,
					unload: true
				});
			} else {
				this.chart.load({
					columns: value.columns
				});
			}
		} else {
			this.chart.load({ unload: true });
		}
	}

	private loadPerformanceChart(value: c3.Data) {
		if (value.columns && value.columns.slice(1).some(item => item.length > 0)) {
			if (!this.chart.axis) {
				this.initChart(this.chartData);
			}
			this.chart.axis.labels({
				y: this.translate.instant('SecondShort'),
				y2: this.translate.instant('%')
			});
			if (value.axes) {
				this.chart.load({
					columns: value.columns,
					axes: value.axes,
					names: value.names,
					unload: true
				});
			} else {
				this.chart.load({
					columns: value.columns
				});
			}
		} else {
			this.chart.load({ unload: true });
		}
	}

	private loadStaffingChart(value: c3.Data) {
		if (value.columns && value.columns.slice(1).some(item => item.length > 0)) {
			if (!this.chart.axis) {
				this.initChart(this.chartData);
			}
			this.chart.axis.labels({
				y: this.translate.instant('Agents'),
				y2: null
			});
			if (value.axes) {
				this.chart.load({
					columns: value.columns,
					axes: value.axes,
					names: value.names,
					unload: true
				});
			} else {
				this.chart.load({
					columns: value.columns
				});
			}
		} else {
			this.chart.load({ unload: true });
		}
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

	initChart(inData: c3.Data) {
		if (angular.isDefined(inData)) {
			const chartObject = {
				bindto: '#chart',
				data: inData,
				axis: {
					x: {
						label: {
							text: this.translate.instant('SkillTypeTime'),
							position: 'outer-center'
						},
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
						label: {
							text: 'y',
							position: 'outer-middle'
						},
						tick: {
							format: d3.format('.1f')
						}
					},
					y2: {
						label: {
							text: 'y2',
							position: 'outer-middle'
						},
						show: true,
						tick: {
							format: d3.format('.1f')
						}
					}
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
