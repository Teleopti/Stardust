(function () {
    'use strict';

    angular.module('wfm.staffing')
        .directive('staffingImportData', ImportDataDirective)
        .controller('ImportDataCtrl', ['$timeout', 'Toggle', '$translate', 'staffingService', ImportDataController]);

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

    function ImportDataController($timeout, toggles, $translate, staffingService) {
        var vm = this;
        vm.getFileTemplate = getFileTemplate;
        vm.checkValid = checkValid;
        vm.invalidFile = {};
        vm.validFile = {};
        vm.errors = [];

        function resetFileLists() {
            vm.invalidFile = {};
            vm.validFile = {};
        }

        function getFileTemplate() {
            //need to add template file here
        }

        function readFile(input) {
            var fileReader = new FileReader();
            fileReader.onload = function (event) {
                var data = JSON.stringify(event.currentTarget.result);
                var query = staffingService.importbpo.save(data);
                vm.isSuccessful = false;
                vm.isFailed = false;
                vm.errors = [];
                query.$promise.then(function(response){
                    if(response.Success){
                        vm.isSuccessful = true;
                    }else{
                        vm.isFailed = true;
                        vm.errors = response.ErrorInformation;
                    }
                })
            }
            fileReader.onerror = function(event){
                console.log(event);
            }
            var result = fileReader.readAsText(input[0]);

        }

        function checkValid(file) {
            if (!file[0])
                return;
            resetFileLists();
            if (file[0].$error && angular.isDefined(file[0].$error)) {
                return vm.invalidFile = file[0];
            } else if (!file[0].$error) {
                readFile(file);
                return vm.validFile = file[0];
            }
        }
    }
}());