(function () {
	'use strict';

	angular
	.module('wfm.reports')
	.controller('AuditTrailController', AuditTrailController);

	AuditTrailController.$inject = ['$filter', 'Toggle', 'uiGridConstants', 'ReportsService', 'NoticeService', '$translate'];

	function AuditTrailController($filter, ToggleSvc, uiGridConstants, ReportsService, NoticeService, $translate) {
		var vm = this;

		vm.changedBy = [];
		vm.searchData = null;
		vm.loading = false;
		vm.chartLoaded = false;
		vm.gridOptions = {};
		vm.dateRangeCustomValidators = [];
		vm.dateChangeRange = {
			startDate: moment().utc().subtract(1, 'days'),
			endDate: moment().utc()
		};
		vm.dateModifyRange = {
			startDate: moment().utc().subtract(1, 'year'),
			endDate: moment().utc()
		};
		vm.changesData = [];

		vm.sendForm = sendForm;
		vm.refreshData = refreshData;
		vm.maxResults = 100;

		init();
		function init() {
			getChangedBy();
		}

		function getChangedBy() {
			ReportsService.getAuditTrailChangedByPerson.query().$promise.then(function (result) {
				result.push({
					Id:'',
					Name: $translate.instant('Everyone'),
					Default: true
				})
				vm.changedBy = result;
			});
		}

		function sendForm(form) {
			var postObj =	{
				ChangedByPersonId: form.drop.Id,
				ChangesOccurredStartDate: moment(vm.dateChangeRange.startDate).format("YYYY-MM-DD"),
				ChangesOccurredEndDate: moment(vm.dateChangeRange.endDate).format("YYYY-MM-DD"),
				AffectedPeriodStartDate: moment(vm.dateModifyRange.startDate).format("YYYY-MM-DD"),
				AffectedPeriodEndDate: moment(vm.dateModifyRange.endDate).format("YYYY-MM-DD"),
				MaximumResults: vm.maxResults
			};
			vm.loading = true;
			ReportsService.getAuditTrailResult.searching(postObj).$promise.then(function (response) {
				vm.loading = false;
				if (response.length < 1) {
					NoticeService.error($translate.instant('noSearchResults'), 5000, true);
					return;
				}
				else {
					vm.changesData = response;
					generateTable();
				}
			});
		}

		function generateTable() {
			vm.gridOptions = {
				exporterCsvFilename: 'audit-trail.csv',
				exporterMenuPdf: false,
				enableSelectAll: false,
				enableFullRowSelection: true,
				enableRowHeaderSelection: false,
				enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
				selectionRowHeaderWidth: 35,
				data: vm.changesData,
				enableGridMenu: true,
				columnDefs: [
					{field: 'ModifiedAt', displayName: $translate.instant('ModifiedAt'), type: 'date', cellFilter: 'date:"dd-MM-yyyy HH:mm"', sort: { direction: 'desc', priority: 0 } },
					{field: 'ModifiedBy', displayName: $translate.instant('ModifiedBy')},
					{field: 'ScheduledAgent', displayName: $translate.instant('ScheduledAgent')},
					{field: 'ShiftType', displayName: $translate.instant('ShiftType')},
					{field: 'AuditType', displayName: $translate.instant('AuditType')},
					{field: 'Detail', displayName: $translate.instant('Details')},
					{field: 'ScheduleStart', displayName: $translate.instant('ScheduleStart'), type: 'date', cellFilter: 'date:"dd-MM-yyyy HH:mm"' },
					{field: 'ScheduleEnd', displayName: $translate.instant('ScheduleEnd'), type: 'date', cellFilter: 'date:"dd-MM-yyyy HH:mm"' }
				]
			};
			vm.chartLoaded = true;
		}

		function refreshData(keyword) {
			vm.gridOptions.data = $filter('filter')(vm.changesData, keyword);
		};
	}
})();
