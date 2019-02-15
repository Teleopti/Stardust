import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { NzInputModule, NzTableModule, NzDividerModule, NzGridModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NavigationService } from 'src/app/core/services';
import { PlanningGroupsOverviewComponent } from './planning-groups-overview.component';
import { PlanningGroupService } from '../../shared';
import { TitleBarComponent } from '../../components/title-bar';
import {HttpClientTestingModule} from "@angular/common/http/testing";

class MockPlanningGroupService implements Partial<PlanningGroupService> {
	getPlanningGroups() {
		return of([
			{
				Name: 'B',
				Id: '123',
				AgentCount: 44
			},
			{
				Name: 'a',
				Id: '123',
				AgentCount: 44
			},
			{
				Name: 'A',
				Id: '123',
				AgentCount: 44
			}
		]);
	}
}

describe('Planning Group Overview', () => {
	let component: PlanningGroupsOverviewComponent;
	let fixture: ComponentFixture<PlanningGroupsOverviewComponent>;
	let page: Page;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [PlanningGroupsOverviewComponent, TitleBarComponent],
			imports: [
				NzInputModule,
				NzTableModule,
				ReactiveFormsModule,
				MockTranslationModule,
				NzDividerModule,
				NzGridModule,
				HttpClientTestingModule
			],
			providers: [
				{ provide: PlanningGroupService, useClass: MockPlanningGroupService },
				{ provide: NavigationService, useValue: {} }
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(PlanningGroupsOverviewComponent);
		component = fixture.componentInstance;
		page = new Page(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should display planning group rows', async(() => {
		fixture.whenStable().then(() => {
			fixture.detectChanges();
			expect(page.filteredPlanningGroups.length).toBe(3);
		});
	}));

	it('should sort planning groups', async(() => {
		fixture.whenStable().then(() => {
			fixture.detectChanges();
			expect(page.filteredPlanningGroupsNames[2].nativeElement.innerText).toBe('B');
		});
	}));

	it('should apply filter', async(() => {
		component.filterControl.setValue('b');
		fixture.detectChanges();

		fixture.whenStable().then(() => {
			fixture.detectChanges();
			expect(page.filteredPlanningGroups.length).toBe(1);

			const name = page.filteredPlanningGroupsNames[0];
			expect(name.nativeElement.innerText).toBe('B');
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
