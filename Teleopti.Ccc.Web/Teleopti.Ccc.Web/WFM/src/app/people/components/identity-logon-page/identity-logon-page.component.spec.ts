import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AppLogonPageComponent } from './app-logon-page.component';
import { PeopleModule } from '../../people.module';

describe('AppLogonPageComponent', () => {
	let component: AppLogonPageComponent;
	let fixture: ComponentFixture<AppLogonPageComponent>;

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [PeopleModule]
			}).compileComponents();
		})
	);

	beforeEach(() => {
		fixture = TestBed.createComponent(AppLogonPageComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
