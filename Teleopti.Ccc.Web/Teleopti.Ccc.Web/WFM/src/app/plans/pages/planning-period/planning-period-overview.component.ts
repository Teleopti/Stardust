import {Component, Inject} from '@angular/core';
import {PlanningPeriodService} from "../../shared";
import {IStateService} from "angular-ui-router";
import {TranslateService} from "@ngx-translate/core";
import {NavigationService} from "../../../core/services";

@Component({
	selector: 'plans-period-overview',
	templateUrl: './planning-period-overview.component.html',
	styleUrls: ['./planning-period-overview.component.scss'],
	providers: []
})
export class PlanningPeriodOverviewComponent {

	ppId: string;
	groupId: string;
	runScheduling: boolean = false;
	runIntraday: boolean = false;
	runClear: boolean = false;
	runPublish: boolean = false;
	status: string='';
	isScheduled: boolean = false;
	scheduledAgents: number = 0;
	timer: any;
	planningPeriodInfo: any;
	totalAgents: number = 0;
	valLoading: boolean = true;

	validationFilter;
	

	dayNodes;

	valData = {
		totalValNum: 0,
		totalPreValNum: 0,
		totalLastValNum: 0,
		scheduleIssues: [],
		preValidation: []
	};

	constructor(private planningPeriodService: PlanningPeriodService, @Inject('$state') private $state: IStateService, private translate: TranslateService, private navService: NavigationService) {
		this.ppId = $state.params.ppId.trim();
		this.groupId = $state.params.groupId.trim();
	}

	ngOnInit(){
		this.loadPlanningPeriodInfo();
		this.loadValidations();
		this.loadLastResult();
		this.checkState();
	}

	ngOnDestroy(): void {
		clearInterval(this.timer);
	}

	private checkState() {
		this.checkProgress();
		this.timer = setInterval(this.checkProgress, 10000);
	}
	
	public launchSchedule(){
		this.runScheduling = true;
		this.status = this.translate.instant('PresentTenseSchedule');
		this.planningPeriodService.launchScheduling(this.ppId).subscribe(()=>{
			this.checkProgress();
		});
	}
	
	public optimizeIntraday(){
		this.runIntraday = true;
		this.status = this.translate.instant('IntraOptimize');
		this.planningPeriodService.optimizeIntraday(this.ppId).subscribe(()=>{
			this.checkProgress();
		});
	}
	
	public clearSchedule(){
		this.runClear = true;
		this.status = this.translate.instant('ClearScheduleResultAndHistoryData');
		this.planningPeriodService.clearSchedule(this.ppId).subscribe(()=>{
			this.checkProgress();
		});
	}

	public publishSchedule(){
		this.runPublish = true;
		this.planningPeriodService.publishSchedule(this.ppId).subscribe(()=>{
			this.runPublish = false;
		});
	}

	public editPlanningGroup() {
		this.navService.go('resourceplanner.editplanninggroup', { groupId: this.groupId });
	}
	
	public isDisabled(){
		if (this.runScheduling || this.runClear || this.runIntraday || this.runPublish)
		{
			return true;
		}
	}
	
	private checkProgress = ()=>{
		this.planningPeriodService.lastJobStatus(this.ppId).subscribe((data)=>{
			let schedulingStatus = data.SchedulingStatus;
			if (!schedulingStatus || !schedulingStatus.HasJob) {
				this.runScheduling = false;
			} else {
				if (!schedulingStatus.Successful && !schedulingStatus.Failed) {
					this.runScheduling = true;
					this.status = this.translate.instant('PresentTenseSchedule');
					return;
				}
				if (schedulingStatus.Failed) {
					this.runScheduling = false;
					return;
				}
				if (schedulingStatus.Successful && this.runScheduling) {
					this.runScheduling = false;
					this.loadLastResult();
					return;
				}
			}

			let clearScheduleStatus = data.ClearScheduleStatus;
			if (!clearScheduleStatus || !clearScheduleStatus.HasJob) {
				this.runClear = false;
			} else {
				if (!clearScheduleStatus.Successful && !clearScheduleStatus.Failed) {
					this.runClear = true;
					this.status = this.translate.instant('ClearScheduleResultAndHistoryData');
					return;
				}
				if (clearScheduleStatus.Successful && this.runClear) {
					this.runClear = false;
					this.isScheduled = false;
					this.scheduledAgents = 0;
					this.dayNodes = undefined;
					this.status = '';
					return;
				}
				if (clearScheduleStatus.Failed) {
					this.runClear = false;
					this.status = '';
					return;
				}
			}

			let intradayOptimizationStatus = data.IntradayOptimizationStatus;
			if (!intradayOptimizationStatus || !intradayOptimizationStatus.HasJob) {
				this.runIntraday = false;
			} else {
				if (!intradayOptimizationStatus.Successful && !intradayOptimizationStatus.Failed) {
					this.runIntraday = true;
					this.status = this.translate.instant('IntraOptimize');
					return;
				}
				if (intradayOptimizationStatus.Successful && this.runIntraday) {
					this.runIntraday = false;
					this.status = '';
					this.loadLastResult();
					return;
				}
				if (intradayOptimizationStatus.Failed) {
					this.runIntraday = false;
					this.status = '';
					return;
				}
			}
		});
	};
	
	private loadPlanningPeriodInfo(){
		this.planningPeriodService.getPlanningPeriodInfo(this.ppId).subscribe(data=>{
			this.planningPeriodInfo = data?data:{};
			this.totalAgents = data? data.TotalAgents: 0;
		});
	}
	
	private updateValidationErrorsNumber(){
		this.valData.totalValNum = 0;
		this.valData.totalPreValNum = 0;
		this.valData.totalLastValNum = 0;
		let pre = this.valData.preValidation;
		let after = this.valData.scheduleIssues;
		if (pre.length > 0) {
			pre.forEach(item =>{
				if (item.ValidationErrors !== null) this.valData.totalPreValNum += item.ValidationErrors.length;
			});
		}
		if (after.length > 0) {
			after.forEach(item=>{
				if (item.ValidationErrors !== null) this.valData.totalLastValNum += item.ValidationErrors.length;
			});
		}
		this.valData.totalValNum = this.valData.totalPreValNum + this.valData.totalLastValNum;
	}
	
	private loadLastResult(){
		this.planningPeriodService.lastJobResult(this.ppId).subscribe(data=>{
			let fullSchedulingResult = data.FullSchedulingResult;
			if (fullSchedulingResult) {
				this.isScheduled = true;
				this.scheduledAgents = data.FullSchedulingResult.ScheduledAgentsCount;
				this.valData.scheduleIssues = data.FullSchedulingResult.BusinessRulesValidationResults;
				this.updateValidationErrorsNumber();
				if (!fullSchedulingResult) return;
				this.dayNodes = fullSchedulingResult.SkillResultList ? fullSchedulingResult.SkillResultList : undefined;
			} else {
				this.isScheduled = false;
			}
		});
	}
	
	private loadValidations(){
		this.valLoading = true;
		this.planningPeriodService.getValidation(this.ppId).subscribe(data => {
			this.valData.preValidation = data.InvalidResources;
			this.valLoading = false;
			this.updateValidationErrorsNumber();
		});
	}
}
