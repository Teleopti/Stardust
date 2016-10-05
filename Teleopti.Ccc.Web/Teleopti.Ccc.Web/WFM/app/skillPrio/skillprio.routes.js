(function() {
    'use strict';

    angular
        .module('wfm.skillPrio')
        .config(stateConfig);

    function stateConfig($stateProvider) {
        $stateProvider.state('skillPrio', {
            url: '/skillprio',
            templateUrl: 'app/skillPrio/skillPrio.html',
            controller: 'skillPrioController as vm'
        })
    }
})();