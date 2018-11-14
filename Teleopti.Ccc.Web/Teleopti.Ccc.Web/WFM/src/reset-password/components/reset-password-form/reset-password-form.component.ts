import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ResetPasswordService } from '../../shared/reset-password.service';

export class MatchingPasswordValidation {
	static MatchPassword(AC: AbstractControl) {
		let password = AC.get('password').value; // to get value in input tag
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
	selector: 'reset-password-form',
	templateUrl: './reset-password-form.component.html',
	styleUrls: ['./reset-password-form.component.scss']
})
export class ResetPasswordFormComponent {
	constructor(private fb: FormBuilder, public resetPasswordService: ResetPasswordService) {}

	@Input()
	token: string;

	@Output()
	onSuccess: EventEmitter<boolean> = new EventEmitter();
	@Output()
	onCancel = new EventEmitter();

	form: FormGroup = this.fb.group(
		{
			password: [null, [Validators.required]],
			confirmPassword: [null, [Validators.required]]
		},
		{
			validator: [MatchingPasswordValidation.MatchPassword]
		}
	);

	cancel() {
		this.onCancel.emit();
	}

	get isFormValid() {
		return Object.keys(this.form.controls).every(key => this.form.controls[key].errors == null);
	}

	get passwordControl(): AbstractControl {
		return this.form.get('password');
	}

	onSubmit(): void {
		this.form.setErrors({ resetFailed: false });
		for (const i in this.form.controls) {
			this.form.controls[i].markAsDirty();
			this.form.controls[i].updateValueAndValidity();
		}
		if (this.isFormValid) {
			this.resetPasswordService
				.reset({
					ResetToken: this.token,
					NewPassword: this.passwordControl.value
				})
				.subscribe({
					next: () => {
						this.onSuccess.emit(true);
					},
					error: () => {
						this.form.setErrors({ resetFailed: true });
					}
				});
		}
	}
}
