(function() {
	'use strict';

	angular
		.module('wfm.resourceplanner')
		.config(stateConfig);

	function stateConfig($stateProvider) {
		$stateProvider.state('resourceplanner', {
			url: '/resourceplanner',
			templateUrl: 'app/resourceplanner/resourceplanner.html'
		}).state('resourceplanner.overview', {
			templateUrl: 'app/resourceplanner/planningperiods-overview.html',
			controller: 'ResourcePlannerCtrl'
		}).state('resourceplanner.filter', {
			params: {
				filterId: {},
				periodId: {},
				isDefault: {}
			},
			url: '/dayoffrules',
			templateUrl: 'app/resourceplanner/resourceplanner-filters.html',
			controller: 'ResourceplannerFilterCtrl'
		}).state('resourceplanner.planningperiod', {
			url: '/planningperiod/:id',
			templateUrl: 'app/resourceplanner/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplanner.report', {
			params: {
				result: {},
				interResult: [],
				planningperiod: {}
			},
			url: '/report/:id',
			templateUrl: 'app/resourceplanner/resourceplanner-report.html',
			controller: 'ResourceplannerReportCtrl'
		}).state('resourceplanner.temp', {
			url: '/optimize/:id',
			templateUrl: 'app/resourceplanner/temp.html',
			controller: 'ResourceplannerTempCtrl'
		}).state('resourceplanner.archiveschedule', {
			url: '/archiveschedule',
			templateUrl: 'app/resourceplanner/manageschedule.html',
			controller: 'ResourceplannerManageScheduleCtrl as vm'
		}).state('resourceplanner.importschedule', {
			url: '/importschedule',
			templateUrl: 'app/resourceplanner/manageschedule.html',
			controller: 'ResourceplannerManageScheduleCtrl as vm',
			params: {
				isImportSchedule: true
			}
		}).state('resourceplanner.agentgroups', {   //from here is new
			url: '/agentgroup',
			templateUrl: 'app/resourceplanner/resource_planner_v2/agentgroups.html',
			controller: 'agentGroupsController as vm'
		}).state('resourceplanner.createagentgroup', {
			url: '/createagentgroup',
			templateUrl: 'app/resourceplanner/resource_planner_v2/agentgroup.createform.html',
			controller: 'agentGroupFormController as vm'
		})
		.state('resourceplanner.oneagentroup', {
			url: '/agentgroup/:groupId',
			templateUrl: 'app/resourceplanner/resource_planner_v2/agentgroup.one.html',
			controller: 'agentGroupOneController as vm',
			params: {
				groupId: ''
			}
		});
	}
})();
