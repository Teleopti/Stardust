import { Component, OnInit, OnDestroy } from '@angular/core';
import { NavigationService, WorkspaceService, AppLogonService } from '../../services';
import { MatTableDataSource } from '@angular/material';
import { Person } from '../../types';
import { Subject } from 'rxjs';

@Component({
	selector: 'people-app-logon-page',
	templateUrl: './app-logon-page.component.html',
	styleUrls: ['./app-logon-page.component.scss']
})
export class AppLogonPageComponent implements OnInit, OnDestroy {
	constructor(
		public nav: NavigationService,
		public workspaceService: WorkspaceService,
		public appLogonService: AppLogonService
	) {}

	private componentDestroyed: Subject<any> = new Subject();

	displayedColumns = ['Name', 'ApplicationLogon'];
	dataSource = new MatTableDataSource<Person>([]);

	ngOnInit() {
		this.workspaceService.people$.takeUntil(this.componentDestroyed).subscribe({
			next: (people: Person[]) => {
				this.dataSource.data = people;
				if (people.length === 0) this.nav.navToSearch();
			}
		});
	}

	ngOnDestroy() {
		this.componentDestroyed.next();
		this.componentDestroyed.complete();
	}

	isValid(): boolean {
		return true;
	}

	getName(person: Person): string {
		return `${person.FirstName} ${person.LastName}`;
	}

	save(): void {}
}
