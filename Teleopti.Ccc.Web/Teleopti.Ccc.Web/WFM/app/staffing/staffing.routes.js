(function() {
    'use strict';

    angular
        .module('wfm.staffing')
        .config(stateConfig);

    function stateConfig($stateProvider) {
        $stateProvider.state('staffing', {
            url: '/staffing',
            templateUrl: 'app/staffing/staffing.html',
            controller: 'StaffingController as vm'
        });
    }
})();
