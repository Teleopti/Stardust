(function () {
    'use strict';

    angular
        .module('adminApp')
        .controller('menuController', menuController, []);

    function menuController($location) {
        /* jshint validthis:true */
        var vm = this;
        vm.changeCurrentTab = changeCurrentTab;

        function deselectAllNodes() {
            vm.startPageisActive = false;
            vm.nodesisActive = false;
            vm.queueisActive = false;
            vm.jobs_isActive = false;
        }
        function selectCurrentTab() {
            var current = $location.path();
            if (!current.indexOf('StardustDashboard/')) return;
            current = current.slice(19,24);
            var currentElement = [current] + 'isActive';
            vm[currentElement] = true;


        }
        selectCurrentTab();

        function changeCurrentTab(node) {
            //  deselectAllNodes();
            $location.path('StardustDashboard/' + node);
        }
    }
})();
