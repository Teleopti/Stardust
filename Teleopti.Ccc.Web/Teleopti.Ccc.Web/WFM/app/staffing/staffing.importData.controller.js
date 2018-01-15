(function () {
    'use strict';

    angular.module('wfm.staffing')
        .directive('staffingImportData', ImportDataDirective)
        .controller('ImportDataController', ['$timeout', 'Toggle', '$translate', 'staffingService', 'UtilService', ImportDataController]);

    function ImportDataDirective() {
        return {
            controller: 'ImportDataController',
            controllerAs: 'vm',
            bindToController: true,
            scope: {

            },
            templateUrl: 'app/staffing/staffing.importData.template.html'
        };
    };

    function ImportDataController($timeout, toggles, $translate, staffingService, utilService) {
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
			  var templateFile = 'source,skillcombination,startdatetime,enddatetime,agents\r\n' +
		        'Generic,Email,2017-08-01 11:00,2017-08-01 11:15,8.75\r\n' +
		        'Generic,Channel Sales|Directsales,2017-08-01 10:00,2017-08-01 10:15,12.5\r\n' +
		        'Generic,Channel Sales,2017-08-01 10:00,2017-08-01 10:15,8.75';
	        utilService.saveToFs(templateFile, "template.csv", 'text/csv');

        }

        function readFile(input) {
            var fileReader = new FileReader();
            fileReader.onload = function (event) {
                var data = angular.toJson(event.currentTarget.result);
                var query = staffingService.importbpo.save(data);
                vm.isSuccessful = false;
                vm.isFailed = false;
                vm.errors = [];
                query.$promise.then(function (response) {
                    if (response.Success) {
                        vm.isSuccessful = true;
                    } else {
                        vm.isFailed = true;
                        vm.errors = response.ErrorInformation;
                    }
                })
            }
            fileReader.onerror = function (event) {
                //console.log(event);
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