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

	jobStatus: any;
	defaultLastJobStatus = {
		"SchedulingStatus":{"HasJob":false},
		"IntradayOptimizationStatus":{"HasJob":false},
		"ClearScheduleStatus":{"HasJob":false}
	};
	
	constructor(){
		this.jobStatus = this.defaultLastJobStatus;
	}
	

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
		return of(this.jobStatus);
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

fdescribe('Planning Period Overview', () => {
	let component: PlanningPeriodOverviewComponent;
	let fixture: ComponentFixture<PlanningPeriodOverviewComponent>;
	let page: PlanningPeriodOverviewPage;
	let planningPeriodActionService: PlanningPeriodActionService;
	let planningPeriodService: MockPlanningPeriodService;

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
		planningPeriodService = TestBed.get(PlanningPeriodService);
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(PlanningPeriodOverviewComponent);
		component = fixture.componentInstance;
		page = new PlanningPeriodOverviewPage(fixture);
		
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});

	it('should launch schedule', () =>{
		fixture.detectChanges();
		const spyPlanningPeriodActionService = spyOn(planningPeriodActionService, 'launchScheduling').and.returnValue(of());
		
		component.launchSchedule();
		expect(spyPlanningPeriodActionService.calls.argsFor(0)[0]).toEqual('a557210b-99cc-4128-8ae0-138d812974b6');
		expect(component.isDisabled()).toEqual(true);
	});

	it('should check progress and return schedule is running', function() {
		component.runScheduling = true;
		expect(component.isDisabled()).toEqual(true);
		
		planningPeriodService.jobStatus = {
			SchedulingStatus: {
				Failed: false,
				HasJob: true,
				Successful: false
			}
		};
		fixture.detectChanges();

		expect(component.status).toEqual('PresentTenseSchedule');
		expect(component.isDisabled()).toEqual(true);
	});

	it('should check progress and return schedule is done with success', function() {
		component.runScheduling = true;
		expect(component.isDisabled()).toEqual(true);
		
		planningPeriodService.jobStatus = {
			SchedulingStatus: {
				Failed: false,
				HasJob: true,
				Successful: true
			}
		};
		fixture.detectChanges();
		
		expect(component.isDisabled()).toEqual(false);
	});

	it('should check progress and return schedule is failed', function() {
		component.runScheduling = true;
		expect(component.isDisabled()).toEqual(true);

		planningPeriodService.jobStatus = {
			SchedulingStatus: {
				Failed: true,
				HasJob: true,
				Successful: false
			} 
		};
		fixture.detectChanges();

		expect(component.isDisabled()).toEqual(false);
	});

	it('should launch intraday optimization', () =>{
		fixture.detectChanges();
		const spyPlanningPeriodActionService = spyOn(planningPeriodActionService, 'optimizeIntraday').and.returnValue(of());
		component.optimizeIntraday();

		expect(spyPlanningPeriodActionService.calls.argsFor(0)[0]).toEqual('a557210b-99cc-4128-8ae0-138d812974b6');
		expect(component.isDisabled()).toEqual(true);
	});

	it('should check intraday optimization progress and return intraday optimization is done with success', function() {
		component.runIntraday = true;

		planningPeriodService.jobStatus = {
			IntradayOptimizationStatus: {
				Failed: false,
				HasJob: true,
				Successful: true
			}
		};
		fixture.detectChanges();

		expect(component.isDisabled()).toEqual(false);
	});

	it('should check intraday optimization progress and return intraday optimization is failed', function() {
		component.runIntraday = true;

		planningPeriodService.jobStatus = {
			IntradayOptimizationStatus: {
				Failed: true,
				HasJob: true,
				Successful: false
			}
		};
		fixture.detectChanges();

		expect(component.isDisabled()).toEqual(false);
	});

    it('should launch clear schedule', () => {
		fixture.detectChanges();
		const spyPlanningPeriodActionService = spyOn(planningPeriodActionService, 'clearSchedule').and.returnValue(of());
		component.clearSchedule();

		expect(spyPlanningPeriodActionService.calls.argsFor(0)[0]).toEqual('a557210b-99cc-4128-8ae0-138d812974b6');
		expect(component.isDisabled()).toEqual(true);
    });

	it('should check clear schedule progress and return clear schedule is done with success', function() {
		component.runClear = true;

		planningPeriodService.jobStatus = {
			ClearScheduleStatus: {
				Failed: false,
				HasJob: true,
				Successful: true
			}
		};
		fixture.detectChanges();

		expect(component.isDisabled()).toEqual(false);
	});

	it('should check clear schedule progress and return clear schedule is failed', function() {
		component.runClear = true;

		planningPeriodService.jobStatus = {
			ClearScheduleStatus: {
				Failed: true,
				HasJob: true,
				Successful: false
			}
		};
		fixture.detectChanges();

		expect(component.isDisabled()).toEqual(false);
	});

	it('should launch publish', () => {
		fixture.detectChanges();
		const spyPlanningPeriodActionService = spyOn(planningPeriodActionService, 'publishSchedule').and.returnValue(of());
		component.publishSchedule();

		expect(spyPlanningPeriodActionService.calls.argsFor(0)[0]).toEqual('a557210b-99cc-4128-8ae0-138d812974b6');
		expect(component.isDisabled()).toEqual(true);
	});
	
});




class PlanningPeriodOverviewPage extends PageObject {
	get filteredSkillNames() {
		return this.queryAll('.skill-name');
	}
}