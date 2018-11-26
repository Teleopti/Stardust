import { Injectable } from '@angular/core';

/**
 * This is use to avoid using interceptors for certain requests.
 * Use with great care.
 */
@Injectable()
export class InterceptorOverrideService {
	private ignoreAll = false;

	shouldIntercept(): boolean {
		return !this.ignoreAll;
	}

	getIgnoreAll(): boolean {
		return this.ignoreAll;
	}

	setIgnoreAll(ignoreAll: boolean) {
		this.ignoreAll = ignoreAll;
	}
}
