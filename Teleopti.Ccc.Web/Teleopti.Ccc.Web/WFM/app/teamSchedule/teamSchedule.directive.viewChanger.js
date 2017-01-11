(function (document, angular) {

    'use strict';

    angular.module('wfm.teamSchedule').directive('viewChanger', viewChangerDirective);

    function viewChangerDirective() {
        return {
            restrict: 'E',
            templateUrl: 'app/teamSchedule/html/view-changer.tpl.html',
            replace: true,
            controllerAs: 'vc',
            scope: {
                selectedDate: '=?',
                keyword: '=?',
                selectedTeamIds: '=?'
            },
            bindToController: true,
            controller: viewChangerController
        };
    }

    viewChangerController.$inject = ['$state'];

    function viewChangerController($state) {
        var vc = this;

        vc.viewState = $state.current.name;

        vc.viewStateMap = {
            'day': 'teams.dayView',
            'week': 'teams.weekView'
        };

        vc.changeView = function (viewState) {
            var params = {};
            params.do = true;

            if (vc.keyword) params.keyword = vc.keyword;
            if (vc.selectedDate) params.selectedDate = vc.selectedDate;
            if (vc.selectedTeamIds) params.selectedTeamIds = vc.selectedTeamIds;

            $state.go(viewState, params);
        };
    }

})(document, angular);
