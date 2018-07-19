import { HttpClientModule } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NzButtonModule, NzFormModule, NzGridModule, NzTableModule } from 'ng-zorro-antd';
import { WorkspaceComponent } from '..';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import {
	fakeBackendProvider,
	LogonInfoService,
	NavigationService,
	SearchService,
	WorkspaceService
} from '../../services';
import { IdentityLogonPageComponent } from './identity-logon-page.component';

describe('IdentityLogonPageComponent', () => {
	let component: IdentityLogonPageComponent;
	let fixture: ComponentFixture<IdentityLogonPageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [IdentityLogonPageComponent, WorkspaceComponent],
			imports: [
				MockTranslationModule,
				ReactiveFormsModule,
				HttpClientModule,
				NoopAnimationsModule,
				NzFormModule,
				NzGridModule,
				NzTableModule,
				NzButtonModule
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
