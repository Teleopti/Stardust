import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite } from '../../../../configure-test-suit';
import { PeopleModule } from '../../people.module';
import { IdentityLogonPageComponent } from './identity-logon-page.component';
import { IdentityLogonPageService } from './identity-logon-page.service';
import { WorkspaceComponent, PageContainerComponent } from '..';
import { MockTranslationModule } from '../../../../mocks/translation';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { MatInputModule } from '@angular/material';
import { ReactiveFormsModule } from '@angular/forms';
import {
	LogonInfoService,
	WorkspaceService,
	fakeBackendProvider,
	NavigationService,
	SearchService
} from '../../services';

describe('AppLogonPageComponent', () => {
	let component: IdentityLogonPageComponent;
	let fixture: ComponentFixture<IdentityLogonPageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [IdentityLogonPageComponent, PageContainerComponent, WorkspaceComponent],
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
		fixture = TestBed.createComponent(IdentityLogonPageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
