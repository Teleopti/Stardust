(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .controller('ImportexportController', ImportexportController);

    ImportexportController.inject = [
        '$state',
        'staffingService',
        'UtilService'
    ];
    function ImportexportController($state, staffingService, UtilService) {
        var vm = this;
        vm.selected;
        vm.openImportData;
        vm.selectedSkill;
        vm.selectedSkillChange = selectedSkillChange;
        vm.querySearchSkills = querySearchSkills;
        vm.exportFile = exportFile;

        var skills;
        getSkills()
        ////////////////

        function getSkills() {
            var query = staffingService.getSkills.query();
            query.$promise.then(function (response) {
                selectSkill(response[0]);
                skills = response;
            })
        }

        function selectedSkillChange(skill) {
            if (skill == null) return;
            selectSkill(skill);
        }

        function selectSkill(skill) {
            vm.selectedSkill = skill;
        }
        function createFilterFor(query) {
            var lowercaseQuery = angular.lowercase(query);
            return function filterFn(item) {
                var lowercaseName = angular.lowercase(item.Name);
                return (lowercaseName.indexOf(lowercaseQuery) === 0);
            };
        };

        function querySearchSkills(query) {
            var results = query ? skills.filter(createFilterFor(query)) : skills,
                deferred;
            return results;
        };

        function exportFile() {
            var request = staffingService.postFileExport.get({ skillId: vm.selectedSkill.Id })
            request.$promise.then(function (response) {
                var data = angular.toJson(response.Content);
                UtilService.saveToFs(response.Content, vm.selectedSkill.Name + ".csv", 'text/csv');
            })
        }


    }
})();