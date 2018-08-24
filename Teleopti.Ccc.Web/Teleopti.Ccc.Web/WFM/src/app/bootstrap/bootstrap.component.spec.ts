import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite } from '../../configure-test-suit';
import { BootstrapComponent } from './bootstrap.component';

describe('FeedbackMessageComponent', () => {
	let component: BootstrapComponent;
	let fixture: ComponentFixture<BootstrapComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [BootstrapComponent],
			imports: []
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
