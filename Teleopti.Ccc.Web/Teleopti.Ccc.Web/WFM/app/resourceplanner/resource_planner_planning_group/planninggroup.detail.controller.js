(function () {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.controller('planningGroupsDetailController', Controller)
		.directive('planninggroupDetail', planninggroupDetailDirective);

	Controller.$inject = ['$stateParams', 'planningGroupService'];

	function Controller($stateParams, planningGroupService) {
		var vm = this;

		vm.organizationFilters = vm.planningGroup.Filters.filter(function(filter){
			return filter.FilterType == 'site' || filter.FilterType == 'team';
		});

		vm.skillFilters = vm.planningGroup.Filters.filter(function(filter){
			return filter.FilterType == 'skill';
		});

		vm.contractFilters = vm.planningGroup.Filters.filter(function(filter){
			return filter.FilterType == 'contract';
		});
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
