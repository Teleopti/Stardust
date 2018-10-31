import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite } from '@wfm/test';
import { NzButtonModule, NzFormModule, NzGridModule, NzTableModule, NzToolTipModule } from 'ng-zorro-antd';
import { LogonInfoService } from '../../../shared/services';
import { MockTitleBarModule, WorkspaceComponent } from '../../components';
import { fakeBackendProvider } from '../../mocks';
import { NavigationService, SearchService, WorkspaceService } from '../../shared';
import { IdentityLogonPageComponent } from './identity-logon-page.component';

describe('IdentityLogonPageComponent', () => {
	let component: IdentityLogonPageComponent;
	let fixture: ComponentFixture<IdentityLogonPageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [IdentityLogonPageComponent, WorkspaceComponent],
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
			providers: [fakeBackendProvider, WorkspaceService, NavigationService, LogonInfoService, SearchService]
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
