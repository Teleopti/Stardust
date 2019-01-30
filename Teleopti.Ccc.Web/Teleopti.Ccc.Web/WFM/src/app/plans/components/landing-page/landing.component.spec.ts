import { async, ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { NzButtonModule, NzDropDownModule, NzInputModule, NzTableModule, NzDividerModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { LandingPageComponent } from './landing.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PlanningGroupService } from '../../shared';
import { NavigationService } from 'src/app/core/services';

class MockPlanningGroupService implements Partial<PlanningGroupService> {
	getPlanningGroups() {
		return of([
			{
				Name: 'planning group',
				Id: '123',
				AgentCount: 44
			},
			{
				Name: 'planning group 2',
				Id: '123',
				AgentCount: 44
			}
		]);
	}
}

fdescribe('Plans LandingPage', () => {
	let component: LandingPageComponent;
	let fixture: ComponentFixture<LandingPageComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [LandingPageComponent],
			imports: [NzInputModule, NzTableModule, ReactiveFormsModule, MockTranslationModule, NzDividerModule],
			providers: [
				{ provide: PlanningGroupService, useClass: MockPlanningGroupService },
				{ provide: NavigationService, useValue: {} }
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(LandingPageComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display planning group rows', () => {
		fixture.detectChanges();
		fixture.whenStable().then(() => {
			fixture.detectChanges();
			expect(page.filteredPlanningGroups.length).toBe(2);
		});
	});

	it('should apply filter', async(() => {
		component.filterControl.setValue('planning group 2');
		fixture.detectChanges();

		fixture.whenStable().then(() => {
			fixture.detectChanges();
			expect(page.filteredPlanningGroups.length).toBe(1);

			const name = page.filteredPlanningGroupsNames[0];
			expect(name.nativeElement.innerText).toBe('planning group 2');
		});
	}));
});

class Page extends PageObject {
	get filteredPlanningGroups() {
		return this.queryAll('tbody tr');
	}
	get filteredPlanningGroupsNames() {
		return this.queryAll('tbody tr [data-test-group-name]');
	}
}
