import { DOCUMENT } from '@angular/common';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { InterceptorOverrideService } from '../services/interceptor-override.service';

@Injectable()
export class AuthenticatedInterceptor implements HttpInterceptor {
	constructor(
		@Inject(DOCUMENT) private document: Document,
		private overrideInterceptorService: InterceptorOverrideService
	) {}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		if (!this.overrideInterceptorService.shouldIntercept()) return next.handle(req);
		return next.handle(req).pipe(
			tap(
				event => {},
				error => {
					if (error instanceof HttpErrorResponse && error.status === 401) {
						this.handleUnauthorized();
					}
				}
			)
		);
	}

	handleUnauthorized() {
		this.document.location.href = 'Authentication?redirectUrl=' + document.location.hash;
	}
}
