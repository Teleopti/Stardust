import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PeopleComponent } from './people.component';
import { PeopleModule } from './people.module';
import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { fakeBackendProvider } from './services/fake-backend.provider';
import { HttpClientModule } from '@angular/common/http';

describe('PeopleComponent', () => {
	let component: PeopleComponent;
	let fixture: ComponentFixture<PeopleComponent>;

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [PeopleModule, HttpClientModule],
				providers: [fakeBackendProvider]
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
