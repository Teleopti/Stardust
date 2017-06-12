(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupsDetailController', Controller)
		.directive('planninggroupDetail', planninggroupDetailDirective);

	Controller.$inject = ['$stateParams', 'planningGroupService'];

	function Controller($stateParams, planningGroupService) {
		var vm = this;
	}
	
	function planninggroupDetailDirective() {
		var directive = {
			restrict: 'EA',
			scope: {
				planningGroup: '='
			},
			templateUrl: 'app/resourceplanner/resource_planner_planning_group/planninggroup.detail.html',
			controller: 'planningGroupsDetailController as vm',
			bindToController: true
		};
		return directive;
	}
})();
