import {
	HTTP_INTERCEPTORS,
	HttpEvent,
	HttpHandler,
	HttpInterceptor,
	HttpRequest,
	HttpResponse
} from '@angular/common/http';
import { Injectable, Provider } from '@angular/core';
import { Observable ,  of } from 'rxjs';
import { PeopleSearchResult } from '..';
import { Person, Role } from '../../types';
import { ROLES } from './roles';
import { PEOPLE } from './people';
import { mergeMap } from 'rxjs/operators';
import { LOGONS } from './logons';

@Injectable()
export class FakeBackendInterceptor implements HttpInterceptor {
	constructor() {}

	intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		// wrap in delayed observable to simulate server api call
		return of(null).pipe(
			mergeMap(() => {
				if (request.url.endsWith('/api/PeopleData/fetchRoles') && request.method === 'GET') {
					return of(new HttpResponse({ status: 200, body: ROLES }));
				}

				if (request.url.endsWith('/api/PeopleData/fetchPersons') && request.method === 'POST') {
					return of(new HttpResponse({ status: 200, body: PEOPLE }));
				}
				if (request.url.endsWith('/PersonInfo/LogonInfoFromGuids') && request.method === 'POST') {
					return of(new HttpResponse({ status: 200, body: LOGONS }));
				}

				if (request.url.endsWith('/api/Search/FindPeople') && request.method === 'POST') {
					const response: PeopleSearchResult = {
						People: PEOPLE,
						TotalRows: PEOPLE.length
					};
					return of(new HttpResponse({ status: 200, body: response }));
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
