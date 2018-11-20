import { Component, OnInit } from '@angular/core';
import { NavigationService } from '../../services';
import { ExternalApplication } from '../../types';
import { ListPageService } from './list-page.service';

interface ExternalApplicationItem extends ExternalApplication {
	checked: boolean;
}

@Component({
	selector: 'api-access-list-page',
	templateUrl: './list-page.component.html',
	styleUrls: ['./list-page.component.scss'],
	providers: [ListPageService]
})
export class ListPageComponent implements OnInit {
	constructor(public listPageService: ListPageService, public nav: NavigationService) {}

	data: ExternalApplicationItem[] = [];
	allChecked = false;
	indeterminate = false;

	ngOnInit() {
		this.reloadList();
	}

	reloadList() {
		this.listPageService.listExternalApplications().subscribe({
			next: (apps: ExternalApplication[]) => {
				this.data = apps.map(item => {
					return {
						...item,
						checked: false
					};
				});
			}
		});
	}

	refreshStatus(): void {
		const allChecked = this.data.every(value => value.checked === true);
		const allUnChecked = this.data.every(value => !value.checked);
		this.allChecked = allChecked;
		this.indeterminate = !allChecked && !allUnChecked;
	}

	checkAll(value: boolean): void {
		this.data.forEach(data => {
			data.checked = value;
		});
		this.refreshStatus();
	}

	getCheckedItems(): ExternalApplicationItem[] {
		return this.data.filter(item => item.checked);
	}

	revokeSelected() {
		const items = this.getCheckedItems();
		items.forEach(app => {
			this.listPageService.revokeApiAccess(app.Id).subscribe({
				next: _ => {
					this.reloadList();
				}
			});
		});
	}
}
