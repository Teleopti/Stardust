(function (document, angular) {

    'use strict';

    angular.module('wfm.teamSchedule')
    .directive('viewChanger', viewChangerDirective);

    function viewChangerDirective() {
        return {
            restrict: 'E',
            templateUrl: 'js/teamSchedule/html/view-changer.tpl.html',
            replace: true,
            controllerAs: 'vc',
            scope: {
                scheduleDate: '=?'
            },
            bindToController: true,
            controller: viewChangerController,
        };
    }

    viewChangerController.$inject = ['$state'];

    function viewChangerController($state) {
        var vc = this;

        vc.viewState = $state.current.name;

        vc.viewStateMap = {
            'day': 'myTeamSchedule.start',
            'week': 'myTeamSchedule.weekView'
        };

        vc.changeView = function (viewState) {
            if (vc.scheduleDate) {
                $state.go(viewState, {selectedDate: vc.scheduleDate});
                return;
            }
            $state.go(viewState);
        };
    }

})(document, angular);
