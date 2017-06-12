(function() {
    'use strict';
    function IntradayDateOffsetController($filter) {
        var ctrl = this;
		
			ctrl.dayOffsets = [];

        var off = -7;
        for (var i = -7; i < 2; i++) {
            ctrl.dayOffsets.push({
                text: getText(i),
                value: i,
				display: getDisplay(i)
            });
        }

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

		function getDisplay(value) {
			if(value <= 0) return value + '';
			else return '+' + value;
		}

        function getText(offset) {
            return moment().add(offset, 'days').format('dddd, LL');
        }
    }

    angular.module('wfm.dateOffset').component('dateOffset', {
        templateUrl: 'app/intraday/components/intradayDateOffset.html',
        controller: IntradayDateOffsetController,
        bindings: {
            chosenOffset: '<',
            onDateChange: '&'
        }
    });

    IntradayDateOffsetController.$inject = ['$filter'];
})();
