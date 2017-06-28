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
        output: '='
      },
    });

  SkillPickerComponentController.inject = [];
  function SkillPickerComponentController() {
    var ctrl = this;

    ctrl.selectSkill = function (skill) {
      if(ctrl.selectedSkill === null){
        ctrl.itemToReturn = undefined;
        return;
      }
      
      ctrl.selectedSkillArea = null;
      ctrl.itemToReturn = skill;
    }

    ctrl.selectSkillArea = function (skillArea) {
      if(ctrl.selectedSkillArea === null){
        ctrl.itemToReturn = undefined;
        return;
      }
      
      ctrl.selectedSkill = null;
      ctrl.itemToReturn = skillArea;
    }

  }
})();