(function() {
    'use strict'

    function RtaCardBodyController() {
        var ctrl = this

        var classes = ctrl.classes != null ? cleanStrArr(ctrl.classes.split(' ')) : []
        ctrl.allClasses = classes.join(' ')
    }

    function cleanStrArr(arr) {
        return arr
            .map(function(str) { return str.trim() })
            .filter(function(str) { return str != "" });
    }

    angular
        .module('wfm.rta')
        .component('rtaCardBody', {
            templateUrl: 'app/rta/historical/rta-card-body-component.html',
            controller: RtaCardBodyController,
            transclude: true,
            bindings: {
                classes: '@',
                styles: '@'
            }
        })
}());