import { DOCUMENT } from '@angular/common';
import { HttpResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DocumentMock } from '@wfm/mocks/dom';
import { DummyHttpService } from '../mocks';
import { InterceptorOverrideService } from '../services/interceptor-override.service';
import { AuthenticatedInterceptor } from './authenticated.interceptor';

describe('AuthenticatedInterceptor', () => {
	let service: DummyHttpService;
	let httpMock: HttpTestingController;
	let document: Document;
	let overrideInterceptorService: InterceptorOverrideService;

	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [HttpClientTestingModule],
			providers: [
				{ provide: DOCUMENT, useClass: DocumentMock },
				InterceptorOverrideService,
				{
					provide: HTTP_INTERCEPTORS,
					useClass: AuthenticatedInterceptor,
					multi: true
				},
				DummyHttpService
			]
		});

		service = TestBed.get(DummyHttpService);
		httpMock = TestBed.get(HttpTestingController);
		document = TestBed.get(DOCUMENT);
		overrideInterceptorService = TestBed.get(InterceptorOverrideService);
	});

	it('should redirect user on 401 error', () => {
		service.dummy().subscribe({
			error: (response: HttpResponse<any>) => {
				expect(response.status).toBe(401);
			}
		});

		httpMock.expectOne('/dummy').flush(null, { status: 401, statusText: '401 error' });
		expect(document.location.href).toContain('Authentication?redirectUrl=');
	});

	it('should be able to ignore request errors', () => {
		overrideInterceptorService.shouldIntercept = false;

		service.dummy().subscribe({
			error: (response: HttpResponse<any>) => {
				expect(response.status).toBe(401);
			}
		});

		httpMock.expectOne('/dummy').flush(null, { status: 401, statusText: '401 error' });
		expect(document.location.href).toBe('');
	});
});
