import { AfterContentInit, Component, OnDestroy, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import c3 from 'c3';
import moment, { Moment } from 'moment';
import { NzMessageService } from 'ng-zorro-antd';
import { Subscription } from 'rxjs';
import { IntradayDataService } from '../services/intraday-data.service';
import { IntradayIconService } from '../services/intraday-icon.service';
import { IntradayPersistService } from '../services/intraday-persist.service';
import { TogglesService } from '../../core/services';
import {
	IntradayChartType,
	IntradayLatestTimeData,
	IntradayPerformanceDataSeries,
	IntradayPerformanceSummaryData,
	IntradayPerformanceSummaryItem,
	IntradayStaffingDataSeries,
	IntradayTrafficData,
	IntradayTrafficDataSeries,
	IntradayTrafficSummaryData,
	IntradayTrafficSummaryItem,
	SkillPickerItem,
	SkillPickerItemType,
	IIntradayAxisLabels
} from '../types';

@Component({
	selector: 'app-intraday-main',
	templateUrl: './intraday-main.html',
	styleUrls: ['./intraday-main.component.scss']
})
export class IntradayMainComponent implements OnInit, OnDestroy, AfterContentInit {
	request: Subscription;
	browserTabToggle = false;
	constructor(
		public intradayDataService: IntradayDataService,
		public translate: TranslateService,
		private message: NzMessageService,
		private persistData: IntradayPersistService,
		public skillIcons: IntradayIconService,
		public toggleService: TogglesService
	) {
		this.toggleService.toggles$.subscribe({
			next: toggles => {
				this.browserTabToggle = toggles.WFM_Intraday_Browser_Tab_81482;
			}
		});
	}

	pollingInterval = 60000;
	selectedSkillOrGroup: SkillPickerItem;
	// selectedSubSkill: Skill;
	selectedSubSkillId: string;
	selectedOffset = 0;
	selectedChartType: IntradayChartType = 'traffic';
	selectedDate: Moment;
	selectedTabIndex: number;

	displayDate: string = moment().format('LLLL');
	intradayTabs: IntradayChartType;
	chartData: c3.Data = this.trafficDataToC3Data(this.getEmptyTrafficData().DataSeries);
	latestTime: IntradayLatestTimeData | undefined;
	summaryData: IntradayTrafficSummaryItem[] | IntradayPerformanceSummaryItem[] = [];
	loading = false;
	error = false;
	errorMessage = '';
	exporting = false;
	timer: any;
	showSkills = true;
	showReforecastedWarning = false;
	axisLabels: IIntradayAxisLabels = { yLabel: '-', y2Label: '-' };
	isFocused = true;

	handleVisibilityChange() {
		this.isFocused = !document.hidden;
	}

	addListeneersForHiddenTab() {
		// Set the name of the hidden property and the change event for visibility
		let hidden = '';
		if (typeof document.hidden !== 'undefined') {
			// Opera 12.10 and Firefox 18 and later support
			hidden = 'hidden';

			// Warn if the browser doesn't support addEventListener or the Page Visibility API
			if (typeof document.addEventListener === 'undefined' || hidden === undefined) {
				console.log(
					'This requires a browser, such as Google Chrome or Firefox, that supports the Page Visibility API.'
				);
			} else {
				// Handle page visibility change
				document.addEventListener('visibilitychange', this.handleVisibilityChange, false);
			}
		}
	}

	ngOnInit() {
		this.startTimer();
		if (this.browserTabToggle) {
			this.addListeneersForHiddenTab();
		}
	}

	startTimer() {
		this.timer = setInterval(this.updateOnInterval, this.pollingInterval);
	}

	ngAfterContentInit() {
		const persisted = this.persistData.getPersisted();
		this.selectedOffset = 0;
		if (persisted) {
			this.selectedSkillOrGroup = persisted.selectedSkillOrGroup;
			this.selectedSubSkillId = persisted.selectedSubSkillId;
			this.selectedOffset = persisted.selectedOffset;
			this.selectedChartType = persisted.selectedChartType;
			this.selectedDate = persisted.selectedDate;
			this.displayDate = moment(this.selectedDate).format('LLLL');
			this.selectedTabIndex = persisted.selectedTabIndex;
			this.updateData();
		}
	}

	updateOnInterval = () => {
		if (this.browserTabToggle) {
			if (!document.hidden && this.selectedOffset === 0 && this.selectedSkillOrGroup && this.loading === false) {
				this.updateData();
			}
		} else {
			if (this.selectedOffset === 0 && this.selectedSkillOrGroup && this.loading === false) {
				this.updateData();
			}
		}
	};

	ngOnDestroy(): void {
		clearInterval(this.timer);
	}

	private setPersistedData() {
		this.persistData.setPersisted({
			selectedSkillOrGroup: this.selectedSkillOrGroup,
			selectedSubSkillId: this.selectedSubSkillId,
			selectedOffset: this.selectedOffset,
			selectedChartType: this.selectedChartType,
			selectedDate: this.selectedDate,
			selectedTabIndex: this.selectedTabIndex
		});
	}

	onSelectSkill(e: SkillPickerItem) {
		this.selectedSkillOrGroup = e;
		this.selectedSubSkillId = 'all';
		this.updateData();
		this.setPersistedData();
	}

	onClickTab(index: number) {
		this.selectedTabIndex = index;
		const chartTypes: IntradayChartType[] = ['traffic', 'performance', 'staffing'];
		this.selectedChartType = chartTypes[index];

		this.updateData();
	}

	onSelectDate(e: number) {
		if (e !== 0) clearInterval(this.timer);
		else this.startTimer();
		this.selectedOffset = e;
		this.selectedDate = moment().add(e, 'days');
		this.displayDate = this.selectedDate.format('LLLL');
		this.updateData();
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

	onPickSubSkill() {
		this.updateData();
	}

	onSubSkillClick(event: any) {
		event.stopPropagation();
	}

	updateData = () => {
		// tslint:disable-next-line:no-debugger
		this.setPersistedData();
		if (!this.selectedSkillOrGroup || !this.selectedSkillOrGroup.Skills) {
			return;
		}

		this.showReforecastedWarning = this.isWarningableTab() && this.isSkillEmailOrBackoffice();

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
			if (this.request) this.request.unsubscribe();
			if (selectedSkill.Skills.length === 0) {
				this.loading = true;
				this.error = false;
				this.request = this.intradayDataService.getTrafficData(selectedSkill.Id, this.selectedOffset).subscribe(
					data => {
						if (data.IncomingTrafficHasData) {
							if (this.selectedOffset === 0) {
								this.latestTime = {
									StartTime: data.LatestActualIntervalStart,
									EndTime: data.LatestActualIntervalEnd
								};
							}
							this.chartData = this.trafficDataToC3Data(data.DataSeries);
						} else {
							this.chartData = {};
						}

						this.summaryData = this.trafficDataToSummaryData(data.Summary);
						this.loading = false;
					},
					() => {
						this.error = true;
						this.errorMessage = this.translate.instant('NoDataAvailable');
					}
				);
			}
			if (selectedSkill.Skills.length > 0) {
				this.loading = true;
				this.error = false;
				this.request = this.intradayDataService
					.getGroupTrafficData(selectedSkill.Id, this.selectedOffset)
					.subscribe(
						data => {
							if (data.IncomingTrafficHasData) {
								if (this.selectedOffset === 0) {
									this.latestTime = {
										StartTime: data.LatestActualIntervalStart,
										EndTime: data.LatestActualIntervalEnd
									};
								}
								this.chartData = this.trafficDataToC3Data(data.DataSeries);
							} else {
								this.chartData = {};
							}

							this.summaryData = this.trafficDataToSummaryData(data.Summary);
							this.loading = false;
						},
						() => {
							this.error = true;
							this.errorMessage = this.translate.instant('NoDataAvailable');
						}
					);
			}
		}

		if (this.selectedChartType === 'performance') {
			if (this.request) this.request.unsubscribe();
			if (selectedSkill.Skills.length === 0) {
				this.loading = true;
				this.error = false;
				this.request = this.intradayDataService
					.getPerformanceData(selectedSkill.Id, this.selectedOffset)
					.subscribe(
						data => {
							if (data.PerformanceHasData) {
								if (this.selectedOffset === 0) {
									this.latestTime = {
										StartTime: data.LatestActualIntervalStart,
										EndTime: data.LatestActualIntervalEnd
									};
								}
								this.chartData = this.performanceDataToC3Data(data.DataSeries);
							} else {
								this.chartData = {};
							}

							this.summaryData = this.performanceDataToSummaryData(data.Summary);
							this.loading = false;
						},
						() => {
							this.error = true;
							this.errorMessage = this.translate.instant('NoDataAvailable');
						}
					);
			}
			if (selectedSkill.Skills.length > 0) {
				this.loading = true;
				this.error = false;
				this.request = this.intradayDataService
					.getGroupPerformanceData(selectedSkill.Id, this.selectedOffset)
					.subscribe(
						data => {
							if (data.PerformanceHasData) {
								if (this.selectedOffset === 0) {
									this.latestTime = {
										StartTime: data.LatestActualIntervalStart,
										EndTime: data.LatestActualIntervalEnd
									};
								}
								this.chartData = this.performanceDataToC3Data(data.DataSeries);
							} else {
								this.chartData = {};
							}

							this.summaryData = this.performanceDataToSummaryData(data.Summary);
							this.loading = false;
						},
						() => {
							this.error = true;
							this.errorMessage = this.translate.instant('NoDataAvailable');
						}
					);
			}
		}

		if (this.selectedChartType === 'staffing') {
			this.latestTime = undefined;
			if (this.request) this.request.unsubscribe();
			if (selectedSkill.Skills.length === 0) {
				this.loading = true;
				this.error = false;
				this.request = this.intradayDataService
					.getStaffingData(selectedSkill.Id, this.selectedOffset)
					.subscribe(
						data => {
							this.chartData = this.staffingDataToC3Data(data.DataSeries);
							this.loading = false;
						},
						() => {
							this.error = true;
							this.errorMessage = this.translate.instant('NoDataAvailable');
						}
					);
			}
			if (selectedSkill.Skills.length > 0) {
				this.loading = true;
				this.error = false;
				this.request = this.intradayDataService
					.getGroupStaffingData(selectedSkill.Id, this.selectedOffset)
					.subscribe(
						data => {
							this.chartData = this.staffingDataToC3Data(data.DataSeries);
							this.loading = false;
						},
						() => {
							this.error = true;
							this.errorMessage = this.translate.instant('NoDataAvailable');
						}
					);
			}
		}
	};

	goToGroupManager() {
		location.hash = '#/intraday/skill-group-manager';
	}

	private trafficDataToC3Data(input: IntradayTrafficDataSeries): c3.Data {
		if (input && input.Time !== null) {
			const timeStamps = input.Time.map(item => {
				return moment(item).format('YYYY-MM-DD HH:mm');
			});
			if (!timeStamps || timeStamps.length === 0) return {};
			this.axisLabels.yLabel = this.translate.instant('Volume');
			this.axisLabels.y2Label = this.translate.instant('AverageHandlingTime');
			return {
				x: 'x',
				xFormat: '%Y-%m-%d %H:%M',
				columns: [
					['x'].concat(timeStamps),
					['Forecasted_calls'].concat(input.ForecastedCalls),
					['Calls'].concat(input.CalculatedCalls),
					['Forecasted_AHT'].concat(input.ForecastedAverageHandleTime),
					['AHT'].concat(input.AverageHandleTime)
				],
				type: 'area-spline',
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
					Difference: input.ForecastedActualCallsDiff
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

	private performanceDataToC3Data(input: IntradayPerformanceDataSeries): c3.Data {
		if (input && input.Time !== null) {
			const timeStamps = input.Time.map(item => moment(item).format('YYYY-MM-DD HH:mm'));
			if (!timeStamps || timeStamps.length === 0) return {};
			this.axisLabels.yLabel = this.translate.instant('SecondShort');
			this.axisLabels.y2Label = this.translate.instant('%');
			return {
				x: 'x',
				xFormat: '%Y-%m-%d %H:%M',
				columns: this.showReforecastedWarning
					? [
							['x'].concat(timeStamps),
							['Service_level'].concat(input.ServiceLevel),
							['ESL'].concat(input.EstimatedServiceLevels),
							['ASA'].concat(input.AverageSpeedOfAnswer)
					  ]
					: [
							['x'].concat(timeStamps),
							['Service_level'].concat(input.ServiceLevel),
							['ESL'].concat(input.EstimatedServiceLevels),
							['ASA'].concat(input.AverageSpeedOfAnswer),
							['Abandoned_rate'].concat(input.AbandonedRate)
					  ],
				type: 'area-spline',
				colors: {
					ASA: '#99D6FF',
					Abandoned_rate: '#0099FF',
					Service_level: '#FFC285',
					ESL: '#FB8C00'
				},
				names: {
					ASA: this.translate.instant('AverageSpeedOfAnswer') + ' ←',
					Service_level: this.translate.instant('ServiceLevel') + ' →',
					ESL: this.translate.instant('ESL') + ' →',
					Abandoned_rate: this.translate.instant('AbandonedRate') + ' ←'
				},
				axes: {
					Service_level: 'y2',
					ESL: 'y2',
					ASA: 'y',
					Abandoned_rate: 'y2'
				}
			};
		} else {
			return {};
		}
	}

	private performanceDataToSummaryData(input: IntradayPerformanceSummaryData): IntradayPerformanceSummaryItem[] {
		if (input) {
			return this.showReforecastedWarning
				? [
						{
							Heading: this.translate.instant('Average'),
							ServiceLevel: input.ServiceLevel <= 1 ? input.ServiceLevel * 100 : input.ServiceLevel,
							EstimatedServiceLevel: input.EstimatedServiceLevel * 100,
							AverageSpeedOfAnswer: input.AverageSpeedOfAnswer
						}
				  ]
				: [
						{
							Heading: this.translate.instant('Average'),
							ServiceLevel: input.ServiceLevel <= 1 ? input.ServiceLevel * 100 : input.ServiceLevel,
							EstimatedServiceLevel: input.EstimatedServiceLevel * 100,
							AverageSpeedOfAnswer: input.AverageSpeedOfAnswer,
							AbandonRate: input.AbandonRate * 100
						}
				  ];
		} else {
			return undefined;
		}
	}

	private staffingDataToC3Data(input: IntradayStaffingDataSeries): c3.Data {
		if (input && input.Time !== null) {
			const timeStamps = input.Time.map(item => moment(item).format('YYYY-MM-DD HH:mm'));
			if (!timeStamps || timeStamps.length === 0) return {};
			this.axisLabels.yLabel = this.translate.instant('Agents');
			this.axisLabels.y2Label = this.translate.instant('%');
			return {
				x: 'x',
				xFormat: '%Y-%m-%d %H:%M',
				columns: this.showReforecastedWarning
					? [
							['x'].concat(timeStamps),
							['Forecasted_staffing'].concat(input.ForecastedStaffing),
							['Scheduled_staffing'].concat(input.ScheduledStaffing),
							['Actual_staffing'].concat(input.ActualStaffing)
					  ]
					: [
							['x'].concat(timeStamps),
							['Forecasted_staffing'].concat(input.ForecastedStaffing),
							['Scheduled_staffing'].concat(input.ScheduledStaffing),
							['Actual_staffing'].concat(input.ActualStaffing),
							['Updated_forecasted_staffing'].concat(input.UpdatedForecastedStaffing)
					  ],
				type: 'area-spline',
				colors: {
					Forecasted_calls: '#99D6FF',
					Calls: '#0099FF',
					Forecasted_AHT: '#FFC285',
					AHT: '#FB8C00'
				},
				names: {
					Forecasted_staffing: this.translate.instant('ForecastedStaff') + ' ←',
					Scheduled_staffing: this.translate.instant('ScheduledStaff') + ' ←',
					Actual_staffing: this.translate.instant('RequiredStaff') + ' ←',
					Updated_forecasted_staffing: this.translate.instant('ReforecastedStaff') + ' ←'
				},
				axes: {
					Forecasted_staffing: 'y',
					Scheduled_staffing: 'y',
					Actual_staffing: 'y',
					Updated_forecasted_staffing: 'y'
				}
			};
		} else {
			return {};
		}
	}

	public getEmailOrBackofficeWarning() {
		if (this.selectedTabIndex === 1) {
			return this.translate.instant('NotShowingAbandonRate');
		} else if (this.selectedTabIndex === 2) {
			return this.translate.instant('NotShowingReforcastedAgents');
		}
	}

	public isSkillEmailOrBackoffice(): boolean {
		if (this.selectedSkillOrGroup.Skills && this.selectedSkillOrGroup.Skills.length > 0) {
			const found = this.selectedSkillOrGroup.Skills.find(
				skill => skill.SkillType === 'SkillTypeEmail' || skill.SkillType === 'SkillTypeBackoffice'
			);
			if (found && this.selectedSubSkillId === 'all') return true;
			else {
				const s = this.selectedSkillOrGroup.Skills.find(x => x.Id === this.selectedSubSkillId);
				if (!s) return false;
				return s.SkillType === 'SkillTypeEmail' || s.SkillType === 'SkillTypeBackoffice';
			}
		} else {
			return (
				this.selectedSkillOrGroup.Skill.SkillType === 'SkillTypeEmail' ||
				this.selectedSkillOrGroup.Skill.SkillType === 'SkillTypeBackoffice'
			);
		}
	}

	public isWarningableTab() {
		if (this.selectedTabIndex === 0) {
			return false;
		}
		return true;
	}

	getEmptyTrafficData(): IntradayTrafficData {
		return {
			FirstIntervalStart: moment().format(),
			FirstIntervalEnd: moment()
				.add(15, 'minutes')
				.format(),
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
