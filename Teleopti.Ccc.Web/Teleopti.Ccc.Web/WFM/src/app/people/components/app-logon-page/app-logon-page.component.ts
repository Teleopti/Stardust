import { Component, OnInit } from '@angular/core';
import { NavigationService, WorkspaceService, AppLogonService } from '../../services';
import { MatTableDataSource } from '@angular/material';
import { Person } from '../../types';
import { FormControl, Validators, FormGroup, FormBuilder } from '@angular/forms';

@Component({
	selector: 'people-app-logon-page',
	templateUrl: './app-logon-page.component.html',
	styleUrls: ['./app-logon-page.component.scss']
})
export class AppLogonPageComponent implements OnInit {
	constructor(
		public nav: NavigationService,
		public workspaceService: WorkspaceService,
		public appLogonService: AppLogonService
	) {}

	displayedColumns = ['Name', 'ApplicationLogon'];
	dataSource = new MatTableDataSource<Person>([]);

	ngOnInit() {
		this.workspaceService.getSelectedPeople().subscribe({
			next: (people: Person[]) => {
				this.dataSource.data = people;
				if (people.length === 0) this.nav.navToSearch();
			}
		});
	}

	isValid(): boolean {
		return true;
	}

	getName(person: Person): string {
		return `${person.FirstName} ${person.LastName}`;
	}

	save(): void {}
}
