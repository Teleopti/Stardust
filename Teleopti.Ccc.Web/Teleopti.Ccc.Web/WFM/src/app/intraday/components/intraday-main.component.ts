import { Component, OnInit, OnDestroy, AfterContentInit, OnChanges } from '@angular/core';
import moment, { Moment } from 'moment';
import { IntradayDataService } from '../services/intraday-data.service';
import { IntradayPersistService } from '../services/intraday-persist.service';
import { TranslateService } from '@ngx-translate/core';
import { NzMessageService, NzTabComponent, NzTabChangeEvent } from 'ng-zorro-antd';
import c3 from 'c3';
import {
	SkillPickerItem,
	IntradayChartType,
	IntradayTrafficData,
	IntradayTrafficDataSeries,
	IntradayTrafficSummaryItem,
	IntradayTrafficSummaryData,
	IntradayPerformanceDataSeries,
	IntradayPerformanceSummaryItem,
	IntradayPerformanceSummaryData,
	IntradayStaffingDataSeries,
	Skill,
	SkillPickerItemType
} from '../types';
import { IntradayIconService } from '../services/intraday-icon.service';

@Component({
	selector: 'app-intraday-main',
	templateUrl: './intraday-main.html',
	styleUrls: ['./intraday-main.component.scss']
})
export class IntradayMainComponent implements OnInit, OnDestroy, AfterContentInit {
	constructor(
		public intradayDataService: IntradayDataService,
		public translate: TranslateService,
		private message: NzMessageService,
		private persistData: IntradayPersistService,
		private skillIcons: IntradayIconService
	) {}

	selectedSkillOrGroup: SkillPickerItem;
	selectedSubSkill: Skill;
	selectedSubSkillId: string;
	selectedOffset = 0;
	selectedChartType: IntradayChartType = 'traffic';
	selectedDate: Moment;
	selectedTabIndex: number;

	displayDate: string = moment().format('LLLL');
	intradayTabs: IntradayChartType;
	chartData: c3.Data = this.trafficDataToC3Data(this.getEmptyTrafficData().DataSeries);
	summaryData: IntradayTrafficSummaryItem[] | IntradayPerformanceSummaryItem[] = [];
	loading = false;
	exporting = false;
	timer: any;
	showSkills = true;

	ngOnInit() {
		this.startTimer();
	}

	startTimer() {
		this.timer = setInterval(this.updateOnInterval, 60000);
	}

	ngAfterContentInit() {
		const persisted = this.persistData.getPersisted();
		this.selectedOffset = 0;
		if (persisted) {
			this.selectedSkillOrGroup = persisted.selectedSkillOrGroup;
			this.selectedSubSkill = persisted.selectedSubSkill;
			this.selectedSubSkillId = persisted.selectedSubSkillId;
			this.selectedOffset = persisted.selectedOffset;
			this.selectedChartType = persisted.selectedChartType;
			this.selectedDate = persisted.selectedDate;
			this.selectedTabIndex = persisted.selectedTabIndex;
			this.updateData(false);
		}
	}

	updateOnInterval = () => {
		if (this.selectedOffset === 0 && this.selectedSkillOrGroup && this.loading === false) {
			this.updateData(true);
		}
	};

	ngOnDestroy(): void {
		clearInterval(this.timer);
	}

	private setPersistedData() {
		this.persistData.setPersisted({
			selectedSkillOrGroup: this.selectedSkillOrGroup,
			selectedSubSkill: this.selectedSubSkill,
			selectedSubSkillId: this.selectedSubSkillId,
			selectedOffset: this.selectedOffset,
			selectedChartType: this.selectedChartType,
			selectedDate: this.selectedDate,
			selectedTabIndex: this.selectedTabIndex
		});
	}

	onSelectSkill(e: SkillPickerItem) {
		this.selectedSkillOrGroup = e;
		this.updateData(false);
		this.setPersistedData();
	}

	onClickTab(index: number) {
		this.selectedTabIndex = index;
		const chartTypes: IntradayChartType[] = ['traffic', 'performance', 'staffing'];
		this.selectedChartType = chartTypes[index];

		this.updateData(false);
	}

	onSelectDate(e: number) {
		if (e !== 0) clearInterval(this.timer);
		else this.startTimer();
		this.selectedOffset = e;
		this.selectedDate = moment().add(e, 'days');
		this.displayDate = this.selectedDate.format('LLLL');
		this.updateData(true);
	}

