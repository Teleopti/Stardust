(function() {
    'use strict';
    function IntradayDateOffsetController() {
        var ctrl = this;
        ctrl.dayOffsets = [
            {text: '-7 days', value: -7},
            {text: '-6 days', value: -6},
            {text: '-5 days', value: -5},
            {text: '-4 days', value: -4},
            {text: '-3 days', value: -3},
            {text: '-2 days', value: -2},
            {text: 'Yesterday', value: -1},
            {text: 'Today', value: 0},
            {text: 'Tomorrow', value: 1}
        ];

        ctrl.selOffset = {value: 0};

        ctrl.$onInit = function() {
            ctrl.selOffset = {value: 0};
        };

        ctrl.$onChanges = function(changesObj) {
            if (changesObj.chosenOffset.currentValue.value !== changesObj.chosenOffset.previousValue.value) {
                ctrl.selOffset = changesObj.chosenOffset.currentValue;
            }
        };

        ctrl.dateChange = function(val) {
            ctrl.onDateChange({value: val});
        };
    }

    angular.module('wfm.dateOffset').component('dateOffset', {
        templateUrl: 'app/intraday/components/intradayDateOffset.html',
        controller: IntradayDateOffsetController,
        bindings: {
            chosenOffset: '<',
            onDateChange: '&'
        }
    });
})();
