import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import format from 'date-fns/format';
import { NzTreeNode, NzTreeNodeOptions } from 'ng-zorro-antd';
import { filter, map, switchMap } from 'rxjs/operators';
import { Moment } from '../../../../../node_modules/moment';
import { UserService } from '../../../core/services';
import { AuditTrailService } from '../../services';
import { Person } from '../../../shared/types';

const mapToISODate = date => format(date, 'YYYY-MM-DD');
const toISORange = ([startDate, endDate]: DateRange): DateRange => [mapToISODate(startDate), mapToISODate(endDate)];
/*const orgTreeToNgTree = (unit: OrgUnit): NzTreeNodeOptions => {
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
};*/

interface DateRange extends Array<string> {
	[0]: string;
	[1]: string;
}

@Component({
	selector: 'general-audit-trail',
	templateUrl: './general-audit-trail.component.html',
	styleUrls: ['./general-audit-trail.component.scss'],
	providers: []
})
export class GeneralAuditTrailComponent implements OnInit {
	dateformat = 'YYYY-MM-DD';
	moment: Moment;

	searchForm: FormGroup;
	//changedBy = this.auditTrailService.personsWhoChangedSchedules();
	orgTree: NzTreeNode[] = [];
	//auditData: SearchResult = [];
	person: Person;

	constructor(
		private fb: FormBuilder,
		private auditTrailService: AuditTrailService,
		private userService: UserService
	) {
		this.userService.getPreferences().subscribe({
			next: prefs => {
				this.moment = moment().locale(prefs.DateFormatLocale);
			}
		});
	}

	ngOnInit() {
		this.setupForm();
		this.getPersonByKeyword();
	}

	getPersonByKeyword(): any {
		this.auditTrailService.getPersonByKeyword('ash').subscribe({
			next: results => {
				return results;
			}
		});
		//	return '';
	}

	setupForm() {
		this.searchForm = this.fb.group({
			personPicker: this.person,
			changedRange: [[new Date(), new Date()]]
		});
	}
	/*
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
			AffectedPeriodStartDate: scheduleStart,
			AffectedPeriodEndDate: scheduleEnd,
			ChangedByPersonId: auditTarget,
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
	}*/
}
