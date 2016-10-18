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
            vm.jobsisActive = false;
        }
        function selectCurrentTab(){
            var current = $location.path();
            current = current.slice(19);
            var currentElement = [current] + 'isActive';
            vm[currentElement] = true;
        }
        selectCurrentTab();

        function changeCurrentTab(node) {
          //  deselectAllNodes();
            $location.path('StardustDashboard/'+node);
        }

    }
})();
