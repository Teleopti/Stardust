(function() {
	angular.module('wfm.teamSchedule').directive('settings', settingsDirective);

	function settingsDirective() {
		return {
			restrict: 'E',
			scope: {},
			controller: settingsCtrl,
			controllerAs: 'vm',
			require: ['^teamscheduleCommandContainer', 'settings'],
			templateUrl: 'app/teamSchedule/commands/teams.directive.cmd.settings.html'
		}
	}

	settingsCtrl.$inject = ['ValidateRulesService'];

	function settingsCtrl(validateRulesService) {
		var vm = this;

		vm.availableValidationRules = [];
		validateRulesService.getAvailableValidationRules().then(function(data){
			data.data.forEach(function(rule){
				vm.availableValidationRules.push({
					type: rule,
					checked: true
				});
				validateRulesService.updateCheckedValidationTypes(rule, true);
			});
		});

		vm.updateValidateRulesToggleState = function(type, checked){
			validateRulesService.updateCheckedValidationTypes(type, checked);
		};

	}
})();
