(function () {
	'use strict';

	angular
	.module('wfm.reports')
	.controller('AuditTrailController', AuditTrailController);

	AuditTrailController.$inject = ['$filter', 'Toggle', 'uiGridConstants', 'ReportsService', 'NoticeService', '$translate', 'localeLanguageSortingService'];

	function AuditTrailController($filter, ToggleSvc, uiGridConstants, ReportsService, NoticeService, $translate, localeLanguageSortingService) {
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
			startDate: moment().utc().subtract(1, 'days'),
			endDate: moment().utc()
		};
		vm.changesData = [];

		vm.filteredOrgData = [];
		vm.orgData = {};
		vm.option = {
			NodeDisplayName: "Name",
			NodeChildrenName: "Children",
			NodeSelectedMark: "selected"
		}

		vm.sendForm = sendForm;
		vm.refreshData = refreshData;
		vm.calculateOrgSelection = calculateOrgSelection;
		vm.getOrgData = getOrgData;
		vm.maxResults = 10000;

		vm.label = $translate.instant('SeveralTeamsSelected').replace('{0}', vm.filteredOrgData.length )

		init();
		function init() {
			getChangedBy();
			getOrgData();
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

		function getOrgData() {
			if (angular.isUndefined(vm.dateModifyRange)) {
				return;
			}
			var postObj = {
				startDate: moment(vm.dateModifyRange.startDate).format("YYYY-MM-DD"),
				endDate: moment(vm.dateModifyRange.endDate).format("YYYY-MM-DD"),
			}
			ReportsService.getOrganization.org(postObj).$promise.then(function (response) {
				vm.orgData = {
					Children: []
				};

				if (response.length > 0) {
					for (var i = 0; i < response.length; i++) {
						vm.orgData.Children.push(response[i]);
					}
				}
			});
		}

		function sendForm(form) {
			var postObj =	{
				ChangedByPersonId: form.drop.Id,
				ChangesOccurredStartDate: moment(vm.dateChangeRange.startDate).format("YYYY-MM-DD"),
				ChangesOccurredEndDate: moment(vm.dateChangeRange.endDate).format("YYYY-MM-DD"),
				AffectedPeriodStartDate: moment(vm.dateModifyRange.startDate).format("YYYY-MM-DD"),
				AffectedPeriodEndDate: moment(vm.dateModifyRange.endDate).format("YYYY-MM-DD"),
				MaximumResults: vm.maxResults,
				TeamIds: vm.filteredOrgData
			};
			vm.loading = true;
			ReportsService.getAuditTrailResult.searching(postObj).$promise.then(function (response) {
				vm.loading = false;
				if (response.length < 1) {
					NoticeService.warning($translate.instant('noSearchResults'), 5000, true);
					return;
				}
				else {
					vm.changesData = response;
					generateTable();

					if (response.length >= vm.maxResults) {
						NoticeService.info($translate.instant('MaxResultsReached').replace('{0}', vm.maxResults), 5000, true);
					}
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
					{field: 'ShiftType', displayName: $translate.instant('Type')},
					{field: 'AuditType', displayName: $translate.instant('Action')},
					{field: 'Detail', displayName: $translate.instant('Details')},
					{field: 'ScheduleStart', displayName: $translate.instant('StartTime'), type: 'date', cellFilter: 'date:"dd-MM-yyyy HH:mm"' },
					{field: 'ScheduleEnd', displayName: $translate.instant('FullDayAbsenceReqEndTime'), type: 'date', cellFilter: 'date:"dd-MM-yyyy HH:mm"' }
				]
			};
			vm.chartLoaded = true;
		}

		function calculateOrgSelection(nodes) {
			for (var i = 0; i < nodes.length; i++) {
				if (angular.isUndefined(nodes[i].Children)) {
					if (vm.filteredOrgData.indexOf(nodes[i].Id) == -1 && nodes[i].selected === true) {
						vm.filteredOrgData.push(nodes[i].Id)
					}
					else if(vm.filteredOrgData.indexOf(nodes[i].Id) !== -1 && nodes[i].selected === false){
						vm.filteredOrgData.splice(vm.filteredOrgData.indexOf(nodes[i].Id), 1)
					}
				}
				else if (nodes[i].Children){
					calculateOrgSelection(nodes[i].Children);
				}
			}
			vm.label = $translate.instant('SeveralTeamsSelected').replace('{0}', vm.filteredOrgData.length )
		}

		function refreshData(keyword) {
			vm.gridOptions.data = $filter('filter')(vm.changesData, keyword);
		};
	}
})();
