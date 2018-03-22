import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchPageComponent } from './search-page.component';
import { PeopleModule } from '../../people.module';
import { HttpClientModule } from '@angular/common/http';
import { fakeBackendProvider } from '../../services';
import { By } from '@angular/platform-browser';

describe('SearchPageComponent', () => {
	let component: SearchPageComponent;
	let fixture: ComponentFixture<SearchPageComponent>;
	const SEARCH_PERSON_QUERY = '[data-test-search] [data-test-person]';
	const WORKSPACE_PERSON_QUERY = '[data-test-workspace] [data-test-person]';
	const WORKSPACE_PERSON_REMOVE = '[data-test-workspace] [data-test-person] [data-test-person-remove]';

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [PeopleModule, HttpClientModule],
				providers: [fakeBackendProvider]
			}).compileComponents();
		})
	);

	beforeEach(() => {
		fixture = TestBed.createComponent(SearchPageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it(
		'should display people in list',
		async(() => {
			fixture.detectChanges();

			component.searchPeople();

			fixture.whenStable().then(() => {
				fixture.detectChanges();

				expect(component.searchService.getPeople().getValue().length).toBeGreaterThan(0);

				let debugElements = fixture.debugElement.queryAll(By.css(SEARCH_PERSON_QUERY));
				expect(debugElements.length).toBeGreaterThan(0);
			});
		})
	);

	it(
		'selected people should be shown in workspace',
		async(() => {
			fixture.detectChanges();

			component.searchPeople();

			fixture.whenStable().then(() => {
				let debugElements;
				fixture.detectChanges();

				debugElements = fixture.debugElement.queryAll(By.css(SEARCH_PERSON_QUERY));
				debugElements[0].nativeElement.click();
				debugElements[1].nativeElement.click();
				fixture.detectChanges();
				debugElements = fixture.debugElement.queryAll(By.css(WORKSPACE_PERSON_QUERY));

				expect(debugElements.length).toEqual(2);

				// // Deleting person from workspace
				fixture.debugElement.query(By.css(WORKSPACE_PERSON_REMOVE)).nativeElement.click();
				fixture.detectChanges();
				debugElements = fixture.debugElement.queryAll(By.css(WORKSPACE_PERSON_QUERY));

				expect(debugElements.length).toEqual(1);
			});
		})
	);
});
