﻿(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .config(stateConfig);

    function stateConfig($stateProvider) {
        $stateProvider.state('staffing', {
            params: {
                isNewSkillArea: false,
            },
            url: '/staffing',
            templateUrl: 'app/staffing/staffing.html',
            controller: 'StaffingController as vm'
        })
            .state('staffing-import-export', {
                templateUrl: 'app/staffing/import_export/importexport.overview.html',
                controller: 'ImportexportController as vm'
            })
            .state('bpo-gatekeeper', {
                url: '/bpo',
                templateUrl: 'app/staffing/staffing.html',
                controllerAs: 'vm',
                resolve: {
                    toggles: function (staffingService) {
                        return staffingService;
                    }
                },
                controller: function ($state, staffingService) {
                    var data = staffingService.staffingSettings.get();
                    data.$promise.then(function (response) {
                        if (response.isLicenseAvailable && response.HasPermissionForBpoExchange) {
                            $state.go('staffing-import-export');
                        } else {
                            $state.go('staffing');
                        }
                    });
                }
            })
            .state("staffing.skill-area-config", {
                params: {
                    isNewSkillArea: false,
                    returnState: "staffing"
                },
                url: "/skill-area-config",
                templateUrl: "app/global/skill-group/skill-group-manager.html",
                controller: "SkillGroupManagerController as vm"
              })
    }
})();
