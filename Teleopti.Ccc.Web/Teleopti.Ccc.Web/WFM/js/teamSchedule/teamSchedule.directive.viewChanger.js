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
                selectedDate: '=?',
                keyword: '=?'
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
            var params = {};

            if (vc.keyword) params.keyword = vc.keyword;
            if (vc.selectedDate) params.selectedDate = vc.selectedDate;

            $state.go(viewState, params);
        };
    }

})(document, angular);
