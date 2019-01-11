import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, MockComponent } from '@wfm/test';
import { NzButtonModule, NzFormModule, NzGridModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { LogonInfoService } from '../../../shared/services';
import { fakeBackendProvider } from '../../mocks';
import { NavigationService, SearchService, WorkspaceService } from '../../shared';
import { IdentityLogonPageComponent } from './identity-logon-page.component';

describe('IdentityLogonPageComponent', () => {
	let component: IdentityLogonPageComponent;
	let fixture: ComponentFixture<IdentityLogonPageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				IdentityLogonPageComponent,
				MockComponent({ selector: 'people-workspace' }),
				MockComponent({ selector: 'people-title-bar' })
			],
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
			providers: [
				fakeBackendProvider,
				WorkspaceService,
				{ provide: NavigationService, useValue: {} },
				LogonInfoService,
				SearchService
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(IdentityLogonPageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
