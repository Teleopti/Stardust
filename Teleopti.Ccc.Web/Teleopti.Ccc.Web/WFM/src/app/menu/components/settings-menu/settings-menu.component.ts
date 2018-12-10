import { DOCUMENT } from '@angular/common';
import { Component, Inject, ViewChild } from '@angular/core';
import { ChangePasswordComponent } from '../../../authentication/components/change-password/change-password.component';
import { ThemeService, TogglesService, UserService } from '../../../core/services';

@Component({
	selector: 'settings-menu',
	templateUrl: './settings-menu.component.html',
	styleUrls: ['./settings-menu.component.scss']
})
export class SettingsMenuComponent {
	lowLightFilter = false;
	darkTheme = false;
	visible: boolean;
	changePasswordToggle = false;
	isTeleoptiApplicationLogon = false;
	UserName = '';

	@ViewChild('passwordModal')
	passwordModal: ChangePasswordComponent;

	constructor(
		private themeService: ThemeService,
		@Inject(DOCUMENT) private document: Document,
		public toggleService: TogglesService,
		public userService: UserService
	) {
		this.toggleService.toggles$.subscribe({
			next: toggles => {
				this.changePasswordToggle = toggles.Wfm_Authentication_ChangePasswordMenu_76666;
			}
		});
		this.userService.preferences$.subscribe({
			next: preferences => {
				this.isTeleoptiApplicationLogon = preferences.IsTeleoptiApplicationLogon;
				this.UserName = preferences.UserName;
			}
		});
		this.themeService.theme$.subscribe({
			next: theme => {
				this.lowLightFilter = theme.Overlay;
				this.darkTheme = theme.Name === 'dark';
			}
		});
	}

	toggleLightFilter(bool: boolean) {
		this.themeService.saveThemePreference({
			Name: this.darkTheme ? 'dark' : 'classic',
			Overlay: bool
		});
	}

	toggleDarkTheme(bool: boolean) {
		const themeStyle = bool ? 'dark' : 'classic';
		this.themeService.saveThemePreference({
			Name: themeStyle,
			Overlay: this.lowLightFilter
		});
		this.themeService.applyTheme(themeStyle);
	}

	hide() {
		this.visible = false;
	}

	showChangePasswordDialog() {
		this.passwordModal.showModal();
		this.hide();
	}

	logOut() {
		this.document.location.assign('../Authentication/SignOut');
	}
}
