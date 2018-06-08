import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite } from '../../../../configure-test-suit';
import { PeopleModule } from '../../people.module';
import { IdentityLogonPageComponent } from './identity-logon-page.component';

describe('AppLogonPageComponent', () => {
	let component: IdentityLogonPageComponent;
	let fixture: ComponentFixture<IdentityLogonPageComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [PeopleModule]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(IdentityLogonPageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
