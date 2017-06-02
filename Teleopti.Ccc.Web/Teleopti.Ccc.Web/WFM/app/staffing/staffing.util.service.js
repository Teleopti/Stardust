(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .factory('UtilService', utilService);

    utilService.inject = [];
    function utilService() {
        var service = {
            roundArrayContents: roundArrayContents
        };

        return service;

        ////////////////
        function roundArrayContents(input, decimals) {
            var roundedInput = [];
            input.forEach(function (elm) {
                if (typeof elm != 'number') return
                roundedInput.push(parseFloat(elm.toFixed(decimals)));
            })
            return roundedInput;
        }
    }
})();