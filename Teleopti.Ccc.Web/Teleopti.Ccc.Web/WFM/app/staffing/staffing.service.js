(function() {
'use strict';

    angular
        .module('wfm.staffing')
        .service('staffingService', staffingService);

    staffingService.inject = [];
    function staffingService() {
        this.exposedFn = exposedFn;
        
        ////////////////
        function exposedFn() { }
    }
})();