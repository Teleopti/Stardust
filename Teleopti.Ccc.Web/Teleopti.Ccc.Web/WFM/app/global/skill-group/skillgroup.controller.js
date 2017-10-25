(function () {
	'use strict';
	angular.module('wfm.skillGroup').controller('SkillGroupController', SkillGroupController);

	SkillGroupController.$inject = [
		'$state',
		'SkillGroupSvc',
		'$filter',
		'NoticeService',
		'$translate',
		'Toggle',
		'skillIconService'];

	function SkillGroupController($state, SkillGroupSvc, $filter, NoticeService, $translate, toggleSvc, skillIconService) {
		var vm = this;
		vm.skills = [];
		vm.skillAreaName = '';
		vm.getSkillIcon = skillIconService.get;
		vm.toggles = {
			unifiedSkillGroupManagement: []
		};

		vm.exitConfigMode = function () {
			$state.go($state.params.returnState, {isNewSkillArea: false});
		};

		vm.skillSelected = function () {
			var selectedSkills = $filter('filter')(vm.skills, {isSelected: true});
			var selectedSkillIds = selectedSkills.map(function (skill) {
				return skill.Id;
			});
			return selectedSkillIds.length > 0;
		};

		vm.saveSkillArea = function (form) {
			if (form.$invalid) {
				return;
			}
			var selectedSkills = $filter('filter')(vm.skills, {isSelected: true});

			var selectedSkillIds = selectedSkills.map(function (skill) {
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
				.$promise.then(function (result) {
				notifySkillAreaCreation();
				$state.go($state.params.returnState, {isNewSkillArea: true});
			});
		};

		toggleSvc.togglesLoaded.then(function () {
			vm.toggles.unifiedSkillGroupManagement = toggleSvc.WFM_Unified_Skill_Group_Management_45417;
		});

		var notifySkillAreaCreation = function () {
			NoticeService.success($translate.instant('Created') + ' ' + vm.skillAreaName, 5000, false);
		};

		SkillGroupSvc.getSkills.query().$promise.then(function (skills) {
			vm.skills = skills;
		});
	}
})();
