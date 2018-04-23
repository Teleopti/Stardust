import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { IdentityLogonPageComponent } from './identity-logon-page.component';
import { PeopleModule } from '../../people.module';

describe('AppLogonPageComponent', () => {
	let component: IdentityLogonPageComponent;
	let fixture: ComponentFixture<IdentityLogonPageComponent>;

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
