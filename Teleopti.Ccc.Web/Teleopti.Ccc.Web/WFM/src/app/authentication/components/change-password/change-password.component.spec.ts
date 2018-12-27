import { HttpClient } from '@angular/common/http';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzButtonModule, NzFormModule, NzInputModule, NzMessageModule, NzModalModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { PasswordService } from '../../services/password.service';
import { ChangePasswordComponent } from './change-password.component';

describe('ChangePasswordComponent', () => {
	let component: ChangePasswordComponent;
	let fixture: ComponentFixture<ChangePasswordComponent>;

	let page: Page;
	let passwordService: PasswordService;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [ChangePasswordComponent],
			imports: [
				MockTranslationModule,
				NzModalModule,
				NzFormModule,
				ReactiveFormsModule,
				NzButtonModule,
				NzInputModule,
				NzMessageModule,
				NoopAnimationsModule
			],
			providers: [
				PasswordService,
				{
					provide: HttpClient,
					useValue: {}
				}
			]
		}).compileComponents();
		passwordService = TestBed.get(PasswordService);
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(ChangePasswordComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should pass if no errors', () => {
		const authSuccessResponse = {
			IsSuccessful: true,
			IsAuthenticationSuccessful: true
		};
		spyOn(passwordService, 'setPassword').and.returnValue(of(authSuccessResponse));

		const component = fixture.componentInstance;

		component.currentPasswordControl.setValue('old');
		component.newPasswordControl.setValue('new');
		component.confirmPasswordControl.setValue('new');

		expect(component.newPasswordControl.hasError(component.ENSURE_PASSWORD_NEW_ERROR)).toEqual(false);
		expect(component.confirmPasswordControl.hasError(component.MATCHING_PASSWORDS)).toEqual(false);

		page.okButton.nativeElement.click();

		fixture.detectChanges();

		expect(component.currentPasswordControl.hasError(component.INVALID_PASSWORD_ERROR)).toEqual(false);
		expect(component.newPasswordControl.hasError(component.POLICY_ERROR)).toEqual(false);
	});

	it('should check for wrong password', () => {
		const authFailedResponse = {
			IsSuccessful: true,
			IsAuthenticationSuccessful: false,
			ErrorCode: null
		};
		spyOn(passwordService, 'setPassword').and.returnValue(of(authFailedResponse));

		const component = fixture.componentInstance;

		component.currentPasswordControl.setValue('wrong');
		component.newPasswordControl.setValue('new');
		component.confirmPasswordControl.setValue('new');

		page.okButton.nativeElement.click();

		fixture.detectChanges();
		expect(component.currentPasswordControl.hasError(component.INVALID_PASSWORD_ERROR)).toEqual(true);
	});

	it('should ensure not same password', () => {
		const component = fixture.componentInstance;

		component.currentPasswordControl.setValue('same');
		component.newPasswordControl.setValue('same');

		expect(component.newPasswordControl.hasError(component.ENSURE_PASSWORD_NEW_ERROR)).toEqual(true);

		component.newPasswordControl.setValue('not-the-same');

		expect(component.newPasswordControl.hasError(component.ENSURE_PASSWORD_NEW_ERROR)).toEqual(false);
	});
});

class Page extends PageObject {
	get currentPasswordField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-item-current-password]')[0];
	}

	get newPasswordField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-item-new-password]')[0];
	}

	get confirmPasswordField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-item-confirm-password]')[0];
	}

	get okButton() {
		return this.queryAll('[data-test-change-password-modal] .ant-btn-primary')[0];
	}
}
