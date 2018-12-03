import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite } from '@wfm/test';
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
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
