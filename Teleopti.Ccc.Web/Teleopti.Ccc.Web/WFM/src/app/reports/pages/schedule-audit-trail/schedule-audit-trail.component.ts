/*import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import format from 'date-fns/format';
import { NzTreeNode, NzTreeNodeOptions } from 'ng-zorro-antd';
import { filter, map, switchMap } from 'rxjs/operators';
import { Moment } from '../../../../../node_modules/moment';
import { UserService } from '../../../core/services';
import { AuditTrailService, OrgUnit, ScheduleAuditTrailReportQuery, SearchResult } from '../../services';

const mapToISODate = date => format(date, 'YYYY-MM-DD');
const toISORange = ([startDate, endDate]: DateRange): DateRange => [mapToISODate(startDate), mapToISODate(endDate)];
const orgTreeToNgTree = (unit: OrgUnit): NzTreeNodeOptions => {
	const tree: NzTreeNodeOptions = {
		title: unit.Name,
		key: unit.Id
	};
	if (Array.isArray(unit.Children)) {
		tree.children = unit.Children.map(orgTreeToNgTree);
	} else {
		tree.isLeaf = true;
	}
	return tree;
};

interface DateRange extends Array<string> {
	[0]: string;
	[1]: string;
}

@Component({
	selector: 'report-audit-trail',
	templateUrl: './schedule-audit-trail.component.html',
	styleUrls: ['./schedule-audit-trail.component.scss'],
	providers: []
})
export class ScheduleAuditTrailComponent implements OnInit {
	constructor(
		private fb: FormBuilder,
		private auditTrailService: AuditTrailService,
		private userService: UserService
	) {
		this.userService.preferences$.subscribe({
			next: prefs => {
				//this.moment = moment().locale(prefs.DateFormatLocale);
				this.dateformat = this.moment.localeData().longDateFormat('L');
			}
		});
	}

	dateformat = 'YYYY-MM-DD';
	moment: Moment;

	searchForm: FormGroup;
	changedBy = this.auditTrailService.personsWhoChangedSchedules();
	orgTree: NzTreeNode[] = [];
	auditData: SearchResult = [];

	get scheduleRange(): AbstractControl {
		return this.searchForm.get('scheduleRange');
	}

	get orgSelection(): AbstractControl {
		return this.searchForm.get('orgSelection');
	}

	ngOnInit() {
		this.setupForm();
		this.updateOrganizationList();
	}

	setupForm() {
		this.searchForm = this.fb.group({
			scheduleRange: [[new Date(), new Date()]],
			changedRange: [[new Date(), new Date()]],
			auditTarget: '',
			orgSelection: [[]]
		});
	}

	updateOrganizationList() {
		const teamsFromDateRange = ([startDate, endDate]: DateRange) => {
			return this.auditTrailService.getTeams({ startDate, endDate });
		};
		const orgTreesToNzTreeNodes = orgTrees => orgTrees.map(orgTreeToNgTree).map(node => new NzTreeNode(node));
		this.scheduleRange.valueChanges
			.pipe(
				filter(dateRange => dateRange.length > 0),
				map(dateRange => toISORange(dateRange)),
				switchMap(dateRange => teamsFromDateRange(dateRange)),
				map(orgTrees => orgTreesToNzTreeNodes(orgTrees))
			)
			.subscribe({
				next: teams => {
					this.orgTree = teams;
				}
			});
		// Trigger value change
		this.scheduleRange.updateValueAndValidity();
	}

	get selectedTeamIds() {
		const findCheckedLeaves = (nodes: NzTreeNode[]) => {
			return nodes.reduce((acc, node) => {
				if (node.isLeaf && node.isChecked) return acc.concat(node);
				else if (node.isAllChecked || node.isHalfChecked) {
					return acc.concat(findCheckedLeaves(node.children));
				}
				return acc;
			}, []);
		};
		const findCheckedLeavesIds = tree => findCheckedLeaves(tree).map(node => node.key);
		return findCheckedLeavesIds(this.orgTree);
	}

	submitForm(): void {
		const { scheduleRange, auditTarget, changedRange } = this.searchForm.value;
		const [scheduleStart, scheduleEnd] = toISORange(scheduleRange);
		const [changedStart, changedEnd] = toISORange(changedRange);
		const teamIds = this.selectedTeamIds;

		const body: ScheduleAuditTrailReportQuery = {
			// AffectedPeriodStartDate: '2018-01-01',
			AffectedPeriodStartDate: scheduleStart,
			AffectedPeriodEndDate: scheduleEnd,
			ChangedByPersonId: auditTarget,
			// ChangesOccurredStartDate: '2018-01-01',
			ChangesOccurredStartDate: changedStart,
			ChangesOccurredEndDate: changedEnd,
			MaximumResults: 100,
			TeamIds: teamIds
		};

		this.auditTrailService.search(body).subscribe({
			next: results => {
				this.auditData = results.map(row => {
					return {
						...row,
						ModifiedAt: this.moment.format('L LT'),
						ScheduleStart: this.moment.format('L LT'),
						ScheduleEnd: this.moment.format('L LT')
					};
				});
			}
		});
	}
}
*/
