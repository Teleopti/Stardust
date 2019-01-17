import { Component, OnInit, Input } from '@angular/core';
import { IntradayTrafficSummaryItem, IntradayPerformanceSummaryItem, IntradayChartType } from '../types';

@Component({
	selector: 'intraday-table',
	templateUrl: './intraday-table.component.html',
	styleUrls: ['./intraday-table.component.scss']
})
export class IntradayTableComponent {
	constructor() {}

	@Input()
	chartType: IntradayChartType;

	@Input()
	tableData: IntradayTrafficSummaryItem[] | IntradayPerformanceSummaryItem[];

	public get hideAbandonRate(): boolean {
		return (
			(this.tableData as IntradayPerformanceSummaryItem[])[0] &&
			(this.tableData as IntradayPerformanceSummaryItem[])[0].AbandonRate === undefined
		);
	}
}
