import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnInit, ViewChild } from '@angular/core';
import { ChangePasswordComponent } from '../../../authentication/components/change-password/change-password.component';
import { ThemeService, TogglesService } from '../../../core/services';

@Component({
	selector: 'settings-menu',
	templateUrl: './settings-menu.component.html',
	styleUrls: ['./settings-menu.component.scss']
})
export class SettingsMenuComponent implements OnInit {
	lowLightFilter: boolean = false;
	darkTheme: boolean = false;
	visible: boolean;
	changePasswordToggle: boolean = false;

	@ViewChild('passwordModal')
	passwordModal: ChangePasswordComponent;

	constructor(
		private themeService: ThemeService,
		@Inject(DOCUMENT) private document: Document,
		public toggleService: TogglesService
	) {
		this.toggleService.getToggles().subscribe({
			next: toggles => {
				this.changePasswordToggle = toggles.Wfm_Authentication_ChangePasswordMenu_76666;
			}
		});
		this.themeService.getTheme().subscribe({
			next: theme => {
				this.lowLightFilter = theme.Overlay;
				this.darkTheme = theme.Name === 'dark';
			}
		});
	}

	toggleLightFilter(bool: boolean) {
		console.log('toggle light filter');
		this.themeService.saveThemePreference({
			Name: this.darkTheme ? 'dark' : 'classic',
			Overlay: bool
		});
	}

	toggleDarkTheme(bool: boolean) {
		console.log('toggle dark theme');
		const themeStyle = bool ? 'dark' : 'classic';
		this.themeService.saveThemePreference({
			Name: themeStyle,
			Overlay: this.lowLightFilter
		});
		this.themeService.applyTheme(themeStyle);
	}

	ngOnInit() {}

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

	toggleVisible(v) {
		console.log('toggleVisible', v);
	}
}
