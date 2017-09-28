(function () {
	'use strict';

	angular
	.module('wfm.reports')
	.controller('AuditTrailController', AuditTrailController);

	AuditTrailController.$inject = ['$state', '$filter', 'Toggle', 'uiGridConstants', 'ReportsService', '$translate'];

	function AuditTrailController($state, $filter, ToggleSvc, uiGridConstants, ReportsService, $translate) {
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

		init();
		function init() {
			if(!ToggleSvc.WFM_AuditTrail_44006){
				$state.go('main')
			}
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
				AffectedPeriodEndDate: moment(vm.dateModifyRange.endDate).format("YYYY-MM-DD")
			};
			vm.loading = true;
			ReportsService.getAuditTrailResult.searching(postObj).$promise.then(function (response) {
				vm.loading = false;
				if (response.length < 1) {
					// NoticeService.error($translate.instant('noSearchResults'), 5000, true);
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
				enableGridMenu: true
			};
			vm.chartLoaded = true;
		}

		function refreshData(keyword) {
			vm.gridOptions.data = $filter('filter')(vm.changesData, keyword);
		};
	}
})();
