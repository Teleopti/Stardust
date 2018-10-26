import { DOCUMENT } from '@angular/common';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { VersionService } from '../services';

@Injectable()
export class ClientOutdatedInterceptor implements HttpInterceptor {
	constructor(@Inject(DOCUMENT) private document: Document, private versionService: VersionService) {}

	intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		return this.versionService.getVersion().pipe(
			switchMap(version => {
				if (version.length === 0) {
					return next.handle(request);
				}

				const newRequest = this.createNewRequest(request, version);
				return this.getReponseHandler(next, newRequest);
			})
		);
	}

	private getReponseHandler<T>(next: HttpHandler, request: HttpRequest<T>) {
		return next.handle(request).pipe(
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

	private createNewRequest<T>(request: HttpRequest<T>, version): HttpRequest<T> {
		const clientVersionHeader = { 'X-Client-Version': version };
		return request.clone({ setHeaders: clientVersionHeader });
	}

	private handleOldClientError() {
		this.document.location.reload(true);
	}
}
