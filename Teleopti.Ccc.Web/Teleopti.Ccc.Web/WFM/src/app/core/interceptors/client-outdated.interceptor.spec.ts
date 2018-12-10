import { DOCUMENT } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DocumentMock } from '@wfm/mocks/dom';
import { DummyHttpService } from '../mocks';
import { VersionService } from '../services';
import { ClientOutdatedInterceptor } from './client-outdated.interceptor';

describe('ClientOutdatedInterceptor', () => {
	let service: DummyHttpService;
	let httpMock: HttpTestingController;
	let document: Document;
	let versionService: VersionService;

	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [HttpClientTestingModule],
			providers: [
				{ provide: DOCUMENT, useClass: DocumentMock },
				VersionService,
				{
					provide: HTTP_INTERCEPTORS,
					useClass: ClientOutdatedInterceptor,
					multi: true
				},
				DummyHttpService
			]
		});

		service = TestBed.get(DummyHttpService);
		httpMock = TestBed.get(HttpTestingController);
		document = TestBed.get(DOCUMENT);
		versionService = TestBed.get(VersionService);
	});

	it('should save response version header', () => {
		service.dummy().subscribe();

		httpMock.expectOne('/dummy').flush(null, { headers: { 'X-Server-Version': '1.0' } });
		expect(versionService.getVersion()).toBe('1.0');
	});

	it('should reload if outdated', () => {
		versionService.setVersion('1.0');
		const reloadSpy = spyOn(document.location, 'reload');
		service.dummy().subscribe();

		httpMock.expectOne('/dummy').flush(null, { headers: { 'X-Server-Version': '1.1' } });
		expect(reloadSpy.calls.count()).toBe(1);
	});
});
