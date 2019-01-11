import { HttpClientModule } from '@angular/common/http';
import { Component } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzButtonModule, NzFormModule, NzGridModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { BehaviorSubject } from 'rxjs';
import { LogonInfoService } from '../../../shared/services';
import { MockTitleBarModule } from '../../components';
import { adina, adinaLogon, fakeBackendProvider } from '../../mocks';
import { NavigationService, SearchService, WorkspaceService } from '../../shared';
import { AppLogonPageComponent } from './app-logon-page.component';

@Component({
	selector: 'people-workspace',
	template: `
		MockWorkspace
	`
})
class MockWorkspaceComponent {}

class MockWorkspaceService implements Partial<WorkspaceService> {
	people$ = new BehaviorSubject([adina]);
}

describe('AppLogonPageComponent', () => {
	let component: AppLogonPageComponent;
	let fixture: ComponentFixture<AppLogonPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [AppLogonPageComponent, MockWorkspaceComponent],
			imports: [
				MockTitleBarModule,
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
			providers: [
				fakeBackendProvider,
				{ provide: WorkspaceService, useClass: MockWorkspaceService },
				{ provide: NavigationService, useValue: {} },
				LogonInfoService,
				SearchService
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(AppLogonPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display person app logon', () => {
		fixture.detectChanges();
		const input: HTMLInputElement = page.logonFields[0].nativeElement;
		expect(input.value).toEqual(adinaLogon.LogonName);
	});
});

class Page extends PageObject {
	get logonFields() {
		return this.queryAll('[data-test-application-logon] [data-test-person-logon]');
	}
}
