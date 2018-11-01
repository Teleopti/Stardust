import { DOCUMENT } from '@angular/common';
import {
	HttpErrorResponse,
	HttpEvent,
	HttpHandler,
	HttpHeaders,
	HttpInterceptor,
	HttpRequest,
	HttpResponse
} from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { VersionService } from '../services';

@Injectable()
export class ClientOutdatedInterceptor implements HttpInterceptor {
	constructor(@Inject(DOCUMENT) private document: Document, private versionService: VersionService) {}

	intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		return next.handle(req).pipe(
			tap(
				event => {
					if (event instanceof HttpResponse) {
						this.versionCheck(event.headers);
					}
				},
				error => {
					if (error instanceof HttpErrorResponse) {
						this.versionCheck(error.headers);
					}
				}
			)
		);
	}

	versionCheck(headers: HttpHeaders) {
		const version = this.versionService.getVersion();
		const newVersion = headers.get('X-Server-Version') || '';
		if (newVersion.length === 0) return;
		else if (version.length === 0) this.versionService.setVersion(newVersion);
		else if (version !== newVersion) this.document.location.reload(true);
	}
}