	onShowHideSkills() {
		this.showSkills = !this.showSkills;
	}

	exportToExcel() {
		if (this.selectedSkillOrGroup && !this.exporting) {
			this.exporting = true;
			if (this.selectedSkillOrGroup.Skills.length > 1) {
				this.intradayDataService
					.getIntradayExportForSkillGroup(
						angular.toJson({
							id: this.selectedSkillOrGroup.Id,
							dayOffset: this.selectedOffset
						})
					)
					.subscribe(
						data => {
							this.saveData(data);
						},
						error => this.errorSaveData(error)
					);
			} else {
				this.intradayDataService
					.getIntradayExportForSkill(
						angular.toJson({
							id: this.selectedSkillOrGroup.Id,
							dayOffset: this.selectedOffset
						})
					)
					.subscribe(
						data => {
							this.saveData(data);
						},
						error => this.errorSaveData(error)
					);
			}
		}
	}

	saveData(data) {
		const blob = new Blob([data]);
		this.exporting = false;
		saveAs(blob, 'IntradayExportedData ' + moment().format('YYYY-MM-DD') + '.xlsx');
	}

	errorSaveData(error: Error) {
		this.message.create('error', error.message);
		this.exporting = false;
	}

	// getSelectedSkillOrGroup(): SkillPickerItem {
	// 	console.log('selected');

	// 	clearInterval(this.timer);
	// 	if (!this.selectedSubSkillId || this.selectedSubSkillId === 'all') {
	// 		this.timer = setInterval(this.updateData, 1000);
	// 		return this.selectedSkillOrGroup;
	// 	} else {
	// 		const spi: SkillPickerItem = {
	// 			Id: this.selectedSubSkillId,
	// 			Name: '',
	// 			Skills: [],
	// 			Type: SkillPickerItemType.Skill
	// 		};
	// 		this.timer = setInterval(this.updateData, 1000);
	// 		return spi;
	// 	}
	// }

	onPickSubSkill() {
		this.updateData(true);
	}

	onSubSkillClick(event: any) {
		event.stopPropagation();
	}

	updateData = (columnsOnly: boolean = true) => {
		this.setPersistedData();
		console.log('this.selectedChartType', this.selectedChartType);

		if (!this.selectedSkillOrGroup || !this.selectedSkillOrGroup.Skills) {
			return;
		}
		let selectedSkill = this.selectedSkillOrGroup;
		if (this.selectedSubSkillId && this.selectedSubSkillId !== 'all') {
			selectedSkill = {
				Id: this.selectedSubSkillId,
				Name: '',
				Skills: [],
				Type: SkillPickerItemType.Skill
			};
		}
		if (this.selectedChartType === 'traffic') {
			console.log('RÖW');
			console.log('selectedSkill.Skills.length', selectedSkill.Skills.length);

			if (selectedSkill.Skills.length === 0) {
				this.loading = true;
				this.intradayDataService.getTrafficData(selectedSkill.Id, this.selectedOffset).subscribe(data => {
					this.chartData = this.trafficDataToC3Data(data.DataSeries, columnsOnly);
					this.summaryData = this.trafficDataToSummaryData(data.Summary);
					this.loading = false;
				});
			}
			if (selectedSkill.Skills.length > 0) {
				this.loading = true;
				this.intradayDataService.getGroupTrafficData(selectedSkill.Id, this.selectedOffset).subscribe(data => {
					this.chartData = this.trafficDataToC3Data(data.DataSeries, columnsOnly);
					this.summaryData = this.trafficDataToSummaryData(data.Summary);
					this.loading = false;
				});
			}
		}

		if (this.selectedChartType === 'performance') {
			if (selectedSkill.Skills.length === 0) {
				this.loading = true;
				this.intradayDataService.getPerformanceData(selectedSkill.Id, this.selectedOffset).subscribe(data => {
					this.chartData = this.performanceDataToC3Data(data.DataSeries, columnsOnly);
					this.summaryData = this.performanceDataToSummaryData(data.Summary);
					this.loading = false;
				});
			}
			if (selectedSkill.Skills.length > 0) {
				this.loading = true;
				this.intradayDataService
					.getGroupPerformanceData(selectedSkill.Id, this.selectedOffset)
					.subscribe(data => {
						this.chartData = this.performanceDataToC3Data(data.DataSeries, columnsOnly);
						this.summaryData = this.performanceDataToSummaryData(data.Summary);
						this.loading = false;
					});
			}
		}

		if (this.selectedChartType === 'staffing') {
			if (selectedSkill.Skills.length === 0) {
				this.loading = true;
				this.intradayDataService.getStaffingData(selectedSkill.Id, this.selectedOffset).subscribe(data => {
					this.chartData = this.staffingDataToC3Data(data.DataSeries, columnsOnly);
					this.loading = false;
				});
			}
			if (selectedSkill.Skills.length > 0) {
				this.loading = true;
				this.intradayDataService.getGroupStaffingData(selectedSkill.Id, this.selectedOffset).subscribe(data => {
					this.chartData = this.staffingDataToC3Data(data.DataSeries, columnsOnly);
					this.loading = false;
				});
			}
		}
	};

