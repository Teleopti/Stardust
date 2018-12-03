import { Injectable } from '@angular/core';

/**
 * This is use to avoid using interceptors for certain requests.
 * Use with great care.
 */
@Injectable()
export class InterceptorOverrideService {
	public shouldIntercept = true;
}
