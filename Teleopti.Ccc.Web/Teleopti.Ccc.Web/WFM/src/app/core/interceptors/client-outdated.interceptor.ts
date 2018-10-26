import { DOCUMENT } from '@angular/common';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable()
export class ClientOutdatedInterceptor implements HttpInterceptor {
	constructor(@Inject(DOCUMENT) private document: Document) {}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		const clientVersion = sessionStorage.getItem('X-Client-Version');
		if (clientVersion) {
			const clientVersionHeader = { 'X-Client-Version': clientVersion };
			const requestWithClientVersionHeader = req.clone({ setHeaders: clientVersionHeader });
			return next.handle(requestWithClientVersionHeader).pipe(
				tap(
					event => {},
					error => {
						if (error instanceof HttpErrorResponse && error.status == 418) {
							this.handleOldClientError();
						}
					}
				)
			);
		}
		return next.handle(req);
	}

	handleOldClientError() {
		this.document.location.reload(true);
	}
}
