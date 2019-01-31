import {Component, Inject} from '@angular/core';
import {PlanningPeriodService} from "../../shared";
import {IStateService} from "angular-ui-router";
import {TranslateService} from "@ngx-translate/core";

@Component({
	selector: 'plans-period-overview',
	templateUrl: './planning-period-overview.component.html',
	styleUrls: ['./planning-period-overview.component.scss'],
	providers: []
})
export class PlanningPeriodOverviewComponent {

	ppId: string;
	schedulingPerformed: boolean = false;
	status: string='';
	isScheduled: boolean = false;
	scheduledAgents: number = 0;
	timer: any;
	
	valLoading: boolean = true;

	dayNodes;

	valData = {
		totalValNum: 0,
		totalPreValNum: 0,
		totalLastValNum: 0,
		scheduleIssues: [],
		preValidation: []
	};

	constructor(private planningPeriodService: PlanningPeriodService, @Inject('$state') private $state: IStateService, private translate: TranslateService) {
		this.ppId = $state.params.ppId.trim();
	}

	ngOnInit(){
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
		this.planningPeriodService.launchScheduling(this.ppId).subscribe(()=>{
			this.checkProgress();
		});
	}
	
	private checkProgress = ()=>{
		this.planningPeriodService.lastJobStatus(this.ppId).subscribe((data)=>{
			let schedulingStatus = data.SchedulingStatus;
			if (!schedulingStatus || !schedulingStatus.HasJob) {
				this.schedulingPerformed = false;
			} else {
				if (!schedulingStatus.Successful && !schedulingStatus.Failed) {
					this.schedulingPerformed = true;
					this.status = this.translate.instant('PresentTenseSchedule');
					return;
				}
				if (schedulingStatus.Failed) {
					this.schedulingPerformed = false;
				}
				if (schedulingStatus.Successful && this.schedulingPerformed) {
					this.schedulingPerformed = false;
					this.loadLastResult();
				}
			}
		});
	};
	
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
}
