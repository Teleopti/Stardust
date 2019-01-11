import { Inject, Injectable } from '@angular/core';
import { WINDOW } from '@wfm/common';
import { Observable, ReplaySubject } from 'rxjs';

@Injectable()
export class MediaQueryService {
	private _isMobileView$ = new ReplaySubject<boolean>(1);
	public get isMobileSize$() {
		return this._isMobileView$ as Observable<boolean>;
	}

	constructor(@Inject(WINDOW) private window: Window) {
		this._isMobileView$.next(window.innerWidth <= 768);
		window.matchMedia('(max-width: 768px)').addListener(mq => {
			this._isMobileView$.next(mq.matches);
		});
	}
}
