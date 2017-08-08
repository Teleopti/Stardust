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
			templateUrl: 'app/resourceplanner/resource_planner_planning_group/planninggroups.html',
			controller: 'planningGroupsController as vm',
			resolve: {
				planningGroups: ['planningGroupService', '$stateParams', function (planningGroupService, $stateParams) {
					return planningGroupService.getPlanningGroups().$promise.then(function (data) {
						return data;
					});
				}]
			}
		}).state('resourceplanner.createplanninggroup', {
			url: '/createplanninggroup',
			templateUrl: 'app/resourceplanner/resource_planner_planning_group/planninggroup.createform.html',
			controller: 'planningGroupFormController as vm',
			resolve: {
				editPlanningGroup: function () {
					return null;
				}
			}
		}).state('resourceplanner.editplanninggroup', {
			url: '/planninggroup/:groupId/edit',
			templateUrl: 'app/resourceplanner/resource_planner_planning_group/planninggroup.createform.html',
			controller: 'planningGroupFormController as vm',
			params: {
				groupId: ''
			},
			resolve: {
				editPlanningGroup: ['planningGroupService', '$stateParams', function (planningGroupService, $stateParams) {
					return planningGroupService.getPlanningGroupById({ id: $stateParams.groupId }).$promise;
				}]
			}
		}).state('resourceplanner.selectplanningperiod', {
			url: '/planninggroup/:groupId/selectplanningperiod',
			templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.select.html',
			controller: 'planningPeriodSelectController as vm',
			params: {
				groupId: ''
			},
			resolve: {
				planningPeriods: ['planningPeriodServiceNew', '$stateParams', function (planningPeriodServiceNew, $stateParams) {
					return planningPeriodServiceNew.getPlanningPeriodsForPlanningGroup({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
						return data;
					});
				}],
				planningGroupInfo: ['planningPeriodServiceNew', '$stateParams', function (planningPeriodServiceNew, $stateParams) {
					return planningPeriodServiceNew.getPlanningGroupById({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
						return data;
					});
				}]
			}
		}).state('resourceplanner.planningperiodoverview', {
			url: '/planninggroup/:groupId/detail/:ppId',
			templateUrl: 'app/resourceplanner/resource_planner_planning_period/planningperiod.overview.html',
			controller: 'planningPeriodOverviewController as vm',
			params: {
				groupId: '',
				ppId: ''
			},
			resolve: {
				selectedPp: ['planningPeriodServiceNew', '$stateParams', function (planningPeriodServiceNew, $stateParams) {
					return planningPeriodServiceNew.getPlanningPeriod({ id: $stateParams.ppId }).$promise.then(function (data) {
						return data;
					});
				}],
				planningGroupInfo: ['planningPeriodServiceNew', '$stateParams', function (planningPeriodServiceNew, $stateParams) {
					return planningPeriodServiceNew.getPlanningGroupById({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
						return data;
					});
				}]
			}
		}).state('resourceplanner.dayoffrulesoverview', {
			params: {
				groupId: ''
			},
			url: '/planninggroup/:groupId/dayoffrules/overview',
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.overview.html',
			controller: 'dayoffRuleOverviewController as vm',
			resolve: {
				planningGroupInfo: ['planningPeriodServiceNew', '$stateParams', function (planningPeriodServiceNew, $stateParams) {
					return planningPeriodServiceNew.getPlanningGroupById({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
						return data;
					});
				}],
				dayOffRulesInfo: ['dayOffRuleService', '$stateParams', function (dayOffRuleService, $stateParams) {
					return dayOffRuleService.getDayOffRulesByPlanningGroupId({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
						return data;
					});
				}]
			}
		}).state('resourceplanner.dayoffrule', {
			params: {
				filterId: '',
				isDefault: undefined,
				groupId: '',
				EditDoRule: ''
			},
			url: '/planninggroup/:groupId/dayoffrules/:filterId',
			templateUrl: 'app/resourceplanner/resource_planner_day_off_rule/dayoffrule.createform.html',
			controller: 'dayoffRuleCreateController as vm'
		});
	}
})();
