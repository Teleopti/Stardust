import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzTableModule } from '../../../../../node_modules/ng-zorro-antd';
import { ExternalApplicationService, fakeBackendProvider, NavigationService } from '../../services';
import { ListPageComponent } from './list-page.component';

class Page extends PageObject {
	get resultRows() {
		return this.queryAll('[data-test-app-item]');
	}
}

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
