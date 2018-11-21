import { Injectable, Inject } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { first } from 'rxjs/operators';
import { WINDOW } from '@wfm/common';

@Injectable()
export class ToggleMenuService {
	private _showMenu$ = new ReplaySubject<boolean>(1);
	public get showMenu$() {
		return this._showMenu$ as Observable<boolean>;
	}

	constructor(@Inject(WINDOW) private window: Window) {
		this._showMenu$.next(!this.isMobileView());
	}

	public setMenuVisible(isVisible: boolean) {
		this._showMenu$.next(isVisible);
	}

	public toggle() {
		this.showMenu$.pipe(first()).subscribe(isVisible => {
			this.setMenuVisible(!isVisible);
		});
	}

	private isMobileView() {
		return this.window.innerWidth <= 768;
	}
}
