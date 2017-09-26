(function () {
	'use strict';
	angular.module('wfm.skillGroup').component('skillGroupButton', {
		templateUrl: 'app/global/skill-group/skillGroupButton.html',
		controllerAs: 'vm',
		controller: skillGroupButtonComponentController,
		bindings:{
			onClick: '&'
		}
	});

	angular.module('wfm.skillGroup').controller('SkillGroupButtonComponentController', skillGroupButtonComponentController)
	skillGroupButtonComponentController.$inject = ['$state'];
	
	function skillGroupButtonComponentController($state) {
		var vm = this;

		vm.click = function () {
			vm.onClick();
		};
	}
})();