import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { configureTestSuite } from '../../configure-test-suit';
import { ThemeService } from '../core/services';
import { BootstrapComponent } from './bootstrap.component';

const ThemeServiceMock = {
	provide: ThemeService,
	useValue: {
		getTheme: () => of({ Name: 'classic', Overlay: false })
	}
};

describe('FeedbackMessageComponent', () => {
	let component: BootstrapComponent;
	let fixture: ComponentFixture<BootstrapComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BootstrapComponent],
			imports: [],
			providers: [ThemeServiceMock]
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
