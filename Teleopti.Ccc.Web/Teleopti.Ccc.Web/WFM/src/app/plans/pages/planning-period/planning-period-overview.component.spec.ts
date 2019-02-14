import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { configureTestSuite, PageObject } from '@wfm/test';
import { MockTranslationModule } from '@wfm/mocks/translation';
import {
	NzInputModule,
	NzTableModule,
	NzDividerModule,
	NzGridModule,
	NzTabsModule,
	NzSpinModule,
	NzCollapseModule, NzBadgeModule, NzToolTipModule, NzSwitchModule
} from 'ng-zorro-antd';
import {of} from 'rxjs';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import { NavigationService } from 'src/app/core/services';
import {PlanningGroupService, PlanningPeriodService} from '../../shared';
import { TitleBarComponent } from '../../components/title-bar';
import { PlanningPeriodOverviewComponent } from './planning-period-overview.component';
import {DateFormatPipe, MomentModule} from "ngx-moment";
import {IStateService} from "angular-ui-router";
import {HeatMapColorHelper} from "../../shared/heatmapcolor.service";
import {PlanningPeriodActionService} from "../../shared/planningperiod.action.service";
import {HttpClientTestingModule} from "@angular/common/http/testing";

class MockPlanningGroupService implements Partial<PlanningGroupService> {
	getPlanningGroup(groupId: string) {
		return of({
			Name: 'a',
			Id: groupId,
			AgentCount: 44
		});
	}
}

class MockPlanningPeriodService implements Partial<PlanningPeriodService> {

	public getPlanningPeriodInfo(planningPeriodId: string){
		return of({
			"Id":planningPeriodId,
			"StartDate":"2018-05-28T00:00:00",
			"EndDate":"2018-06-24T00:00:00",
			"HasNextPlanningPeriod":true,
			"State":"Scheduled",
			"PlanningGroupId":"aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e",
			"TotalAgents":212,
			"Number":4,
			"Type":"Week"}
			);
	}

	public getValidation(planningPeriodId: string) {
		return of({
			"InvalidResources": []
		});
	}

	public lastJobStatus(planningPeriodId: string) {
		return of({
			"SchedulingStatus":{"HasJob":false},
			"IntradayOptimizationStatus":{"HasJob":false},
			"ClearScheduleStatus":{"HasJob":false}
		});
	}
	
	public lastJobResult() {
		return of({
			FullSchedulingResult:{
				SkillResultList:[
					{
						"SkillName": "Channel Support",
						"SkillDetails": [{
							"Date": "2018-05-28T00:00:00",
							"RelativeDifference": -1,
							"ColorId": 3
						}, {
							"Date": "2018-05-29T00:00:00",
							"RelativeDifference": 0,
							"ColorId": 4
						}
						]
					}, 
					{
						"SkillName": "Direct Support",
						"SkillDetails": [{
							"Date": "2018-05-28T00:00:00",
							"RelativeDifference": -0.6,
							"ColorId": 3
						}, {
							"Date": "2018-05-29T00:00:00",
							"RelativeDifference": -0.6,
							"ColorId": 3
						}
						]
					}
				],
				BusinessRulesValidationResults: []
			}
		});
	}
}

const mockStateService: Partial<IStateService> = {
	params: {
		groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e', 
		ppId: 'a557210b-99cc-4128-8ae0-138d812974b6'
	}
};

describe('Planning Period Overview', () => {
	let component: PlanningPeriodOverviewComponent;
	let fixture: ComponentFixture<PlanningPeriodOverviewComponent>;
	let page: PlanningPeriodOverviewPage;
	let planningPeriodActionService: PlanningPeriodActionService;

	configureTestSuite();

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [PlanningPeriodOverviewComponent, TitleBarComponent],
			imports: [
				NzInputModule,
				NzTableModule,
				NzTabsModule,
				NzSpinModule,
				NzBadgeModule,
				NzCollapseModule,
				ReactiveFormsModule,
				MockTranslationModule,
				NzDividerModule,
				NzGridModule,
				NzToolTipModule,
				MomentModule,
				NzSwitchModule,
				FormsModule,
				HttpClientTestingModule
			], 
			providers: [
				{ provide: PlanningGroupService, useClass: MockPlanningGroupService },
				PlanningPeriodActionService,
				{ provide: PlanningPeriodService, useClass: MockPlanningPeriodService },
				{ provide: HeatMapColorHelper, useClass: HeatMapColorHelper },
				{ provide: DateFormatPipe, useClass: DateFormatPipe },
				{
					provide: '$state',
					useValue: mockStateService
				},
				{ provide: NavigationService, useValue: {} }
			]
		}).compileComponents();

		planningPeriodActionService = TestBed.get(PlanningPeriodActionService);
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(PlanningPeriodOverviewComponent);
		component = fixture.componentInstance;
		page = new PlanningPeriodOverviewPage(fixture);
		fixture.detectChanges();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should launch schedule', () =>{
		const spyPlanningPeriodActionService = spyOn(planningPeriodActionService, 'launchScheduling').and.returnValue(of());
		
		component.launchSchedule();
		expect(spyPlanningPeriodActionService.calls.argsFor(0)[0]).toEqual('a557210b-99cc-4128-8ae0-138d812974b6');
		expect(component.isDisabled()).toEqual(true);
	});

	it('should launch intraday optimization', function() {
		const spyPlanningPeriodActionService = spyOn(planningPeriodActionService, 'optimizeIntraday').and.returnValue(of());
		component.optimizeIntraday();

		expect(spyPlanningPeriodActionService.calls.argsFor(0)[0]).toEqual('a557210b-99cc-4128-8ae0-138d812974b6');
		expect(component.isDisabled()).toEqual(true);
	});

    it('should launch clear schedule', function() {
		const spyPlanningPeriodActionService = spyOn(planningPeriodActionService, 'clearSchedule').and.returnValue(of());
		component.clearSchedule();

		expect(spyPlanningPeriodActionService.calls.argsFor(0)[0]).toEqual('a557210b-99cc-4128-8ae0-138d812974b6');
		expect(component.isDisabled()).toEqual(true);
    });
});




class PlanningPeriodOverviewPage extends PageObject {
	get filteredSkillNames() {
		return this.queryAll('.skill-name');
	}
}