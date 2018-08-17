import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { PasswordService } from '../../services/password.service';

export class MatchingPasswordValidation {
	static MatchPassword(AC: AbstractControl) {
		let password = AC.get('newPassword').value; // to get value in input tag
		let confirmPassword = AC.get('confirmPassword').value; // to get value in input tag
		if (password != confirmPassword) {
			AC.get('confirmPassword').setErrors({ MatchPassword: true });
		} else {
			AC.get('confirmPassword').setErrors(null);

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
	readonly POLICY_ERROR = 'MeetsPolicy';
	isVisible = false;
	changePasswordForm: FormGroup;

	constructor(private fb: FormBuilder, private ps: PasswordService) {
		this.changePasswordForm = this.fb.group(
			{
				currentPassword: new FormControl('', Validators.required),
				newPassword: new FormControl('', Validators.required),
				confirmPassword: new FormControl('', Validators.required)
			},
			{
				validator: MatchingPasswordValidation.MatchPassword
			}
		);
	}

	showModal() {
		this.isVisible = true;
	}

	hasClientError() {
		return [this.currentPasswordControl, this.newPasswordControl].some(control => {
			return control.hasError('required') || control.hasError('MatchPassword');
		});
	}

	handleOk(): void {
		if (!this.hasClientError()) {
			let response = this.ps
				.setPassword({
					OldPassword: this.changePasswordForm.get('currentPassword').value,
					NewPassword: this.changePasswordForm.get('newPassword').value
				})
				.subscribe({
					next: ({ IsSuccessful, IsAuthenticationSuccessful }) => {
						if (!IsAuthenticationSuccessful) {
							this.currentPasswordControl.setErrors({ [this.INVALID_PASSWORD_ERROR]: true });
						} else if (!IsSuccessful) {
							this.newPasswordControl.setErrors({ [this.POLICY_ERROR]: true });
						}
					}
				});

			//this.isVisible = false;
		} else {
			this.invalidate(this.currentPasswordControl);
			this.invalidate(this.newPasswordControl);
			this.invalidate(this.confirmPasswordControl);
		}
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

	handleCancel(): void {
		this.changePasswordForm.reset();
		this.isVisible = false;
	}
}
