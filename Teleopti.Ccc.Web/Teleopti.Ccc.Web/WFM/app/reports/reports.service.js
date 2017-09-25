(function () {
    'use strict';
    angular
        .module('wfm.reports')
        .factory('ReportsService', ReportsService);
    ReportsService.$inject = ['$resource'];
    function ReportsService($resource) {
        var categorizedReports = $resource('../api/Reports/NavigationsCategorized');
        var auditTrailChangedByPerson = $resource('../api/Reports/PersonsWhoChangedSchedules');
        var service = {
            getCategorizedReports: categorizedReports,
            getAuditTrailChangedByPerson: auditTrailChangedByPerson
        };
        return service;
    }
})();
