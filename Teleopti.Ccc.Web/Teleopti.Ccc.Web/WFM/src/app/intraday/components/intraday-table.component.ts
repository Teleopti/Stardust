import { Component, OnInit, Input } from '@angular/core';
import { IntradayTrafficSummaryItem, IntradayPerformanceSummaryItem, IntradayChartType } from '../types';

@Component({
	selector: 'intraday-table',
	templateUrl: './intraday-table.component.html'
})
export class IntradayTableComponent {
	constructor() {}

	@Input()
	chartType: IntradayChartType;

	@Input()
	tableData: IntradayTrafficSummaryItem[] | IntradayPerformanceSummaryItem[];
}
