(function() {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .config(stateConfig);

    function stateConfig($stateProvider) {
        $stateProvider.state('skillPrio', {
            url: '/skillprio',
            templateUrl: 'app/skillPrio/skillprio.html',
            controller: 'skillPrioController as vm'
        })
    }
})();