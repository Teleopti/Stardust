import { Component, OnInit } from '@angular/core';
import { MatTableDataSource, PageEvent } from '@angular/material';
import { SelectionModel } from '@angular/cdk/collections';

import {
	ExternalApplicationService,
	NavigationService
} from '../../services';
import { ExternalApplication } from '../../types';
import { ListPageService } from './list-page.service';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';

@Component({
	selector: 'api-access-list-page',
	templateUrl: './list-page.component.html',
	styleUrls: ['./list-page.component.scss'],
	providers: [ListPageService, NavigationService]
})
export class ListPageComponent implements OnInit {
	constructor(
		public listPageService: ListPageService,
		public nav: NavigationService
	) {}

	displayedColumns = ['select', 'Id', 'Name'];
	dataSource = new MatTableDataSource<ExternalApplication>([]);
	selection = new SelectionModel<ExternalApplication>(true, []);
	
	ngOnInit() {
		this.reloadList();
	}

	reloadList() {
		this.listPageService.listExternalApplications().subscribe({
			next: (apps: ExternalApplication[]) => {
				this.dataSource.data = apps;
			}
		});
	}

	/** Whether the number of selected elements matches the total number of rows. */
	isAllSelected() {
		const numSelected = this.selection.selected.length;
		const numRows = this.dataSource.data.length;
		return numSelected === numRows;
	}

	/** Selects all rows if they are not all selected; otherwise clear selection. */
	masterToggle() {
		this.isAllSelected() ?
			this.selection.clear() :
			this.dataSource.data.forEach(row => this.selection.select(row));
	}

	revokeSelected() {
		let items = this.selection.selected.slice();
		items.forEach(app => {
			this.listPageService.revokeApiAccess(app.Id).subscribe({ next: _ => { this.reloadList(); } });
		});
	}
}
