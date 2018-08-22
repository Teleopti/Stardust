import { DebugElement } from '@angular/core';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { NzButtonModule, NzFormModule, NzInputModule, NzMessageModule, NzModalModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { configureTestSuite } from '../../../../configure-test-suit';
import { MockTranslationModule } from '../../../../mocks/translation';
import { Spied } from '../../../shared/utils/jasmine-spy';
import { PasswordService } from '../../services/password.service';
import { ChangePasswordComponent } from './change-password.component';

describe('ChangePasswordComponent', () => {
	let component: ChangePasswordComponent;
	let fixture: ComponentFixture<ChangePasswordComponent>;

	let page: Page;
	let passwordService: Spied<PasswordService>;

	configureTestSuite();

	beforeEach(async(() => {
		passwordService = jasmine.createSpyObj('PasswordService', ['setPassword']);

		TestBed.configureTestingModule({
			declarations: [ChangePasswordComponent],
			imports: [
				MockTranslationModule,
				NzModalModule,
				NzFormModule,
				ReactiveFormsModule,
				NzButtonModule,
				NzInputModule,
				NzMessageModule
			],
			providers: [
				{
					provide: PasswordService,
					useValue: passwordService
				}
			]
		}).compileComponents();
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

	it('should ensure not same password', () => {
		const component = fixture.componentInstance;

		component.currentPasswordControl.setValue('same');
		component.newPasswordControl.setValue('same');

		expect(component.newPasswordControl.hasError(component.ENSURE_PASSWORD_NEW_ERROR)).toEqual(true);

		component.newPasswordControl.setValue('not-the-same');

		expect(component.newPasswordControl.hasError(component.ENSURE_PASSWORD_NEW_ERROR)).toEqual(false);
	});

	fit('should check for wrong password', () => {
		const authFailedResponse = {
			IsSuccessful: true,
			IsAuthenticationSuccessful: false
		};
		const getTestSpy = passwordService.setPassword.and.returnValue(of(authFailedResponse));
		const component = fixture.componentInstance;

		component.currentPasswordControl.setValue('old');
		component.newPasswordControl.setValue('new');
		component.confirmPasswordControl.setValue('new');

		page.okButton.nativeElement.click();

		fixture.detectChanges();

		expect(component.currentPasswordControl.hasError(component.INVALID_PASSWORD_ERROR)).toEqual(true);
	});
});

class Page {
	get currentPasswordField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-item-current-password]')[0];
	}

	get newPasswordField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-item-new-password]')[0];
	}

	get confirmPasswordField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-item-confirm-password]')[0];
	}

	get invalidMessageField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-invalid-error]')[0];
	}

	get notNewMessageField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-input-new-password]')[0];
	}

	get requiredMessageField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-required-error]')[0];
	}

	get notMatchingMessageField() {
		return this.queryAll('[data-test-change-password-modal] [data-test-not-matching-error]')[0];
	}

	get okButton() {
		return this.queryAll('[data-test-change-password-modal] .ant-btn-primary')[0];
	}

	fixture: ComponentFixture<ChangePasswordComponent>;

	constructor(fixture: ComponentFixture<ChangePasswordComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