	goToGroupManager() {
		location.hash = '#/intraday/skill-group-manager';
	}

	private trafficDataToC3Data(input: IntradayTrafficDataSeries, columnsOnly: boolean = false): c3.Data {
		if (input && input.Time !== null) {
			const timeStamps = input.Time.map(item => moment(item).format('HH:mm'));
			if (columnsOnly) {
				return {
					x: 'x',
					columns: [
						['x'].concat(timeStamps),
						['Forcasted_calls'].concat(input.ForecastedCalls),
						['Calls'].concat(input.CalculatedCalls),
						['Forecasted_AHT'].concat(input.ForecastedAverageHandleTime),
						['AHT'].concat(input.AverageHandleTime)
					]
				};
			} else {
				return {
					x: 'x',
					columns: [
						['x'].concat(timeStamps),
						['Forcasted_calls'].concat(input.ForecastedCalls),
						['Calls'].concat(input.CalculatedCalls),
						['Forecasted_AHT'].concat(input.ForecastedAverageHandleTime),
						['AHT'].concat(input.AverageHandleTime)
					],
					type: 'area-spline',
					types: {
						Current: 'bar'
					},
					colors: {
						Forecasted_calls: '#99D6FF',
						Calls: '#0099FF',
						Forecasted_AHT: '#FFC285',
						AHT: '#FB8C00'
					},
					names: {
						Forecasted_calls: this.translate.instant('ForecastedVolume') + ' ←',
						Calls: this.translate.instant('ActualVolume') + ' ←',
						Forecasted_AHT: this.translate.instant('ForecastedAverageHandlingTime') + ' →',
						AHT: this.translate.instant('ActualAverageHandlingTime') + ' →'
					},
					axes: {
						Forecasted_AHT: 'y2',
						AHT: 'y2',
						Calls: 'y',
						Forecasted_calls: 'y'
					}
				};
			}
		} else {
			return {};
		}
	}

	private trafficDataToSummaryData(input: IntradayTrafficSummaryData): IntradayTrafficSummaryItem[] {
		if (input) {
			const returnData = [
				{
					Heading: this.translate.instant('Volume'),
					Forecasted: input.ForecastedCalls,
					Actual: input.CalculatedCalls,
					Difference: input.ForecastedActualHandleTimeDiff
				},
				{
					Heading: this.translate.instant('AverageHandlingTime'),
					Forecasted: input.ForecastedAverageHandleTime,
					Actual: input.AverageHandleTime,
					Difference: input.ForecastedActualHandleTimeDiff
				}
			];
			return returnData;
		} else {
			return undefined;
		}
	}

	private performanceDataToC3Data(input: IntradayPerformanceDataSeries, columnsOnly: boolean = false): c3.Data {
		if (input && input.Time !== null) {
			const timeStamps = input.Time.map(item => moment(item).format('HH:mm'));
			if (columnsOnly) {
				return {
					x: 'x',
					columns: [
						['x'].concat(timeStamps),
						['ASA'].concat(input.AverageSpeedOfAnswer),
						['Abandoned_rate'].concat(input.AbandonedRate),
						['Service_level'].concat(input.ServiceLevel),
						['ESL'].concat(input.EstimatedServiceLevels)
					]
				};
			} else {
				return {
					x: 'x',
					columns: [
						['x'].concat(timeStamps),
						['ASA'].concat(input.AverageSpeedOfAnswer),
						['Abandoned_rate'].concat(input.AbandonedRate),
						['Service_level'].concat(input.ServiceLevel),
						['ESL'].concat(input.EstimatedServiceLevels)
					],
					type: 'area-spline',
					types: {
						Current: 'bar'
					},
					colors: {
						Forecasted_calls: '#99D6FF',
						Calls: '#0099FF',
						Forecasted_AHT: '#FFC285',
						AHT: '#FB8C00'
					},
					names: {
						ASA: this.translate.instant('AverageSpeedOfAnswer') + ' ←',
						Abandoned_rate: this.translate.instant('AbandonedRate') + ' ←',
						Service_level: this.translate.instant('ServiceLevel') + ' →',
						ESL: this.translate.instant('ESL') + ' →'
					},
					axes: {
						Service_level: 'y2',
						Abandoned_rate: 'y2',
						ESL: 'y2',
						ASA: 'y'
					}
				};
			}
		} else {
			return {};
		}
	}

