import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { configureTestSuite, PageObject } from '@wfm/test';
import { NzAlertModule, NzButtonModule, NzFormModule, NzInputModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { InvalidLinkMessageComponent } from '../components/invalid-link-message.component';
import { ResetPasswordFormComponent } from '../components/reset-password-form/reset-password-form.component';
import { SuccessMessageComponent } from '../components/success-message.component';
import { ResetPasswordService } from '../shared/reset-password.service';
import { ResetPasswordPageComponent } from './reset-password.component';

const resetPasswordStateService: Partial<ResetPasswordService> = {};

describe('ResetPasswordPageComponent', () => {
	let component: ResetPasswordPageComponent;
	let fixture: ComponentFixture<ResetPasswordPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				ResetPasswordPageComponent,
				ResetPasswordFormComponent,
				SuccessMessageComponent,
				InvalidLinkMessageComponent
			],
			imports: [
				MockTranslationModule,
				ReactiveFormsModule,
				NzFormModule,
				NzAlertModule,
				NzButtonModule,
				NzInputModule,
				BrowserAnimationsModule
			],
			providers: [{ provide: ResetPasswordService, useValue: resetPasswordStateService }]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(ResetPasswordPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should show successful reset message', () => {
		component.token = 'thisisatokeniguess';
		component.successfulReset = true;
		component.hasValidToken$ = of(true);
		fixture.detectChanges();
		expect(page.successMessage.length).toEqual(1);
		expect(page.invalidLinkMessage.length).toEqual(0);
		expect(page.resetPasswordForm.length).toEqual(0);
	});

	it('should show invalid link', () => {
		component.token = undefined;
		component.successfulReset = false;
		component.hasValidToken$ = of(false);
		fixture.detectChanges();
		expect(page.successMessage.length).toEqual(0);
		expect(page.invalidLinkMessage.length).toEqual(1);
		expect(page.resetPasswordForm.length).toEqual(0);
	});

	it('should show the reset form', () => {
		component.token = 'thisisatokeniguess';
		component.successfulReset = false;
		component.hasValidToken$ = of(true);
		fixture.detectChanges();
		expect(page.successMessage.length).toEqual(0);
		expect(page.invalidLinkMessage.length).toEqual(0);
		expect(page.resetPasswordForm.length).toEqual(1);
	});
});

class Page extends PageObject {
	get invalidLinkMessage() {
		return this.queryAll('[data-invalid-message]');
	}

	get successMessage() {
		return this.queryAll('[data-success-message]');
	}

	get resetPasswordForm() {
		return this.queryAll('[data-reset-password-form]');
	}
}
