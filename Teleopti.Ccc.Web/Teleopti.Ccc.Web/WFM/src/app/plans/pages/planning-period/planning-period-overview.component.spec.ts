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
import { of } from 'rxjs';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import { NavigationService } from 'src/app/core/services';
import {PlanningGroupService, PlanningPeriodService} from '../../shared';
import { TitleBarComponent } from '../../components/title-bar';
import { PlanningPeriodOverviewComponent } from './planning-period-overview.component';
import {DateFormatPipe, MomentModule} from "ngx-moment";
import {IStateService} from "angular-ui-router";
import {HeatMapColorHelper} from "../../shared/heatmapcolor.service";

class MockPlanningGroupService implements Partial<PlanningGroupService> {
	getPlanningGroups() {
		return of([
		]);
	}
}

class MockPlanningPeriodService implements Partial<PlanningPeriodService> {
	lastJobResult() {
		return of();
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
				FormsModule
			], 
			providers: [
				{ provide: PlanningGroupService, useClass: MockPlanningGroupService },
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
	}));

	beforeEach(() => {
		fixture = TestBed.createComponent(PlanningPeriodOverviewComponent);
		component = fixture.componentInstance;
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
