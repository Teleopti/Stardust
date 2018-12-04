import { DOCUMENT } from '@angular/common';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Theme, ThemeService } from './theme.service';

const createMockDocument = (): Document => {
	const doc = document.implementation.createHTMLDocument();
	const styleguide = doc.createElement('link');
	styleguide.id = 'themeStyleguide';
	styleguide.rel = 'stylesheet';
	const ant = doc.createElement('link');
	ant.id = 'themeAnt';
	ant.rel = 'stylesheet';
	doc.body.appendChild(styleguide);
	doc.body.appendChild(ant);
	return doc;
};

describe('ThemeService', () => {
	let themeService: ThemeService;
	let httpMock: HttpTestingController;
	let document: Document;

	beforeEach(() => {
		TestBed.configureTestingModule({
			imports: [HttpClientTestingModule],
			providers: [{ provide: DOCUMENT, useValue: createMockDocument() }, ThemeService]
		});

		themeService = TestBed.get(ThemeService);
		httpMock = TestBed.get(HttpTestingController);
		document = TestBed.get(DOCUMENT);
	});

	it('should return current theme', () => {
		const theme: Theme = {
			Name: 'classic',
			Overlay: false
		};
		httpMock.expectOne('../api/Theme').flush(theme);
		expect(themeService.getCurrentTheme()).toBe('classic');
	});
});
