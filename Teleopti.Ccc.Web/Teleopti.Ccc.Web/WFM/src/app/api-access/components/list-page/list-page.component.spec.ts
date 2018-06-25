import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { configureTestSuite } from '../../../../configure-test-suit';
import { ApiAccessTestModule } from '../../api-access.test.module';
import {
	fakeBackendProvider
} from '../../services';
import { ListPageComponent } from './list-page.component';
import { asElementData } from '@angular/core/src/view';

describe('ListPageComponent', () => {
	let component: ListPageComponent;
	let fixture: ComponentFixture<ListPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [ApiAccessTestModule, HttpClientModule],
			providers: [fakeBackendProvider]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(ListPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display search results', async(() => {
		//component.searchPeople();

		fixture.whenStable().then(() => {
			expect(page.resultRows.length).toBeGreaterThan(0);
		});
	}));
});

class Page {
	get resultRows() {
		return this.queryAll('[data-test-search] [data-test-person]');
	}

	get resultRowsFirstName() {
		return this.queryAll('[data-test-search] [data-test-person] [data-test-person-firstname]');
	}

	get actionMenu() {
		return this.queryAll('[data-test-action-menu]')[0];
	}

	get selectAllCheckbox() {
		return this.queryAll('[data-test-search] [data-test-selectall-toggle] input')[0];
	}

	get selectAllOnAllPagesButton() {
		return this.queryAll('[data-test-selectallonallpages-button]')[0];
	}

	get sortOnFirstName() {
		return this.queryAll('[data-test-sort-fn]')[0];
	}

	fixture: ComponentFixture<ListPageComponent>;

	constructor(fixture: ComponentFixture<ListPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
