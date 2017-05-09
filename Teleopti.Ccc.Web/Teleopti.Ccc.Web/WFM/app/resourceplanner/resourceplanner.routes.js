(function () {
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
			controller: 'ResourcePlannerCtrl',
			url: '/planningperiods'
		}).state('resourceplanner.filter', {
			params: {
				filterId: {},
				periodId: {},
				isDefault: {},
				groupId: undefined
			},
			url: '/dayoffrules',
			templateUrl: 'app/resourceplanner/resourceplanner-filters.html',
			controller: 'ResourceplannerFilterCtrl'
		}).state('resourceplanner.planningperiod', {
			url: '/planningperiod/:id?runForTest',
			templateUrl: 'app/resourceplanner/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplanner.report', {
			params: {
				result: {},
				interResult: [],
				planningperiod: {},
				ranSynchronously: undefined
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
		}).state('resourceplanner.newoverview', {   //from here is new
			url: '/v2',
			templateUrl: 'app/resourceplanner/resource_planner_v2/resourceplanning.overview.html',
			controller: 'resourceplanningOverviewController as vm'
		}).state('resourceplanner.createagentgroup', {
			url: '/createagentgroup',
			templateUrl: 'app/resourceplanner/resource_planner_agent_group/agentgroup.createform.html',
			controller: 'agentGroupFormController as vm'
		}).state('resourceplanner.editagentgroup', {
			url: '/agentgroup/:groupId/edit',
			templateUrl: 'app/resourceplanner/resource_planner_agent_group/agentgroup.createform.html',
			controller: 'agentGroupFormController as vm',
			params: {
				groupId: ''
			}
		}).state('resourceplanner.selectplanningperiod', {
			url: '/agentgroup/:groupId/selectplanningperiod',
			templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.select.html',
			controller: 'planningPeriodSelectController as vm',
			params: {
				groupId: ''
			}
		}).state('resourceplanner.planningperiodoverview', {
			url: '/agentgroup/:groupId/detail/:ppId',
			templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.overview.html',
			controller: 'planningPeriodOverviewController as vm',
			params: {
				groupId: '',
				ppId: ''
			}
		}).state('resourceplanner.dayoffrulesoverview', {
			params: {
				groupId: undefined
			},
			url: '/agentgroup/:groupId/dayoffrules/overview',
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
			controller: 'dayoffRuleOverviewController as vm'
		}).state('resourceplanner.dayoffrule', {
			params: {
				filterId: '',
				isDefault: undefined,
				groupId: undefined
			},
			url: '/agentgroup/:groupId/dayoffrules/:filterId',
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.createform.html',
			controller: 'dayoffRuleCreateController as vm'
		});
	}
})();
