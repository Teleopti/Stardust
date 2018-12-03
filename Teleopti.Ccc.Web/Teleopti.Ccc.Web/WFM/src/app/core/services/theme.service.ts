import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Theme {
	readonly Name: 'classic' | 'dark';
	readonly Overlay: boolean;
}

type ThemeType = 'classic' | 'dark';

@Injectable()
export class ThemeService {
	private _theme$ = new ReplaySubject<Theme>(1);

	public get theme$(): Observable<Theme> {
		return this._theme$;
	}

	constructor(@Inject(DOCUMENT) private document: Document, private http: HttpClient) {
		this.http
			.get('../api/Theme')
			.pipe(map(this.ensureThemeNotNull.bind(this)))
			.subscribe({
				next: (theme: Theme) => {
					this._theme$.next(theme);
					this.applyTheme(theme.Name);
				}
			});
	}

	private ensureThemeNotNull(theme: Theme): Theme {
		return {
			Name: theme.Name || 'classic',
			Overlay: theme.Overlay
		};
	}

	saveThemePreference(theme: Theme): void {
		this._theme$.next(theme);
		this.http.post('../api/Theme/Change', theme).subscribe();
	}

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
		if (theme === 'dark' && this.document.documentElement) {
			this.document.documentElement.classList.add('angular-theme-dark');
			document.documentElement.classList.remove('angular-theme-classic');
		}
		if (theme === 'classic' && this.document.documentElement) {
			this.document.documentElement.classList.add('angular-theme-classic');
			this.document.documentElement.classList.remove('angular-theme-dark');
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
			const oldNode = this.document.getElementById(id);
			const newNode = this.document.createElement('link');
			newNode.id = id;
			newNode.rel = 'stylesheet';

			newNode.addEventListener('load', () => {
				res();
			});

			newNode.setAttribute('href', path);
			newNode.setAttribute('class', theme);

			this.document.body.replaceChild(newNode, oldNode);
		});
	}

	getCurrentTheme(): ThemeType {
		const classList = this.document.documentElement.classList;
		if (classList.contains('angular-theme-dark')) {
			return 'dark';
		} else if (classList.contains('angular-theme-classic')) {
			return 'classic';
		} else {
			return null;
		}
	}
}
