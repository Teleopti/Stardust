import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { DummyHttpService } from '../mocks';
import { BusinessUnitInterceptor } from './business-unit.interceptor';

class MockStorage implements Partial<Storage> {
	store = {};
	getItem = (key: string): string => {
		return key in this.store ? this.store[key] : null;
	};
	setItem = (key: string, value: string) => {
		this.store[key] = `${value}`;
	};
	removeItem = (key: string) => {
		delete this.store[key];
	};
	clear = () => {
		this.store = {};
	};
}

describe('BusinessUnitInterceptor', () => {
	let service: DummyHttpService;
	let httpMock: HttpTestingController;
	let mockStorage: MockStorage;

	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [HttpClientTestingModule],
			providers: [
				{
					provide: HTTP_INTERCEPTORS,
					useClass: BusinessUnitInterceptor,
					multi: true
				},
				DummyHttpService
			]
		});

		mockStorage = new MockStorage();

		spyOn(sessionStorage, 'getItem').and.callFake(mockStorage.getItem);
		spyOn(sessionStorage, 'setItem').and.callFake(mockStorage.setItem);
		spyOn(sessionStorage, 'removeItem').and.callFake(mockStorage.removeItem);
		spyOn(sessionStorage, 'clear').and.callFake(mockStorage.clear);

		service = TestBed.get(DummyHttpService);
		httpMock = TestBed.get(HttpTestingController);
	});

	it('should attach business unit id to request', () => {
		mockStorage.setItem('buid', 'foo');
		service.dummy().subscribe(() => {});
		const { request } = httpMock.expectOne('/dummy');
		expect(request.headers.get('X-Business-Unit-Filter')).toBe('foo');
	});

	it('should only attach header if business unit is selected', () => {
		mockStorage.removeItem('buid');
		service.dummy().subscribe(() => {});
		const { request } = httpMock.expectOne('/dummy');
		expect(request.headers.get('X-Business-Unit-Filter')).toBe(null);
	});
});
