import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, MockComponent, PageObject } from '@wfm/test';
import { NzButtonModule, NzFormModule, NzInputModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { adina, eva, fakeBackendProvider, myles } from '../../mocks';
import {
	COLUMNS,
	DIRECTION,
	NavigationService,
	PeopleSearchQuery,
	PeopleSearchResult,
	RolesService,
	SearchOverridesService,
	SearchService,
	WorkspaceService
} from '../../shared';
import { SearchPageComponent } from './search-page.component';

describe('SearchPageComponent', () => {
	let component: SearchPageComponent;
	let fixture: ComponentFixture<SearchPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				SearchPageComponent,
				MockComponent({ selector: 'people-workspace' }),
				MockComponent({ selector: 'people-title-bar' })
			],
			imports: [
				MockTranslationModule,
				HttpClientModule,
				ReactiveFormsModule,
				NoopAnimationsModule,
				NzFormModule,
				NzButtonModule,
				NzTableModule,
				NzInputModule,
				NzToolTipModule
			],
			providers: [
				fakeBackendProvider,
				WorkspaceService,
				SearchService,
				SearchOverridesService,
				{ provide: NavigationService, useValue: {} },
				RolesService
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(SearchPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display search results', async(() => {
		component.searchPeople();

		fixture.detectChanges();

		fixture.whenStable().then(() => {
			expect(page.resultRows.length).toBeGreaterThan(0);
		});
	}));

	it('should be able to select people', async(() => {
		component.searchPeople();

		fixture.detectChanges();

		fixture.whenStable().then(() => {
			expect(page.resultRows.length).toBeGreaterThan(0);
			page.resultRows[0].nativeElement.click();
			component.workspaceService.getSelectedPeople().subscribe({
				next: people => {
					expect(people.length).toEqual(1);
				}
			});
		});
	}));

	it('should be able to select all people', async(() => {
		component.searchPeople();

		fixture.detectChanges();

		fixture.whenStable().then(() => {
			expect(page.resultRows.length).toBeGreaterThan(0);
			page.selectAllCheckbox.nativeElement.click();
			component.workspaceService.getSelectedPeople().subscribe({
				next: people => {
					expect(people.length).toEqual(3);
				}
			});
		});
	}));

	it('should be able to sort on first name', async(() => {
		const searchResultStubBefore: PeopleSearchResult = {
			People: [eva, adina, myles],
			TotalRows: 3
		};

		const searchResultStubAfter: PeopleSearchResult = {
			People: [adina, eva, myles],
			TotalRows: 3
		};

		const searchSpy = spyOn(component.searchService, 'searchPeople');

		searchSpy.and.returnValues(of(searchResultStubBefore), of(searchResultStubAfter));

		component.searchPeople();
		fixture.detectChanges();

		page.sortOnFirstName.nativeElement.click();

		fixture.whenStable().then(() => {
			let searchQuery = searchSpy.calls.argsFor(0)[0] as PeopleSearchQuery;
			expect(searchQuery.sortColumn).toEqual(COLUMNS.LastName);
			expect(searchQuery.direction).toEqual(DIRECTION.asc);

			searchQuery = searchSpy.calls.argsFor(1)[0] as PeopleSearchQuery;
			expect(searchQuery.sortColumn).toEqual(COLUMNS.FirstName);
			expect(searchQuery.direction).toEqual(DIRECTION.asc);

			const adinaRow = page.resultRowsFirstName[0].nativeElement;
			expect(adinaRow.textContent).toContain(adina.FirstName);
		});
	}));
});

class Page extends PageObject {
	get resultRows() {
		return this.queryAll('[data-test-search] [data-test-person]');
	}

	get resultRowsFirstName() {
		return this.queryAll('[data-test-search] [data-test-person] [data-test-person-firstname]');
	}

	get selectAllCheckbox() {
		return this.queryAll('[data-test-search] [data-test-selectall-toggle] input')[0];
	}

	get sortOnFirstName() {
		return this.queryAll('[data-test-sort-fn]')[0];
	}
}
