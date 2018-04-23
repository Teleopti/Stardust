import { HttpClientModule } from '@angular/common/http';
import { DebugElement } from '@angular/core';
import { ComponentFixture, TestBed, async } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { PeopleModule } from '../../people.module';
import { WorkspaceService, WorkspaceServiceStub } from '../../services';
import { ROLES, fakeBackendProvider } from '../../services/fake-backend.provider';
import { GrantPageComponent } from './grant-page.component';

describe('GrantPage', () => {
	let component: GrantPageComponent;
	let fixture: ComponentFixture<GrantPageComponent>;
	let page: Page;

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			imports: [PeopleModule, HttpClientModule],
			providers: [{ provide: WorkspaceService, useClass: WorkspaceServiceStub }, fakeBackendProvider]
		}).compileComponents();
	}));

	beforeEach(async(async () => {
		fixture = TestBed.createComponent(GrantPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);

		fixture.whenStable().then(async () => {
			component.roles = ROLES;

			fixture.detectChanges();
		});
	}));

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should show current roles', () => {
		fixture.detectChanges();
		expect(page.currentChips.length).toEqual(2);
	});

	it('should only show chips from selected people', () => {
		fixture.detectChanges();
		expect(page.currentChips.length).toEqual(2);

		const firstPerson = component.workspaceService.getSelectedPeople().getValue()[0];
		component.workspaceService.deselectPerson(firstPerson);
		fixture.detectChanges();

		expect(page.currentChips.length).toEqual(1);
	});
});

class Page {
	get currentChips() {
		return this.queryAll('[data-test-grant-current] [data-test-chip]');
	}

	get availableChips() {
		return this.queryAll('[data-test-grant-available] [data-test-chip]');
	}

	fixture: ComponentFixture<GrantPageComponent>;

	constructor(fixture: ComponentFixture<GrantPageComponent>) {
		this.fixture = fixture;
	}

	private queryAll(selector: string): DebugElement[] {
		return this.fixture.debugElement.queryAll(By.css(selector));
	}
}
