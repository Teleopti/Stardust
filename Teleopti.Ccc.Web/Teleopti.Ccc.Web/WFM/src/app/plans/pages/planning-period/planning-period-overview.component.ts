import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import {PlanningGroupService, PlanningPeriodService} from '../../shared';
import { IStateService } from 'angular-ui-router';
import { TranslateService } from '@ngx-translate/core';
import { NavigationService } from '../../../core/services';
import { FormBuilder, FormControl } from '@angular/forms';
import {groupBy, map, mergeMap, reduce, toArray} from 'rxjs/operators';
import {HeatMapColorHelper} from "../../shared/heatmapcolor.service";
import {DateFormatPipe} from "ngx-moment";
import {from} from "rxjs";

@Component({
	selector: 'plans-period-overview',
	templateUrl: './planning-period-overview.component.html',
	styleUrls: ['./planning-period-overview.component.scss'],
	providers: []
})
export class PlanningPeriodOverviewComponent implements OnInit, OnDestroy {
	preValidationFilterControl: FormControl = this.fb.control('');
	scheduleIssuesFilterControl: FormControl = this.fb.control('');
	skillFilterControl: FormControl = this.fb.control('');
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
	months : any;
	legends: any[] = [];
	worstUnderStaffDay: any;
	worstOverStaffDay: any;
	showNumbers: false;

	validationFilter;

	skills;
	filteredSkills;

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
		private fb: FormBuilder,
		private heatMapColorHelper:HeatMapColorHelper, 
		private amDateFormat: DateFormatPipe
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
		this.initLegends();
		
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

		this.skillFilterControl.valueChanges
			.pipe(
				map(filterString => {
					return this.skills
						.filter(
						g =>
							g.SkillName.toLowerCase().includes(filterString.toLowerCase()) 
					);
				})
			)
			.subscribe(filteredSkills => {
				this.filteredSkills = filteredSkills;
			});
	}

	ngOnDestroy(): void {
		clearInterval(this.timer);
	}

	private padZero(str) {
		let len = 2;
		let zeros = new Array(len).join('0');
		return (zeros + str).slice(-len);
	}

	private invertColor(hex, bw) {
		if (hex.indexOf('#') === 0) {
			hex = hex.slice(1);
		}
		// convert 3-digit hex to 6-digits.
		if (hex.length === 3) {
			hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
		}
		if (hex.length !== 6) {
			throw new Error('Invalid HEX color.');
		}
		let r = parseInt(hex.slice(0, 2), 16),
			g = parseInt(hex.slice(2, 4), 16),
			b = parseInt(hex.slice(4, 6), 16);
		if (bw) {
			// http://stackoverflow.com/a/3943023/112731
			return (r * 0.299 + g * 0.587 + b * 0.114) > 186
				? '#000000'
				: '#FFFFFF';
		}
		// pad each with zeros and return
		return "#" + this.padZero((255 - r).toString(16)) + this.padZero((255 - g).toString(16)) + this.padZero((255 - b).toString(16));
	}
	
	private initLegends(){
		for(let i = 0; i <41;i++){
			const number = i*5-100;
			this.legends.push({
				number: number,
				bgcolor: this.heatMapColorHelper.getColor(number)
			});
		}
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
	
	public clearSkillMapFilter() {
		this.skillFilterControl.setValue('');
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
					this.skills = undefined;
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
	
	private parseMonths(){
		const months = this.skills[0].SkillDetails.map((item: any) => this.amDateFormat.transform(item.Date, 'MMMM'));
		const monthCount = from(months).pipe(
			groupBy(item => item),
			mergeMap(group => group.pipe(
				reduce((total, item) => total + 1, 0),
				map(total => ({Name: group.key, Count: total}))
				)
			),
			toArray());

		monthCount.subscribe(result => this.months = result);
	}
	
	private parseWorstDays(){
		const allDays = [];
		this.skills.forEach(skill=>{
			skill.SkillDetails.forEach(day =>{
				allDays.push(day);
			});
		});
		allDays.sort((a, b)=>
			a.RelativeDifference>b.RelativeDifference?1:-1
		);
		this.worstUnderStaffDay = allDays[0];
		this.worstOverStaffDay = allDays[allDays.length-1];
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
				const skillResultList = fullSchedulingResult.SkillResultList ? fullSchedulingResult.SkillResultList : undefined;
				if(skillResultList){
					const culturalDaysOff = {
						a : 6, //saturday
						b : 0, //sunday
						start : 1
					};
					skillResultList.forEach(skill=>{
						skill.SkillDetails.forEach(day=>{
							day.bgcolor = this.heatMapColorHelper.getColor(day.RelativeDifference*100);
							day.fontcolor = this.invertColor(day.bgcolor, true);
							const weekday = new Date(day.Date).getDay();
							if (weekday === culturalDaysOff.a) {
								day.weekend = true;
								day.saturday = true;
							}
							if(weekday === culturalDaysOff.b){
								day.weekend = true;
								day.sunday = true;
							}
							if (weekday === culturalDaysOff.start) {
								day.weekstart = true;
							}
							day.RelativeDifferencePercent = (day.RelativeDifference * 100).toFixed(1);
							day.tooltip = (day.ColorId === 4? this.translate.instant('Closed') : this.translate.instant('RelativeDifference') + ' ' + day.RelativeDifferencePercent + '%') + ' | ' + skill.SkillName + ' | ' + this.amDateFormat.transform(day.Date, 'L');
						});
					});

					skillResultList.sort((a, b)=>
					{
						let suma = 0;
						a.SkillDetails.forEach(item=>{
							suma +=item.RelativeDifference;
						});
						let sumb = 0;
						b.SkillDetails.forEach(item=>{
							sumb +=item.RelativeDifference;
						});

						return suma>sumb?1:-1;
					});
				}
				this.skills = skillResultList.filter(skill=>skill.SkillDetails.some(day=>day.ColorId!==4));
				this.skillFilterControl.updateValueAndValidity();
				if(skillResultList && skillResultList.length>0) {
					this.parseMonths();
					this.parseWorstDays();
				}
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
