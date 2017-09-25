// @ts-check
(function() {
    'use strict';

    angular.module('wfm.skillPicker', []).component('skillPicker', {
        templateUrl: 'app/global/skill-picker/skill-picker.html',
        controller: SkillPickerComponentController,
        bindings: {
            skills: '=',
            skillAreas: '=',
            itemToReturn: '=',
            preselectedItem: '='
        }
    });

    SkillPickerComponentController.$inject = ['Toggle', '$state'];

    function SkillPickerComponentController(toggleSvc, $state) {
        var ctrl = this;

        ctrl.toggles = {
            unifiedSkillGroupManagement: []
        };

        // behavior test
        ctrl.skillsLoaded = true;
        ctrl.skillAreasLoaded = true;
        // end

        ctrl.$onInit = function() {
            if (angular.isDefined(ctrl.preselectedItem.skillIds)) {
                ctrl.selectedSkill = ctrl.skills.find(function(skill) {
                    return skill.Id === ctrl.preselectedItem.skillIds[0];
                });
            } else {
                ctrl.selectedSkillArea = ctrl.skillAreas.find(function(sa) {
                    return sa.Id === ctrl.preselectedItem.skillAreaId;
                });
            }
            toggleSvc.togglesLoaded.then(function() {
                ctrl.toggles.unifiedSkillGroupManagement = toggleSvc.WFM_Unified_Skill_Group_Management_45417;
            });
        };

        ctrl.selectSkill = function(skill) {
            if (angular.isDefined(skill)) {
                ctrl.selectedSkillArea = null;
                ctrl.itemToReturn(skill);
            } else if (ctrl.selectedSkillArea === null && ctrl.selectedSkill === null) {
                ctrl.itemToReturn(undefined);
            }
        };

        ctrl.selectSkillArea = function(skillArea) {
            if (angular.isDefined(skillArea)) {
                ctrl.selectedSkill = null;
                ctrl.itemToReturn(skillArea);
            } else if (ctrl.selectedSkill === null && ctrl.selectedSkillArea === null) {
                ctrl.itemToReturn(undefined);
            }
        };

        ctrl.configMode = function() {
			console.log('$state', $state)
			
            $state.go('rta-skill-area-config', {
                isNewSkillArea: false
            });
        };
    }
})();
