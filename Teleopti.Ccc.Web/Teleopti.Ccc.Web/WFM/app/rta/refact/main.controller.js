(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaMainController', RtaMainController);

	RtaMainController.$inject = ['rtaService', '$state', '$stateParams'];

	function RtaMainController(rtaService, $state, $stateParams) {
		var vm = this;
		vm.skillIds = $stateParams.skillIds || [];
		$stateParams.open = ($stateParams.open || "false");
		vm.skills = [];
		vm.skillAreas = [];
		vm.organization = [];
		vm.siteCards = [];
		(function fetchDataForFilterComponent() {
			rtaService.getSkills().then(function (result) {
				vm.skills = result;
			});

			rtaService.getSkillAreas().then(function (result) {
				vm.skillAreas = result.SkillAreas;
			});
			if (vm.skillIds.length > 0) {
				rtaService.getOrganizationForSkills({ skillIds: vm.skillIds }).then(function (result) {
					vm.organization = result;
				});
			} else {
				rtaService.getOrganization().then(function (result) {
					vm.organization = result;
				});
			}
		})();

		(function fetchDataForOverviewComponent(){
			rtaService.getSiteCardsFor().then(function(result){
				vm.siteCards = result;
				vm.siteCards.forEach(function(site){
					site.isOpen = $stateParams.open != "false";
				});

			});
		})();

	}
})();
