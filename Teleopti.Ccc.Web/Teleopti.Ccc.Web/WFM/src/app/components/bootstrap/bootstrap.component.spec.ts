import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { Observable, of } from 'rxjs';
import { Theme, ThemeService } from '../../core/services';
import { BootstrapComponent } from './bootstrap.component';

class ThemeServiceMock implements Partial<ThemeService> {
	get theme$(): Observable<Theme> {
		return of({ Name: 'classic', Overlay: false } as Theme);
	}
}

describe('BootstrapComponent', () => {
	let component: BootstrapComponent;
	let fixture: ComponentFixture<BootstrapComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BootstrapComponent],
			imports: [],
			providers: [{ provide: ThemeService, useClass: ThemeServiceMock }]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(BootstrapComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should be able to use low light overlay', () => {
		expect(page.overlayElement).toBeFalsy();
		component.lowLightFilter = true;
		fixture.detectChanges();
		expect(page.overlayElement).toBeTruthy();
	});
});

class Page extends PageObject {
	get overlayElement() {
		return this.queryAll('.warm-overlay')[0];
	}
}
