import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PeopleComponent } from './people.component';
import { RolesService } from './services';
import { PeopleModule } from './people.module';

const rolesServiceStub = {
	async getPeople() {
		return [];
	},

	async getRoles() {
		return [];
	}
};

fdescribe('PeopleComponent', () => {
	let component: PeopleComponent;
	let fixture: ComponentFixture<PeopleComponent>;

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [PeopleModule],
				providers: [{ provide: RolesService, useValue: rolesServiceStub }]
			}).compileComponents();
		})
	);

	beforeEach(() => {
		fixture = TestBed.createComponent(PeopleComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
