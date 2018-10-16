import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DowngradeableComponent } from '@wfm/types';
import { SharedModule } from '../shared/shared.module';
import { ChangePasswordComponent } from './components/change-password/change-password.component';
import { PasswordService } from './services/password.service';

@NgModule({
	imports: [CommonModule, TranslateModule.forChild(), SharedModule, ReactiveFormsModule],
	declarations: [ChangePasswordComponent],
	exports: [ChangePasswordComponent],
	providers: [PasswordService]
})
export class AuthenticationModule {}

export const authenticationComponents: DowngradeableComponent[] = [
	{ ng1Name: 'ng2ChangePassword', ng2Component: ChangePasswordComponent }
];
