//This is the editor used in PBI #45714 (the old one) this can be removed when the #45727 is released
(function() {
	'use strict';
	angular.module('wfm.skillGroup').controller('SkillGroupController', SkillGroupController);

	SkillGroupController.$inject = [
		'$state',
		'SkillGroupSvc',
		'$filter',
		'NoticeService',
		'$translate',
		'Toggle',
		'skillIconService'
	];

	function SkillGroupController(
		$state,
		SkillGroupSvc,
		$filter,
		NoticeService,
		$translate,
		toggleSvc,
		skillIconService
	) {
		var vm = this;
		vm.skills = [];
		vm.skillAreaName = '';
		vm.getSkillIcon = skillIconService.get;

		vm.exitConfigMode = function() {
			$state.go($state.params.returnState, { isNewSkillArea: false });
		};

		vm.skillSelected = function() {
			var selectedSkills = $filter('filter')(vm.skills, { isSelected: true });
			return selectedSkills.length > 0;
		};

		vm.saveSkillArea = function(form) {
			if (form.$invalid) {
				return;
			}
			var selectedSkills = $filter('filter')(vm.skills, { isSelected: true });

			if (selectedSkills.length <= 0) {
				NoticeService.error($translate.instant('SkillAreaNoSkillSelected'), 5000, false);
				return;
			}
			SkillGroupSvc.createSkillGroup({
				Name: vm.skillAreaName,
				Skills: selectedSkills.map(function(item) {
					return item.Id;
				})
			}).then(function() {
				notifySkillAreaCreation();
				$state.go($state.params.returnState, { isNewSkillArea: true });
			});
		};

		var notifySkillAreaCreation = function() {
			NoticeService.success($translate.instant('Created') + ' ' + vm.skillAreaName, 5000, false);
		};

		SkillGroupSvc.getSkills().then(function(response) {
			vm.skills = response.data;
		});
	}
})();
