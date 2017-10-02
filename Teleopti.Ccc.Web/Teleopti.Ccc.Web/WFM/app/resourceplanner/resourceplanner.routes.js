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
			url: '',
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
		}).state('resourceplanner.settingoverview', {
			params: {
				groupId: ''
			},
			url: '/planninggroup/:groupId/setting/overview',
			templateUrl: 'app/resourceplanner/resource_planner_planning_group_setting/groupsetting.overview.html',
			controller: 'planningGroupSettingOverviewController as vm',
			resolve: {
				planningGroupInfo: ['planningPeriodServiceNew', '$stateParams', function (planningPeriodServiceNew, $stateParams) {
					return planningPeriodServiceNew.getPlanningGroupById({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
						return data;
					});
				}],
				schedulingSettingInfo: ['PlanGroupSettingService', '$stateParams', function (PlanGroupSettingService, $stateParams) {
					return PlanGroupSettingService.getSettingsByPlanningGroupId({ planningGroupId: $stateParams.groupId }).$promise.then(function (data) {
						return data;
					});
				}]
			}
		}).state('resourceplanner.editsetting', {
			params: {
				filterId: '',
				isDefault: undefined,
				groupId: '',
			},
			url: '/planninggroup/:groupId/setting/:filterId',
			templateUrl: 'app/resourceplanner/resource_planner_planning_group_setting/groupsetting.createform.html',
			controller: 'planningGroupSettingEditController as vm'
		}).state('resourceplanner.archiveschedule', {
			url: '/archiveschedule',
			templateUrl: 'app/resourceplanner/manageschedule/manageschedule.html',
			controller: 'ResourceplannerManageScheduleCtrl as vm'
		}).state('resourceplanner.importschedule', {
			url: '/importschedule',
			templateUrl: 'app/resourceplanner/manageschedule/manageschedule.html',
			controller: 'ResourceplannerManageScheduleCtrl as vm',
			params: {
				isImportSchedule: true
			}
		}).state('resourceplanner.planningperiod', { // Bellows are for performance tests
			url: '/planningperiod/:id?runForTest',
			templateUrl: 'app/resourceplanner/web_performance_tests/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplanner.report', {
			params: {
				result: {},
				interResult: [],
				planningperiod: {},
				ranSynchronously: undefined
			},
			url: '/report/:id',
			templateUrl: 'app/resourceplanner/web_performance_tests/resourceplanner-report.html',
			controller: 'ResourceplannerReportCtrl'
		}).state('resourceplanner.temp', {
			url: '/optimize/:id',
			templateUrl: 'app/resourceplanner/web_performance_tests/temp.html',
			controller: 'ResourceplannerTempCtrl'
		});
	}
})();
