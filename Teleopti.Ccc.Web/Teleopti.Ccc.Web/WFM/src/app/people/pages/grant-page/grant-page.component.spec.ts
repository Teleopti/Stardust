import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, MockComponent, PageObject } from '@wfm/test';
import {
	NzButtonModule,
	NzCheckboxModule,
	NzDividerModule,
	NzFormModule,
	NzTableModule,
	NzToolTipModule
} from 'ng-zorro-antd';
import { BehaviorSubject } from 'rxjs';
import { adina, eva, fakeBackendProvider, myles } from '../../mocks';
import { NavigationService, RolesService, SearchOverridesService, SearchService, WorkspaceService } from '../../shared';
import { countUniqueRolesFromPeople } from '../../utils';
import { GrantPageComponent } from './grant-page.component';
import { GrantPageService } from './grant-page.service';

class MockWorkspaceService implements Partial<WorkspaceService> {
	people$ = new BehaviorSubject([adina, myles, eva]);
}

describe('GrantPageComponent', () => {
	let component: GrantPageComponent;
	let fixture: ComponentFixture<GrantPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				GrantPageComponent,
				MockComponent({ selector: 'people-workspace' }),
				MockComponent({ selector: 'people-title-bar' })
			],
			imports: [
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
				{ provide: WorkspaceService, useClass: MockWorkspaceService },
				SearchService,
				SearchOverridesService,
				{ provide: NavigationService, useValue: {} }
			]
		}).compileComponents();
	}));

	beforeEach(async(async () => {
		fixture = TestBed.createComponent(GrantPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
	}));

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should show current roles', () => {
		fixture.detectChanges();
		const uniqueRoleCount = countUniqueRolesFromPeople([adina, myles, eva]);
		expect(page.currentChips.length).toEqual(uniqueRoleCount);
	});
});

class Page extends PageObject {
	get currentChips() {
		return this.queryAll('[data-test-grant-current] [data-test-chip]');
	}

	get availableChips() {
		return this.queryAll('[data-test-grant-available] [data-test-chip]');
	}
}
