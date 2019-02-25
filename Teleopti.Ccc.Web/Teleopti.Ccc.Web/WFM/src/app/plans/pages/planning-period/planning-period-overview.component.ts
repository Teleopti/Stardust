import { Component, Inject, OnDestroy, OnInit } from '@angular/core';
import {PlanningGroupService, PlanningPeriodService} from '../../shared';
import { IStateService } from 'angular-ui-router';
import { TranslateService } from '@ngx-translate/core';
import { NavigationService } from '../../../core/services';
import { FormBuilder, FormControl } from '@angular/forms';
import {debounceTime, groupBy, map, mergeMap, reduce, tap, toArray} from 'rxjs/operators';
import {HeatMapColorHelper} from "../../shared/heatmapcolor.service";
import {DateFormatPipe} from "ngx-moment";
import {from} from "rxjs";
import {PlanningPeriodActionService} from "../../shared/planningperiod.action.service";
import * as moment from 'moment';

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
	selectedDay = null;
	selectedSkill: string = null;

	validationFilter;

	skills;
	filteredSkills;
	lastUpdated;
	
	forTesting = false;

	valData = {
		totalValNum: 0,
		totalPreValNum: 0,
		totalLastValNum: 0,
		scheduleIssues: [],
		preValidation: []
	};

	sortName = 'SkillName';
	sortValue = null;
	sortMap = {
		SkillName   : null
	};

	constructor(
		private planningPeriodService: PlanningPeriodService,
		private planningPeriodActionService: PlanningPeriodActionService,
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
			.pipe(this.forTesting ? tap() : debounceTime(600))
			.subscribe(filterString => {
				this.search(filterString);
			});
		
		this.loadPlanningGroupInfo();
		this.loadPlanningPeriodInfo();
		this.loadValidations();
		this.loadLastResult();
		this.checkState();
		this.initLegends();
	}

	ngOnDestroy(): void {
		clearInterval(this.timer);
	}

	public search(filterString: string){
		const data = this.skills.filter(g => g.SkillName.toLowerCase().includes(filterString.toLowerCase()));
		if (this.sortName && this.sortValue) {
			this.filteredSkills = data.sort((a, b) => (this.sortValue === 'ascend') ?
				(a[ this.sortName ].toLowerCase() > b[ this.sortName ].toLowerCase() ? 1 : -1) :
				(b[ this.sortName ].toLowerCase() > a[ this.sortName ].toLowerCase() ? 1 : -1));
		}else{
			this.filteredSkills = data;
		}
	}

	public sort(key: string, value: string): void{
		this.sortName = key;
		this.sortValue = value;
		this.search(this.skillFilterControl.value);
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
		if(!this.forTesting){
			this.timer = setInterval(this.checkProgress, 10000);
		}
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
		this.planningPeriodActionService.launchScheduling(this.ppId).subscribe(() => {
			this.checkProgress();
		});
	}

	public optimizeIntraday() {
		this.runIntraday = true;
		this.status = this.translate.instant('IntraOptimize');
		this.planningPeriodActionService.optimizeIntraday(this.ppId).subscribe(() => {
			this.checkProgress();
		});
	}

	public clearSchedule() {
		this.runClear = true;
		this.status = this.translate.instant('ClearScheduleResultAndHistoryData');
		this.planningPeriodActionService.clearSchedule(this.ppId).subscribe(() => {
			this.checkProgress();
		});
	}

	public publishSchedule() {
		this.runPublish = true;
		this.planningPeriodActionService.publishSchedule(this.ppId).subscribe(() => {
			this.runPublish = false;
		});
	}

	public editPlanningGroup() {
		this.navService.go('resourceplanner.editplanninggroup', { groupId: this.groupId });
	}

	public isDisabled() {
		return this.runScheduling || this.runClear || this.runIntraday || this.runPublish;	
	}
	
	public updateIntradayDetails(selectedDay, skillName){
		if(this.selectedDay) {
			this.selectedDay.selected = false;
		}
		if(this.selectedDay === selectedDay){
			this.selectedDay = null;
			this.selectedSkill = null;
		} else{
			this.selectedDay = selectedDay;
			this.selectedDay.selected = true;
			this.selectedSkill = skillName;
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
		
		monthCount.subscribe(result => {
			
			if (result.length > 1) {
				if(result[0].Count<3){
					result[1].Count -= 3-result[0].Count;
					result[0].Count = 3;
				}

				if(result[result.length-1].Count<3){
					result[result.length-2].Count -= 3-result[result.length-1].Count;
					result[result.length-1].Count = 3;
				}
			}
			
			this.months = result
			});
	}
	
	private parseWorstDays(){
		const allDays = [];
		this.skills.forEach(skill=>{
			skill.SkillDetails.forEach(day =>{
				allDays.push(day);
			});
		});
		allDays.sort((a, b)=>{
			if(isNaN(a.RelativeDifference)){
				return 1;
			}
			return a.RelativeDifference>b.RelativeDifference?1:-1;
		});
		this.worstUnderStaffDay = allDays[0];
		this.worstOverStaffDay = allDays[allDays.length-1];
	}

	private loadLastResult() {
		this.planningPeriodService.lastJobResult(this.ppId).subscribe(data => {
			const fullSchedulingResult = data.FullSchedulingResult;
			if (fullSchedulingResult) {
				this.isScheduled = true;
				this.lastUpdated = moment(data.LastUpdated).format('YYYY-MM-DD HH:mm');
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
							let relativeDifferencePercent = day.RelativeDifference * 100;
							day.bgcolor = this.heatMapColorHelper.getColor(relativeDifferencePercent);
							day.fontcolor = this.heatMapColorHelper.invertColor(day.bgcolor, true);
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
							day.RelativeDifferencePercent = relativeDifferencePercent.toFixed(2);
							if(day.ColorId === 4){
								day.DisplayedPercent = '';
							}else if(relativeDifferencePercent.toFixed(0)==='-0'){
								day.DisplayedPercent = '0';
							}else if(relativeDifferencePercent > 999 || isNaN(relativeDifferencePercent)){
								day.DisplayedPercent = '999+';
							}else {
								day.DisplayedPercent = relativeDifferencePercent.toFixed(0);
							}
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
