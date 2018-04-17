import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaygroundComponent } from './playground.component';
import { PeopleModule } from '../../people.module';

describe('PlaygroundComponent', () => {
	let component: PlaygroundComponent;
	let fixture: ComponentFixture<PlaygroundComponent>;

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [PeopleModule]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(PlaygroundComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
