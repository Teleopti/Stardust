import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzAlertModule, NzButtonModule, NzFormModule, NzInputModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { ResetPasswordService, ResetRequest } from '../../shared/reset-password.service';
import { ResetPasswordFormComponent } from './reset-password-form.component';

class MockResetPasswordService implements Partial<ResetPasswordService> {
	reset(body: ResetRequest) {
		return of(true);
	}
}

describe('ResetPasswordFormComponent', () => {
	let component: ResetPasswordFormComponent;
	let fixture: ComponentFixture<ResetPasswordFormComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [ResetPasswordFormComponent],
			imports: [
				MockTranslationModule,
				ReactiveFormsModule,
				NzFormModule,
				NzAlertModule,
				NzButtonModule,
				NzInputModule
			],
			providers: [{ provide: ResetPasswordService, useValue: MockResetPasswordService }]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(ResetPasswordFormComponent);
		component = fixture.componentInstance;

		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should not pass empty password', () => {
		component.passwordControl.setValue('');
		component.passwordControl.updateValueAndValidity();

		expect(component.passwordControl.hasError('required')).toBe(true);

		component.passwordControl.setValue('password1');
		component.passwordControl.updateValueAndValidity();

		expect(component.passwordControl.hasError('required')).toBe(false);
	});

	it('should pass identical passwords', () => {
		component.passwordControl.setValue('password1');
		component.passwordControl.updateValueAndValidity();

		component.confirmPasswordControl.setValue('password1');
		component.confirmPasswordControl.updateValueAndValidity();

		expect(component.isFormValid).toBeTruthy();

		component.confirmPasswordControl.setValue('password12');
		component.confirmPasswordControl.updateValueAndValidity();

		expect(component.isFormValid).toBe(false);
	});

	/*it('should submit form', () => {
		component.passwordControl.setValue('password1');
		component.passwordControl.updateValueAndValidity();

		component.confirmPasswordControl.setValue('password1');
		component.confirmPasswordControl.updateValueAndValidity();

		component.onSubmit();
	});*/
});

class Page extends PageObject {
	get menuItems() {
		return this.queryAll('[data-test-menu-item]');
	}
}
