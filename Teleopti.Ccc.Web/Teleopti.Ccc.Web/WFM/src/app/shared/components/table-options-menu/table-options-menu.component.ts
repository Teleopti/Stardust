import { Component, Input } from '@angular/core';
import FileSaver from 'file-saver';
import { NzMessageService } from 'ng-zorro-antd';
import Papa from 'papaparse';
/*
	Part of working around https://github.com/angular/angular/issues/14761
 */
export function toBoolean(value: any): boolean {
	return value != null && `${value}` !== 'false';
}

@Component({
	selector: 'telop-table-options-menu',
	templateUrl: './table-options-menu.component.html',
	styleUrls: ['./table-options-menu.component.scss'],
	providers: []
})
export class TableOptionsMenuComponent {
	private _enableCsvExport = false;

	constructor(private message: NzMessageService) {}

	@Input()
	data: any[];

	@Input()
	set enableCsvExport(value: boolean) {
		this._enableCsvExport = toBoolean(value);
	}

	get enableCsvExport() {
		return this._enableCsvExport;
	}

	exportToCsv() {
		if (!this.enableCsvExport) return;
		if (this.data.length === 0) return this.message.warning('No data to export');
		console.log('export', this.data);
		const csvString = Papa.unparse(this.data);
		const blob = new Blob([csvString], { type: 'text/csv;charset=utf-8' });
		FileSaver.saveAs(blob, 'export.csv');
	}
}
