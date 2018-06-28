import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { configureTestSuite } from '../../../../configure-test-suit';
import { PeopleTestModule } from '../../people.test.module';
import {
	adina,
	WorkspaceService,
	NavigationService,
	fakeBackendProvider,
	LogonInfoService,
	SearchService
} from '../../services';
import { adinaLogon } from '../../services/fake-backend/logons';
import { AppLogonPageComponent } from './app-logon-page.component';
import { MockTranslationModule } from '../../../../mocks/translation';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material';
import { PageContainerComponent, WorkspaceComponent } from '..';
import { HttpClientModule } from '@angular/common/http';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('AppLogonPageComponent', () => {
	let component: AppLogonPageComponent;
	let fixture: ComponentFixture<AppLogonPageComponent>;
	let workspaceService: WorkspaceService;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [AppLogonPageComponent, PageContainerComponent, WorkspaceComponent],
			imports: [
				MockTranslationModule,
				ReactiveFormsModule,
				MatInputModule,
				HttpClientModule,
				NoopAnimationsModule
			],
			providers: [fakeBackendProvider, WorkspaceService, NavigationService, LogonInfoService, SearchService]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(AppLogonPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		workspaceService = fixture.debugElement.injector.get(WorkspaceService);

		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display person app logon', () => {
		workspaceService.selectPerson(adina);
		fixture.detectChanges();
		let input: HTMLInputElement = page.logonFields[0].nativeElement;
		expect(input.value).toEqual(adinaLogon.LogonName);
	});
});

class Page {
	get logonFields() {
		return this.queryAll('[data-test-application-logon] [data-test-person-logon]');
	}

	fixture: ComponentFixture<AppLogonPageComponent>;

	constructor(fixture: ComponentFixture<AppLogonPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
