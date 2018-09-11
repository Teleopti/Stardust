import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

export interface Theme {
	readonly Name: 'classic' | 'dark';
	readonly Overlay: boolean;
}

type ThemeType = 'classic' | 'dark';

@Injectable()
export class ThemeService {
	private theme$ = new ReplaySubject<Theme>(1);

	constructor(private http: HttpClient) {
		this.http.get('../api/Theme').subscribe({
			next: (theme: Theme) => {
				this.theme$.next(theme);
				this.saveLocally(theme);
				this.applyTheme(theme.Name);
			}
		});
	}

	getTheme(): Observable<Theme> {
		return this.theme$;
	}

	saveLocally(theme: Theme) {
		localStorage.setItem('theme', theme.Name);
	}

	saveThemePreference(theme: Theme): void {
		this.saveLocally(theme);
		this.theme$.next(theme);
		this.http.post('../api/Theme/Change', theme).subscribe();
	}

	// This can be made platform agnostic
	applyTheme(themeToApply: ThemeType) {
		if (this.getCurrentTheme() !== themeToApply) {
			const waitForThemes = Promise.all([
				this.applyAngularMatrialTheme(themeToApply),
				this.applyStyleguideTheme(themeToApply),
				this.applyAntTheme(themeToApply)
			]);
		}
	}

	async applyAngularMatrialTheme(theme: ThemeType) {
		if (theme === 'dark' && document.documentElement) {
			document.documentElement.classList.add('angular-theme-dark');
			document.documentElement.classList.remove('angular-theme-classic');
		}
		if (theme === 'classic' && document.documentElement) {
			document.documentElement.classList.add('angular-theme-classic');
			document.documentElement.classList.remove('angular-theme-dark');
		}
		return;
	}

	applyStyleguideTheme(theme) {
		return this.replaceCssFile(`dist/styleguide_${theme}.css`, 'themeStyleguide', theme);
	}

	applyAntTheme(theme: ThemeType) {
		return this.replaceCssFile(`dist/ant_${theme}.css`, 'themeAnt', theme);
	}

	replaceCssFile(path: string, id: string, theme: ThemeType) {
		return new Promise((res, rej) => {
			var oldNode = document.getElementById(id);
			var newNode = document.createElement('link');
			newNode.id = id;
			newNode.rel = 'stylesheet';

			newNode.addEventListener('load', () => {
				res();
			});

			newNode.setAttribute('href', path);
			newNode.setAttribute('class', theme);

			document.body.replaceChild(newNode, oldNode);
		});
	}

	getCurrentTheme(): ThemeType {
		let classList = document.documentElement.classList;
		if (classList.contains('angular-theme-dark')) {
			return 'dark';
		} else if (classList.contains('angular-theme-classic')) {
			return 'classic';
		} else {
			return null;
		}
	}
}
