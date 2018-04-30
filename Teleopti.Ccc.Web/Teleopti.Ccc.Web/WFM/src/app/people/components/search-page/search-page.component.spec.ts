import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { PeopleTestModule } from '../../people.test.module';
import { fakeBackendProvider } from '../../services';
import { SearchPageComponent } from './search-page.component';

describe('SearchPageComponent', () => {
	let component: SearchPageComponent;
	let fixture: ComponentFixture<SearchPageComponent>;
	let page: Page;

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [PeopleTestModule, HttpClientModule],
			providers: [fakeBackendProvider]
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

		fixture.whenStable().then(() => {
			expect(page.resultRows.length).toBeGreaterThan(0);
		});
	}));

	it('should be able to select people', async(() => {
		component.searchPeople();

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
});

class Page {
	get resultRows() {
		return this.queryAll('[data-test-search] [data-test-person]');
	}

	fixture: ComponentFixture<SearchPageComponent>;

	constructor(fixture: ComponentFixture<SearchPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
