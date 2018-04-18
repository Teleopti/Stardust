(function() {
	'use strict';
	angular.module('wfm.intraday').controller('IntradayConfigController', IntradayConfigController);

	IntradayConfigController.$inject = [
		'$state',
		'intradayService',
		'SkillGroupSvc',
		'$filter',
		'NoticeService',
		'$translate',
		'Toggle',
		'skillIconService'
	];

	function IntradayConfigController(
		$state,
		intradayService,
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
			$state.go('intraday', { isNewSkillArea: false });
		};

		vm.skillSelected = function() {
			var selectedSkills = $filter('filter')(vm.skills, { isSelected: true });
			var selectedSkillIds = selectedSkills.map(function(skill) {
				return skill.Id;
			});
			return selectedSkillIds.length > 0;
		};

		vm.saveSkillArea = function(form) {
			if (form.$invalid) {
				return;
			}
			var selectedSkills = $filter('filter')(vm.skills, { isSelected: true });

			var selectedSkillIds = selectedSkills.map(function(skill) {
				return skill.Id;
			});

			if (selectedSkillIds.length <= 0) {
				NoticeService.error($translate.instant('SkillAreaNoSkillSelected'), 5000, false);
				return;
			}

			SkillGroupSvc.createSkillGroup
				.query({
					Name: vm.skillAreaName,
					Skills: selectedSkillIds
				})
				.$promise.then(function() {
					notifySkillAreaCreation();
					$state.go('intraday', { isNewSkillArea: true });
				});
		};

		toggleSvc.togglesLoaded.then(function() {
			vm.toggles = toggleSvc;
		});

		var notifySkillAreaCreation = function() {
			NoticeService.success($translate.instant('Created') + ' ' + vm.skillAreaName, 5000, false);
		};

		SkillGroupSvc.getSkills().then(function(skills) {
			vm.skills = skills;
		});
	}
})();
