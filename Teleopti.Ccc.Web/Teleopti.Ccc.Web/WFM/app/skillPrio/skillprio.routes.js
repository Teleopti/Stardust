(function () {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .config(stateConfig);

    function stateConfig($stateProvider) {
        $stateProvider.state('skillPrio', {
            url: '/skillprio',
            templateUrl: 'app/skillPrio/skillprio.new.html',
            controller: 'skillPrioControllerNew as vm'
        });
    }
})();
