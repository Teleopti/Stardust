(function () {
    'use strict';

    angular.module('wfm.staffing')
        .directive('staffingImportData', ImportDataDirective)
        .controller('ImportDataCtrl', ['$timeout', 'Toggle', '$translate', ImportDataController]);

    function ImportDataDirective() {
        return {
            controller: 'ImportDataCtrl',
            controllerAs: 'vm',
            bindToController: true,
            scope: {

            },
            templateUrl: 'app/staffing/staffing.importData.template.html'
        };
    };

    function ImportDataController($timeout, toggles, $translate) {
        var vm = this;
        vm.getFileTemplate = getFileTemplate;
        vm.checkValid = checkValid;
        vm.invalidFile = {};
        vm.validFile = {};

        function resetFileLists() {
            vm.invalidFile = {};
            vm.validFile = {};
        }

        function getFileTemplate() {
            //need to add template file here
        }

        function checkValid(file) { 
            if (!file[0])
                return;
            resetFileLists();
            if (file[0].$error && angular.isDefined(file[0].$error)) {
                return vm.invalidFile = file[0];
            } else if (!file[0].$error) {
                // upload file uploadFile(file);
                return vm.validFile = file[0];
            }
        }
    }
}());