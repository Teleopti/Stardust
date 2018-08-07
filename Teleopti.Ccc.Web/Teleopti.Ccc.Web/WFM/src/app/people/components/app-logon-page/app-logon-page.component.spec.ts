import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NzButtonModule, NzFormModule, NzGridModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { WorkspaceComponent } from '..';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import {
	adina,
	fakeBackendProvider,
	LogonInfoService,
	NavigationService,
	SearchService,
	WorkspaceService
} from '../../services';
import { adinaLogon } from '../../services/fake-backend/logons';
import { AppLogonPageComponent } from './app-logon-page.component';

describe('AppLogonPageComponent', () => {
	let component: AppLogonPageComponent;
	let fixture: ComponentFixture<AppLogonPageComponent>;
	let workspaceService: WorkspaceService;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [AppLogonPageComponent, WorkspaceComponent],
			imports: [
				MockTranslationModule,
				ReactiveFormsModule,
				HttpClientModule,
				NoopAnimationsModule,
				NzFormModule,
				NzGridModule,
				NzTableModule,
				NzButtonModule,
				NzToolTipModule
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
