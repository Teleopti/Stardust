import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PeopleModule } from '../../people.module';
import { FormFieldInputComponent } from './form-field-input.component';

describe('FormFieldInputComponent', () => {
	let component: FormFieldInputComponent;
	let fixture: ComponentFixture<FormFieldInputComponent>;

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [PeopleModule]
			}).compileComponents();
		})
	);

	beforeEach(() => {
		fixture = TestBed.createComponent(FormFieldInputComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
