angular.module('wfm.resourceplanner').component('wfmWeekMonthGroup', {
    templateUrl: 'app/resourceplanner/components/week-month-group.tpl.html',
    controller: WeekMonthGroupCtrl,
    bindings: {
        items: '<',
        selected: '<',
        output: '<',
        btnClass: '<',
        selectionClass: '<'
    }
});

function WeekMonthGroupCtrl($translate) {
    var ctrl = this;

    ctrl.$onInit = function () {
        if (ctrl.selected) {
            ctrl.output(ctrl.selected);
        }
    };
};