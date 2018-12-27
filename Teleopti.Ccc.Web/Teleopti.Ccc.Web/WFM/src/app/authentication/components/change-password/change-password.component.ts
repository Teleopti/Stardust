import { Component, ElementRef, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { NzMessageService } from 'ng-zorro-antd';
import { PasswordService } from '../../services/password.service';

export class MatchingPasswordValidation {
	static MatchPassword(AC: AbstractControl) {
		const password = AC.get('newPassword').value; // to get value in input tag
		const confirmPassword = AC.get('confirmPassword').value; // to get value in input tag
		if (password !== confirmPassword) {
			AC.get('confirmPassword').setErrors({ MatchPassword: true });
		} else {
			AC.get('confirmPassword').setErrors(null);

			return null;
		}
	}
}
export class EnsurePasswordIsNewValidation {
	static IsNewPassword(AC: AbstractControl) {
		const newPassword = AC.get('newPassword').value; // to get value in input tag
		const currentPassword = AC.get('currentPassword').value; // to get value in input tag
		if (newPassword === currentPassword) {
			AC.get('newPassword').setErrors({ IsNewPassword: true });
		} else {
			AC.get('newPassword').setErrors(null);

			return null;
		}
	}
}

@Component({
	selector: 'app-change-password',
	templateUrl: './change-password.component.html',
	styleUrls: ['./change-password.component.scss']
})
export class ChangePasswordComponent {
	readonly INVALID_PASSWORD_ERROR = 'InvalidPassword';
	readonly POLICY_ERROR = 'Error_Policy';
	readonly ENSURE_PASSWORD_NEW_ERROR = 'IsNewPassword';
	readonly MATCHING_PASSWORDS = 'MatchPassword';
	isVisible = false;
	changePasswordForm: FormGroup;

	@ViewChild('passwordField')
	passwordFieldRef: ElementRef;

	constructor(
		private fb: FormBuilder,
		private ps: PasswordService,
		private messageService: NzMessageService,
		private translateService: TranslateService
	) {
		this.changePasswordForm = this.fb.group(
			{
				currentPassword: new FormControl('', Validators.required),
				newPassword: new FormControl('', Validators.required),
				confirmPassword: new FormControl('', Validators.required)
			},
			{
				validator: [EnsurePasswordIsNewValidation.IsNewPassword, MatchingPasswordValidation.MatchPassword]
			}
		);
	}

	showModal() {
		this.isVisible = true;
	}

	focusInput() {
		this.passwordFieldRef.nativeElement.focus();
	}

	hasClientError() {
		return [this.currentPasswordControl, this.newPasswordControl, this.confirmPasswordControl].some(control => {
			return (
				control.hasError('required') ||
				control.hasError(this.ENSURE_PASSWORD_NEW_ERROR) ||
				control.hasError(this.MATCHING_PASSWORDS)
			);
		});
	}

	handleOk(): void {
		if (!this.hasClientError()) {
			this.ps
				.setPassword({
					OldPassword: this.changePasswordForm.get('currentPassword').value,
					NewPassword: this.changePasswordForm.get('newPassword').value
				})
				.subscribe({
					next: ({ IsSuccessful, IsAuthenticationSuccessful, ErrorCode }) => {
						if (!IsAuthenticationSuccessful && ErrorCode === null) {
							this.currentPasswordControl.setErrors({ [this.INVALID_PASSWORD_ERROR]: true });
						} else if (!IsSuccessful && ErrorCode === this.POLICY_ERROR) {
							this.newPasswordControl.setErrors({ [this.POLICY_ERROR]: true });
						} else {
							this.close();
							this.successMessage();
						}
					}
				});
		} else {
			this.invalidate(this.currentPasswordControl);
			this.invalidate(this.newPasswordControl);
			this.invalidate(this.confirmPasswordControl);
		}
	}

	private successMessage() {
		this.translateService.get('PasswordChangedSuccessfully').subscribe({
			next: message => {
				this.messageService.success(message);
			}
		});
	}

	get currentPasswordControl() {
		return this.changePasswordForm.get('currentPassword');
	}

	get newPasswordControl() {
		return this.changePasswordForm.get('newPassword');
	}

	get confirmPasswordControl() {
		return this.changePasswordForm.get('confirmPassword');
	}

	invalidate(control: AbstractControl) {
		control.markAsDirty();
		control.updateValueAndValidity();
	}

	close(): void {
		this.changePasswordForm.reset();
		this.isVisible = false;
	}
}
