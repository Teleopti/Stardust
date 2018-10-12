import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import {
	NzButtonModule,
	NzCheckboxModule,
	NzDividerModule,
	NzFormModule,
	NzTableModule,
	NzToolTipModule
} from 'ng-zorro-antd';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import { MockTitleBarModule, WorkspaceComponent } from '../../components';
import { adina, eva, fakeBackendProvider, myles } from '../../mocks';
import { NavigationService, RolesService, SearchOverridesService, SearchService, WorkspaceService } from '../../shared';
import { countUniqueRolesFromPeople } from '../../utils';
import { GrantPageComponent } from './grant-page.component';
import { GrantPageService } from './grant-page.service';

describe('GrantPageComponent', () => {
	let component: GrantPageComponent;
	let fixture: ComponentFixture<GrantPageComponent>;
	let page: Page;
	let workspaceService: WorkspaceService;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [GrantPageComponent, WorkspaceComponent],
			imports: [
				MockTitleBarModule,
				MockTranslationModule,
				HttpClientModule,
				NzCheckboxModule,
				NzFormModule,
				FormsModule,
				NzDividerModule,
				NzButtonModule,
				NzTableModule,
				NzToolTipModule
			],
			providers: [
				GrantPageService,
				RolesService,
				fakeBackendProvider,
				WorkspaceService,
				SearchService,
				SearchOverridesService,
				NavigationService
			]
		}).compileComponents();
	}));

	beforeEach(async(async () => {
		fixture = TestBed.createComponent(GrantPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		workspaceService = fixture.debugElement.injector.get(WorkspaceService);
	}));

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should show current roles', () => {
		workspaceService.selectPeople([adina, myles, eva]);
		fixture.detectChanges();
		expect(page.currentChips.length).toEqual(countUniqueRolesFromPeople([adina, myles, eva]));
	});
});

class Page {
	get currentChips() {
		return this.queryAll('[data-test-grant-current] [data-test-chip]');
	}

	get availableChips() {
		return this.queryAll('[data-test-grant-available] [data-test-chip]');
	}

	fixture: ComponentFixture<GrantPageComponent>;

	constructor(fixture: ComponentFixture<GrantPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
