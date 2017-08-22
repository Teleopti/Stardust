(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .controller('ImportexportController', ImportexportController);

    ImportexportController.inject = [
        '$state',
        'staffingService'
    ];
    function ImportexportController($state, staffingService) {
        var vm = this;
        vm.selected;
        vm.openImportData;
        vm.selectedSkill;
        vm.selectedSkillChange = selectedSkillChange;
        vm.querySearchSkills = querySearchSkills;

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
        
		function querySearchSkills(query) {
			var results = query ? skills.filter(createFilterFor(query)) : skills,
				deferred;
			return results;
		};


    }
})();