import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import {PlanningGroupService, PlanningPeriodService} from '../../shared';
import { IStateService } from 'angular-ui-router';
import { TranslateService } from '@ngx-translate/core';
import { NavigationService } from '../../../core/services';
import { FormBuilder, FormControl } from '@angular/forms';
import { map } from 'rxjs/operators';

@Component({
	selector: 'plans-period-overview',
	templateUrl: './planning-period-overview.component.html',
	styleUrls: ['./planning-period-overview.component.scss'],
	providers: [],
})
export class PlanningPeriodOverviewComponent implements OnInit, OnDestroy {
	preValidationFilterControl: FormControl = this.fb.control('');
	scheduleIssuesFilterControl: FormControl = this.fb.control('');
	ppId: string;
	groupId: string;
	runScheduling = false;
	runIntraday = false;
	runClear = false;
	runPublish = false;
	status = '';
	isScheduled = false;
	scheduledAgents = 0;
	timer: any;
	planningPeriodInfo: any = {};
	planningGroupInfo: any = {};
	totalAgents = 0;
	valLoading = true;
	filteredPreValidations: any[];
	filteredScheduleIssues: any[];

	validationFilter;

	dayNodes;

	valData = {
		totalValNum: 0,
		totalPreValNum: 0,
		totalLastValNum: 0,
		scheduleIssues: [],
		preValidation: []
	};

	constructor(
		private planningPeriodService: PlanningPeriodService,
		private planningGroupService: PlanningGroupService,
		@Inject('$state') private $state: IStateService,
		private translate: TranslateService,
		private navService: NavigationService,
		private fb: FormBuilder
	) {
		this.ppId = $state.params.ppId.trim();
		this.groupId = $state.params.groupId.trim();
	}

	ngOnInit() {
		this.loadPlanningGroupInfo();
		this.loadPlanningPeriodInfo();
		this.loadValidations();
		this.loadLastResult();
		this.checkState();

		this.preValidationFilterControl.valueChanges
			.pipe(
				map(filterString => {
					return this.valData.preValidation.filter(
						g =>
							g.ResourceName.toLowerCase().includes(filterString.toLowerCase()) ||
							g.ValidationErrors.some(
								item =>
									item.ErrorMessageLocalized.toLowerCase().includes(filterString.toLowerCase()) ||
									this.translate
										.instant(item.ResourceType.toLowerCase())
										.includes(filterString.toLowerCase())
							)
					);
				})
			)
			.subscribe(filteredPreValidations => {
				this.filteredPreValidations = filteredPreValidations;
			});

		this.scheduleIssuesFilterControl.valueChanges
			.pipe(
				map(filterString => {
					return this.valData.scheduleIssues.filter(
						g =>
							g.ResourceName.toLowerCase().includes(filterString.toLowerCase()) ||
							g.ValidationErrors.some(
								item =>
									item.ErrorMessageLocalized.toLowerCase().includes(filterString.toLowerCase()) ||
									this.translate
										.instant(item.ResourceType.toLowerCase())
										.includes(filterString.toLowerCase())
							)
					);
				})
			)
			.subscribe(filteredScheduleIssues => {
				this.filteredScheduleIssues = filteredScheduleIssues;
			});
		
	}

	ngOnDestroy(): void {
		clearInterval(this.timer);
	}

	private checkState() {
		this.checkProgress();
		this.timer = setInterval(this.checkProgress, 10000);
	}

	public clearPreValidationFilter() {
		this.preValidationFilterControl.setValue('');
	}

	public clearScheduleIssuesFilter() {
		this.scheduleIssuesFilterControl.setValue('');
	}

	public launchSchedule() {
		this.runScheduling = true;
		this.status = this.translate.instant('PresentTenseSchedule');
		this.planningPeriodService.launchScheduling(this.ppId).subscribe(() => {
			this.checkProgress();
		});
	}

	public optimizeIntraday() {
		this.runIntraday = true;
		this.status = this.translate.instant('IntraOptimize');
		this.planningPeriodService.optimizeIntraday(this.ppId).subscribe(() => {
			this.checkProgress();
		});
	}

	public clearSchedule() {
		this.runClear = true;
		this.status = this.translate.instant('ClearScheduleResultAndHistoryData');
		this.planningPeriodService.clearSchedule(this.ppId).subscribe(() => {
			this.checkProgress();
		});
	}

	public publishSchedule() {
		this.runPublish = true;
		this.planningPeriodService.publishSchedule(this.ppId).subscribe(() => {
			this.runPublish = false;
		});
	}

	public editPlanningGroup() {
		this.navService.go('resourceplanner.editplanninggroup', { groupId: this.groupId });
	}

	public isDisabled() {
		if (this.runScheduling || this.runClear || this.runIntraday || this.runPublish) {
			return true;
		}
	}

	private checkProgress = () => {
		this.planningPeriodService.lastJobStatus(this.ppId).subscribe(data => {
			const schedulingStatus = data.SchedulingStatus;
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

			const clearScheduleStatus = data.ClearScheduleStatus;
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

			const intradayOptimizationStatus = data.IntradayOptimizationStatus;
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

	private loadPlanningPeriodInfo() {
		this.planningPeriodService.getPlanningPeriodInfo(this.ppId).subscribe(data => {
			this.planningPeriodInfo = data ? data : {};
			this.totalAgents = data ? data.TotalAgents : 0;
		});
	}

	private loadPlanningGroupInfo() {
		this.planningGroupService.getPlanningGroup(this.groupId).subscribe(data => {
			this.planningGroupInfo = data ? data : {};
		});
	}

	private updateValidationErrorsNumber() {
		this.valData.totalValNum = 0;
		this.valData.totalPreValNum = 0;
		this.valData.totalLastValNum = 0;
		const pre = this.valData.preValidation;
		const after = this.valData.scheduleIssues;
		if (pre.length > 0) {
			pre.forEach(item => {
				if (item.ValidationErrors !== null) this.valData.totalPreValNum += item.ValidationErrors.length;
			});
		}
		if (after.length > 0) {
			after.forEach(item => {
				if (item.ValidationErrors !== null) this.valData.totalLastValNum += item.ValidationErrors.length;
			});
		}
		this.valData.totalValNum = this.valData.totalPreValNum + this.valData.totalLastValNum;
	}

	private loadLastResult() {
		this.planningPeriodService.lastJobResult(this.ppId).subscribe(data => {
			const fullSchedulingResult = data.FullSchedulingResult;
			if (fullSchedulingResult) {
				this.isScheduled = true;
				this.scheduledAgents = data.FullSchedulingResult.ScheduledAgentsCount;
				this.valData.scheduleIssues = data.FullSchedulingResult.BusinessRulesValidationResults;
				this.scheduleIssuesFilterControl.updateValueAndValidity();
				this.updateValidationErrorsNumber();
				if (!fullSchedulingResult) return;
				this.dayNodes = fullSchedulingResult.SkillResultList ? fullSchedulingResult.SkillResultList : undefined;
			} else {
				this.isScheduled = false;
			}
		});
	}

	private loadValidations() {
		this.valLoading = true;
		this.planningPeriodService.getValidation(this.ppId).subscribe(data => {
			this.valData.preValidation = data.InvalidResources;
			this.preValidationFilterControl.updateValueAndValidity();
			this.valLoading = false;
			this.updateValidationErrorsNumber();
		});
	}
}
