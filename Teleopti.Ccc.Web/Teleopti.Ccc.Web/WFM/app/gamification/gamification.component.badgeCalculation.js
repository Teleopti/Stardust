(function () {
    'use strict';
    angular
        .module('wfm.gamification')
        .component('badgeCalculation', {
            templateUrl: 'app/gamification/html/g.component.badgeCalculation.tpl.html',
            controller: badgeCalculationController,
            controllerAs: '$ctrl',
            bindings: {
                Binding: '=',
            },
        });

    badgeCalculationController.$inject = [];
    function badgeCalculationController() {
        var $ctrl = this;

        $ctrl.$onInit = function () {
            $ctrl.templateType = 'popup';
            var startDate = moment.utc().subtract(2, 'days').toDate();
            var endDate = moment.utc().toDate();
            $ctrl.dateRange = { startDate: startDate, endDate: endDate };

        };
        $ctrl.$onChanges = function (changesObj) { };
        $ctrl.$onDestroy = function () { };
    }
})();