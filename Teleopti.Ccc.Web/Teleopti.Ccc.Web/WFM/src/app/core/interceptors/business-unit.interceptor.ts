import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class BusinessUnitInterceptor implements HttpInterceptor {
	constructor() {}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		const businessUnitId = sessionStorage.getItem('buid') || '';
		const businessUnitHeader = { 'X-Business-Unit-Filter': businessUnitId };
		const requestWithBusinessUnitHeader = req.clone({ setHeaders: businessUnitHeader });
		return next.handle(requestWithBusinessUnitHeader);
	}
}
