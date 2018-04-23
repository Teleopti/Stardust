(function() {
	angular.module('wfm.intraday').component('theSkillPicker', {
		templateUrl: 'app/intraday/components/skillPicker.html',
		controller: theComponent,
		bindings: {
			skills: '<', //The list of skills to display, each object should contain at least Name and Id.
			skillGroups: '<', //The list of skillgroups to display, each object should contain at least Name and Id.
			preselectedSkill: '<', //The skill to display as the component loads
			preselectedSkillGroup: '<', //The skillgroups to display as the component loads
			onSkillSelected: '&', //Function to execute when skill is selected
			onSkillGroupSelected: '&', //Function to execute when skillgroup is selected
			onClearSkillSelection: '&', //Function to execute when skill selection is cleared (x)
			onClearSkillGroupSelection: '&' //Function to execute when skillgroup selection is cleared (x)
		}
	});

	theComponent.$inject = ['$translate', '$log'];

	function theComponent($translate, $log) {
		var ctrl = this;

		ctrl.selectedSkill = '';
		ctrl.selectedSkillGroup = '';

		ctrl.skillSelected = function(skill) {
			ctrl.skillPickerOpen = false;
			ctrl.skillPickerText = skill.Name;
			ctrl.skillGroupPickerText = '';
			ctrl.onSkillSelected({ skill: skill });
		};

		ctrl.clearSkillSelection = function() {
			ctrl.skillPickerOpen = false;
			ctrl.skillPickerText = '';
			ctrl.onClearSkillSelection();
		};

		ctrl.skillGroupSelected = function(skillGroup) {
			ctrl.skillGroupPickerText = skillGroup.Name;
			ctrl.skillGroupPickerOpen = false;
			ctrl.skillPickerText = '';
			ctrl.onSkillGroupSelected({ skillGroup: skillGroup });
		};

		ctrl.clearSkillGroupSelection = function() {
			ctrl.skillGroupPickerOpen = false;
			ctrl.skillGroupPickerText = '';
			ctrl.onClearSkillGroupSelection();
		};

		ctrl.$onChanges = function(changesObj) {
			if (
				angular.isDefined(changesObj.preselectedSkill) &&
				changesObj.preselectedSkill !== null &&
				changesObj.preselectedSkill.currentValue !== null
			)
				ctrl.skillPickerText = changesObj.preselectedSkill.currentValue.Name;
			if (
				angular.isDefined(changesObj.preselectedSkillGroup) &&
				changesObj.preselectedSkillGroup !== null &&
				changesObj.preselectedSkillGroup.currentValue !== null
			)
				ctrl.skillGroupPickerText = changesObj.preselectedSkillGroup.currentValue.Name;
		};
	}
})();