	private performanceDataToSummaryData(input: IntradayPerformanceSummaryData): IntradayPerformanceSummaryItem[] {
		if (input) {
			return [
				{
					Heading: this.translate.instant('Average'),
					ServiceLevel: input.ServiceLevel,
					EstimatedServiceLevel: input.EstimatedServiceLevel,
					AbandonRate: input.AbandonRate,
					AverageSpeedOfAnswer: input.AverageSpeedOfAnswer
				}
			];
		} else {
			return undefined;
		}
	}

	private staffingDataToC3Data(input: IntradayStaffingDataSeries, columnsOnly: boolean = false): c3.Data {
		if (input && input.Time !== null) {
			const timeStamps = input.Time.map(item => moment(item).format('HH:mm'));
			if (columnsOnly) {
				return {
					x: 'x',
					columns: [
						['x'].concat(timeStamps),
						['Forecasted_staffing'].concat(input.ForecastedStaffing),
						['Updated_forecasted_staffing'].concat(input.UpdatedForecastedStaffing),
						['Actual_staffing'].concat(input.ActualStaffing),
						['Scheduled_staffing'].concat(input.ScheduledStaffing)
					]
				};
			} else {
				return {
					x: 'x',
					columns: [
						['x'].concat(timeStamps),
						['Forecasted_staffing'].concat(input.ForecastedStaffing),
						['Updated_forecasted_staffing'].concat(input.UpdatedForecastedStaffing),
						['Actual_staffing'].concat(input.ActualStaffing),
						['Scheduled_staffing'].concat(input.ScheduledStaffing)
					],
					type: 'area-spline',
					types: { Current: 'bar' },
					colors: {
						Forecasted_calls: '#99D6FF',
						Calls: '#0099FF',
						Forecasted_AHT: '#FFC285',
						AHT: '#FB8C00'
					},
					names: {
						Forecasted_staffing: this.translate.instant('ForecastedStaff') + ' ←',
						Updated_forecasted_staffing: this.translate.instant('ReforecastedStaff') + ' ←',
						Actual_staffing: this.translate.instant('RequiredStaff') + ' ←',
						Scheduled_staffing: this.translate.instant('ScheduledStaff') + ' ←'
					},
					axes: {
						Forecasted_staffing: 'y',
						Updated_forecasted_staffing: 'y',
						Actual_staffing: 'y',
						Scheduled_staffing: 'y'
					}
				};
			}
		} else {
			return {};
		}
	}

	getEmptyTrafficData(): IntradayTrafficData {
		return {
			FirstIntervalStart: '2018-08-17T08:00:00',
			FirstIntervalEnd: '2018-08-17T08:15:00',
			LatestActualIntervalStart: null,
			LatestActualIntervalEnd: null,
			Summary: {
				ForecastedCalls: 0.0,
				ForecastedAverageHandleTime: 0.0,
				ForecastedHandleTime: 0.0,
				CalculatedCalls: 0.0,
				AverageHandleTime: 0.0,
				HandleTime: 0.0,
				ForecastedActualCallsDiff: 0.0,
				ForecastedActualHandleTimeDiff: 0.0,
				AverageSpeedOfAnswer: 0.0,
				SpeedOfAnswer: 0.0,
				AnsweredCalls: 0.0,
				ServiceLevel: 0.0,
				AnsweredCallsWithinSL: 0.0,
				AbandonRate: 0.0,
				AbandonedCalls: 0.0
			},
			DataSeries: {
				AverageSpeedOfAnswer: [],
				Time: [],
				ForecastedCalls: [],
				ForecastedAverageHandleTime: [],
				AverageHandleTime: [],
				CalculatedCalls: [],
				AbandonedRate: [],
				ServiceLevel: []
			},
			IncomingTrafficHasData: false
		};
	}
}
