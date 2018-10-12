import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { NzTableModule } from '../../../../../node_modules/ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { ExternalApplicationService, fakeBackendProvider, NavigationService } from '../../services';
import { ListPageComponent } from './list-page.component';

describe('ListPageComponent', () => {
	let component: ListPageComponent;
	let fixture: ComponentFixture<ListPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [ListPageComponent],
			imports: [HttpClientModule, NzTableModule],
			providers: [fakeBackendProvider, ExternalApplicationService, { provide: NavigationService, useValue: {} }]
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
		fixture.whenStable().then(() => {
			expect(page.resultRows.length).toBeGreaterThan(0);
		});
	}));
});

class Page {
	get resultRows() {
		return this.queryAll('[data-test-app-item]');
	}

	fixture: ComponentFixture<ListPageComponent>;

	constructor(fixture: ComponentFixture<ListPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
