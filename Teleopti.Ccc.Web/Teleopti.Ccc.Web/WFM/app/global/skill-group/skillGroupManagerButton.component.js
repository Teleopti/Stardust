(function () {
	'use strict';
	angular.module('wfm.skillGroup').component('skillGroupManagerButton', {
		templateUrl: 'app/global/skill-group/skillGroupManagerButton.html',
		controllerAs: 'vm',
		controller: skillGroupManagerButtonComponentController,
		bindings:{
			onClick: '&'
		}
	});

	angular.module('wfm.skillGroup').controller('SkillGroupManagerButtonComponentController', skillGroupManagerButtonComponentController)
	skillGroupManagerButtonComponentController.$inject = ['$state'];
	
	function skillGroupManagerButtonComponentController($state) {
		var vm = this;

		vm.click = function () {
			vm.onClick();
		};
	}
})();