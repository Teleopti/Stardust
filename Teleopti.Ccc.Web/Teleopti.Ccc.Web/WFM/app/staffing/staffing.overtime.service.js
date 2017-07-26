(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .factory('OvertimeService', overtimeService);

    overtimeService.inject = ['staffingService', 'UtilService'];
    function overtimeService(staffingService, utilService) {

        var service = {
       
        };

        return service;

        ////////////////
        
    }
})();