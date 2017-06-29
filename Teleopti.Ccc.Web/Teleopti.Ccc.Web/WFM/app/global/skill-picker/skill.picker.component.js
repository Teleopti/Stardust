(function () {
  'use strict';

  angular
    .module('wfm.skillPicker', [])
    .component('skillPicker', {
      templateUrl: 'app/global/skill-picker/skill-picker.html',
      controller: SkillPickerComponentController,
      bindings: {
        skills: '=',
        skillAreas: '=',
        itemToReturn: '='
      },
    });

  SkillPickerComponentController.inject = [];
  function SkillPickerComponentController() {
    var ctrl = this;

    ctrl.selectSkill = function (skill) {
      if (skill !== undefined) {
        ctrl.selectedSkillArea = null;
        ctrl.itemToReturn(skill);
      } else if (ctrl.selectedSkillArea === null && ctrl.selectedSkill === null) {
          ctrl.itemToReturn(undefined);
        } 
    }

    ctrl.selectSkillArea = function (skillArea) {
      if (skillArea !== undefined) {
        ctrl.selectedSkill = null;
        ctrl.itemToReturn(skillArea);
      } else if (ctrl.selectedSkill === null && ctrl.selectedSkillArea === null) {
          ctrl.itemToReturn(undefined);
        } 
      }
  }
})();