import {
	HttpEvent,
	HttpHandler,
	HttpInterceptor,
	HttpRequest,
	HttpResponse,
	HTTP_INTERCEPTORS
} from '@angular/common/http';
import { Injectable, Provider } from '@angular/core';
import { Observable, of } from 'rxjs';
import { mergeMap } from 'rxjs/operators';
import { APPLICATIONS, TOKEN } from './external-applications';

@Injectable()
export class FakeBackendInterceptor implements HttpInterceptor {
	constructor() {}

	intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		// wrap in delayed observable to simulate server api call
		return of(null).pipe(
			mergeMap(() => {
				if (request.url.endsWith('/api/token') && request.method === 'GET') {
					return of(new HttpResponse({ status: 200, body: APPLICATIONS }));
				}

				if (request.url.endsWith('/api/token') && request.method === 'POST') {
					return of(new HttpResponse({ status: 200, body: TOKEN }));
				}

				if (request.url.endsWith('/api/token') && request.method === 'DELETE') {
					return of(new HttpResponse({ status: 200 }));
				}

				// pass through any requests not handled above
				return next.handle(request);
			})
		);
	}
}

export let fakeBackendProvider: Provider = {
	// use fake backend in place of Http service for backend-less development
	provide: HTTP_INTERCEPTORS,
	useClass: FakeBackendInterceptor,
	multi: true
};
