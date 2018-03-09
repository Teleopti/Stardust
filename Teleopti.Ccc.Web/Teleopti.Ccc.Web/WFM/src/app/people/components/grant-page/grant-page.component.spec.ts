import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GrantPageComponent } from './grant-page.component';
import { PeopleModule } from '../../people.module';
import { DebugElement, Component, OnInit, ViewChild } from '@angular/core';
import { By } from '@angular/platform-browser';
import { RolesService, WorkspaceService, WorkspaceServiceStub, SearchService } from '../../services';
import { Person, Role } from '../../types';
import { ROLES, fakeBackendProvider } from '../../services/fake-backend.provider';
import { HttpClientModule } from '@angular/common/http';

describe('GrantPage', () => {
	let component: GrantPageComponent;
	let fixture: ComponentFixture<GrantPageComponent>;
	const GRANT_CURRENT_CHIP_QUERY = '[data-test-grant-current] [data-test-chip]';
	const GRANT_AVAILABLE_CHIP_QUERY = '[data-test-grant-available] [data-test-chip]';
	const GRANT_SAVE_QUERY = '[data-test-save]';
	const WORKSPACE_PERSON_QUERY = '[data-test-workspace] [data-test-person]';
	const WORKSPACE_PERSON_REMOVE = '[data-test-workspace] [data-test-person] [data-test-person-remove]';

	beforeEach(
		async(() => {
			TestBed.configureTestingModule({
				imports: [PeopleModule, HttpClientModule],
				providers: [{ provide: WorkspaceService, useClass: WorkspaceServiceStub }, fakeBackendProvider]
			}).compileComponents();
		})
	);

	beforeEach(
		async(async () => {
			fixture = TestBed.createComponent(GrantPageComponent);
			component = fixture.componentInstance;

			fixture.whenStable().then(async () => {
				component.roles = ROLES;

				fixture.detectChanges();
			});
		})
	);

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should show current roles', () => {
		fixture.detectChanges();
		let chipCount = fixture.debugElement.queryAll(By.css(GRANT_CURRENT_CHIP_QUERY)).length;

		expect(chipCount).toEqual(2);
	});

	it('should only show chips from selected people', () => {
		fixture.detectChanges();
		const getChipCount = () => fixture.debugElement.queryAll(By.css(GRANT_CURRENT_CHIP_QUERY)).length;

		expect(getChipCount()).toEqual(2);

		component.workspaceService.deselectPerson(component.workspaceService.getSelectedPeople()[0]);
		fixture.detectChanges();

		expect(getChipCount()).toEqual(1);
	});

	it('should return selected roles on save', () => {
		fixture.detectChanges();
		let chipNativeElement = fixture.debugElement.query(By.css(GRANT_AVAILABLE_CHIP_QUERY)).nativeElement;
		chipNativeElement.click();
		fixture.detectChanges();

		expect(component.selectedRoles.length).toEqual(1);

		let saveButtonNativeElement = fixture.debugElement.query(By.css(GRANT_SAVE_QUERY)).nativeElement;
		saveButtonNativeElement.click();

		component.onRolesChanged.subscribe((roles: Array<Role>) => expect(roles.length).toBe(1));
		component.save();
	});
});
