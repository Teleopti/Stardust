import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { configureTestSuite } from '../../../../configure-test-suit';
import { PeopleTestModule } from '../../people.test.module';
import {
	WorkspaceService,
	RolesService,
	SearchService,
	SearchOverridesService,
	NavigationService
} from '../../services';
import { adina, eva, myles, fakeBackendProvider } from '../../services/fake-backend';
import { countUniqueRolesFromPeople } from '../../utils';
import { GrantPageComponent } from './grant-page.component';
import { MockTranslationModule } from '../../../../mocks/translation';
import { ChipComponent, ChipAddComponent, PageContainerComponent } from '../shared';
import { MatDividerModule } from '@angular/material';
import { WorkspaceComponent } from '..';
import { GrantPageService } from './grant-page.service';
import { HttpClientModule } from '@angular/common/http';

describe('GrantPageComponent', () => {
	let component: GrantPageComponent;
	let fixture: ComponentFixture<GrantPageComponent>;
	let page: Page;
	let workspaceService: WorkspaceService;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				GrantPageComponent,
				ChipComponent,
				ChipAddComponent,
				PageContainerComponent,
				WorkspaceComponent
			],
			imports: [MockTranslationModule, MatDividerModule, HttpClientModule],
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
