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

    badgeCalculationController.$inject = ['GamificationDataService'];
    function badgeCalculationController(dataService) {
        var $ctrl = this;

        $ctrl.$onInit = function () {
            $ctrl.templateType = 'popup';
            var startDate = moment.utc().subtract(2, 'days').toDate();
            var endDate = moment.utc().toDate();
            $ctrl.dateRange = { startDate: startDate, endDate: endDate };
            $ctrl.dateRangeCustomValidators = [{
                key: 'startDateValidation',
                message: 'StartDateCannotEarlierThanLastPurgeDate',
                validate: function (start, end) {
                    return moment(start).toDate() > moment.utc().subtract(30, 'days').toDate();
                }
            }];
        };

        $ctrl.calculate = function () {
            if ($ctrl.dateRange && $ctrl.dateRange.startDate && $ctrl.dateRange.endDate) {
                var start = moment($ctrl.dateRange.startDate).toDate();
                var end = moment($ctrl.dateRange.endDate).toDate();
                dataService.startCalculation(start, end);
            }
        }

        $ctrl.$onChanges = function (changesObj) { };
        $ctrl.$onDestroy = function () { };
    }
})();