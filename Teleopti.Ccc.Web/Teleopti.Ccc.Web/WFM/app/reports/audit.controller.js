(function () {
	'use strict';

	angular
		.module('wfm.reports')
		.controller('AuditTrailController', AuditTrailController);

	AuditTrailController.$inject = ['$state', '$filter', 'Toggle', 'uiGridConstants', 'ReportsService'];

	function AuditTrailController($state, $filter, ToggleSvc, uiGridConstants, ReportsService) {
    var vm = this;

		if(!ToggleSvc.WFM_AuditTrail_44006){
       $state.go('main')
    }

		vm.chagedByPerson = ReportsService.getAuditTrailChangedByPerson();

    vm.refreshData = refreshData;

    var changesData = [];

    vm.lastChangeRange = {
       startDate: moment().utc(),
       endDate: moment().utc()
    };

    vm.dateChangeRange = {
       startDate: moment().utc(),
       endDate: moment().utc()
    };

    vm.gridOptions = {
      exporterCsvFilename: 'audit-trail.csv',
      exporterMenuPdf: false,
      enableSelectAll: false,
      enableFullRowSelection: false,
      enableRowHeaderSelection: false,
      enablePaginationControls: false,
      selectionRowHeaderWidth: 35,
      enableHorizontalScrollbar: uiGridConstants.scrollbars.NEVER,
      data: changesData,
      enableGridMenu: true,
      columnDefs: [
        {
          field: 'lastChange',
          sort: {
            direction: uiGridConstants.ASC,
            ignoreSort: false,
            priority: 0
          }
        },
        {
          field: 'oldDate',
          visible: true
        },
        {
          field: 'newDate',
          visible: true
        },
        {
          field: 'forAgent',
          visible: true
        },
        {
          field: 'changedBy',
          visible: true
        },
        {
          field: 'action',
          visible: false
        },
        {
          field: 'details',
          visible: false
        },
        {
          field: 'type',
          visible: false
        }
      ]
    };

    function refreshData(keyword) {
      vm.gridOptions.data = $filter('filter')(changesData, keyword);
    }

	}
})();
