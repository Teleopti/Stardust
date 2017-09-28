(function () {
    'use strict';
    angular
        .module('wfm.reports')
        .factory('ReportsService', ReportsService);
    ReportsService.$inject = ['$resource'];
    function ReportsService($resource) {
        var categorizedReports = $resource('../api/Reports/NavigationsCategorized');
        var auditTrailChangedByPerson = $resource('../api/Reports/PersonsWhoChangedSchedules');

        var auditTrailResult = $resource('../api/Reports/ScheduleAuditTrailReport', {}, {
          searching: { method: 'POST', params: {}, isArray: true }
        });

        var service = {
            getCategorizedReports: categorizedReports,
            getAuditTrailChangedByPerson: auditTrailChangedByPerson,
            getAuditTrailResult: auditTrailResult
        };
        return service;
    }
})();
