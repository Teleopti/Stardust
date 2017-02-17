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
            current = current.slice(19);
            var currentElement = [current] + 'isActive';
            vm[currentElement] = true;
        	// ¯\_(ツ)_/¯
        }
        selectCurrentTab();

        function changeCurrentTab(node) {
            //  deselectAllNodes();
            $location.path('StardustDashboard/' + node);
        }
    }
})();
