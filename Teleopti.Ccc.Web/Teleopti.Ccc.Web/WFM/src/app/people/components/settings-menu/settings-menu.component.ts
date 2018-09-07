import { Component, OnInit } from '@angular/core';
import { ThemeService } from '../../../core/services';

@Component({
	selector: 'settings-menu',
	templateUrl: './settings-menu.component.html',
	styleUrls: ['./settings-menu.component.scss']
})
export class SettingsMenuComponent implements OnInit {
	lowLightFilter: boolean = false;
	darkTheme: boolean = false;
	visible: boolean;

	constructor(private themeService: ThemeService) {
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
		console.log('Show password dialog');
		this.hide();
	}

	toggleVisible(v) {
		console.log('toggleVisible', v);
	}
}
