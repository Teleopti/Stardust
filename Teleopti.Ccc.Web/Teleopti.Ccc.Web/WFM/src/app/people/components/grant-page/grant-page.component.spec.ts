import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GrantPageComponent } from './grant-page.component';
import { PeopleModule } from '../../people.module';
import { DebugElement, Component, OnInit, ViewChild } from '@angular/core';
import { By } from '@angular/platform-browser';
import { RolesService, RolesServiceStub, WorkspaceService, WorkspaceServiceStub, SearchService, SearchServiceStub } from '../../services';
import { Person, Role } from '../../types';

describe('GrantPage', () => {
	let component: GrantPageComponent;
	let fixture: ComponentFixture<GrantPageComponent>;
	const GRANT_CURRENT_CHIP_QUERY = '[data-test-grant-current] [data-test-chip]';
	const GRANT_AVAILABLE_CHIP_QUERY = '[data-test-grant-available] [data-test-chip]';
	const GRANT_SAVE_QUERY = '[data-test-grant-save]';
	const WORKSPACE_PERSON_QUERY = '[data-test-workspace] [data-test-person]';
	const WORKSPACE_PERSON_REMOVE = '[data-test-workspace] [data-test-person] [data-test-person-remove]';

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [PeopleModule],
				providers: [
					{ provide: WorkspaceService, useValue: new WorkspaceServiceStub() }
				]
			}).compileComponents();
		})
	);

	beforeEach(async(async () => {
		fixture = TestBed.createComponent(GrantPageComponent);
		component = fixture.componentInstance;

		fixture.whenStable().then(async () => {
			let searchService = new SearchServiceStub();
			let rolesService = new RolesServiceStub();
			let people = await searchService.getPeople();
			let roles = await rolesService.getRoles();

			component.roles = roles;

			fixture.detectChanges();
		});
	}));

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it(
		'should show current roles',
		() => {
			fixture.detectChanges();
			let chipCount = fixture.debugElement.queryAll(By.css(GRANT_CURRENT_CHIP_QUERY)).length;

			expect(chipCount).toEqual(2);
		}
	);

	it(
		'should only show chips from selected people',
		() => {
			fixture.detectChanges();
			const getChipCount = () => fixture.debugElement.queryAll(By.css(GRANT_CURRENT_CHIP_QUERY)).length;

			expect(getChipCount()).toEqual(2);

			component.workspaceService.deselectPerson(component.workspaceService.getSelectedPeople()[0]);
			fixture.detectChanges();

			expect(getChipCount()).toEqual(1);
		}
	);

	it(
		'should return selected roles on save',
		() => {
			fixture.detectChanges();
			let chipNativeElement = fixture.debugElement.query(By.css(GRANT_AVAILABLE_CHIP_QUERY)).nativeElement;
			chipNativeElement.click();
			fixture.detectChanges();

			expect(component.selectedRoles.length).toEqual(1);

			let saveButtonNativeElement = fixture.debugElement.query(By.css(GRANT_SAVE_QUERY)).nativeElement;
			saveButtonNativeElement.click();

			component.onRolesChanged.subscribe((roles: Array<Role>) => expect(roles.length).toBe(1));
			component.save();
		}
	);
});
