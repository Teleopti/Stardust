import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { MockTranslationModule } from '@wfm/mocks/translation';
import { NzInputModule, NzTableModule, NzDividerModule, NzGridModule } from 'ng-zorro-antd';
import { of } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { NavigationService } from 'src/app/core/services';
import { PlanningGroupService } from '../../shared';
import { TitleBarComponent } from '../../components/title-bar';
import { PlanningPeriodOverviewComponent } from './planning-period-overview.component';

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

fdescribe('Planning Period Overview', () => {
	let component: PlanningPeriodOverviewComponent;
	let fixture: ComponentFixture<PlanningPeriodOverviewComponent>;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [PlanningPeriodOverviewComponent, TitleBarComponent],
			imports: [
				NzInputModule,
				NzTableModule,
				ReactiveFormsModule,
				MockTranslationModule,
				NzDividerModule,
				NzGridModule
			],
			providers: [
				{ provide: PlanningGroupService, useClass: MockPlanningGroupService },
				{ provide: NavigationService, useValue: {} }
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(PlanningPeriodOverviewComponent);
		component = fixture.componentInstance;
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
