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
				planningGroups: ['planningGroupService', function (planningGroupService) {
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
				editPlanningGroup: function() {
					return null;
				},
				schedulingSettingInfo: function() {
					return [{"MinDayOffsPerWeek":1,"MaxDayOffsPerWeek":3,"MinConsecutiveWorkdays":2,"MaxConsecutiveWorkdays":6,"MinConsecutiveDayOffs":1,"MaxConsecutiveDayOffs":3,"Id":"af99224e-12de-41eb-b12a-a9ad00e544c0","Default":true,"Filters":[],"Name":"Default","PlanningGroupId":"ff69a203-3956-45ab-a7bc-a9ad00e544c0","BlockFinderType":0,"BlockSameStartTime":false,"BlockSameShiftCategory":false,"BlockSameShift":false,"Priority":-1,"MinFullWeekendsOff":0,"MaxFullWeekendsOff":8,"MinWeekendDaysOff":0,"MaxWeekendDaysOff":16,"PreferencePercent":80,"PlanningGroupName":""}];
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
		}).state('resourceplanner.editsetting', {
			params: {
				filterId: '',
				groupId: '',
			},
			url: '/planninggroup/:groupId/setting/:filterId',
			templateUrl: 'app/resourceplanner/resource_planner_planning_group_setting/groupsetting.createform.html',
			controller: 'planningGroupSettingEditController as vm',
			resolve: {
				planningGroupInfo: ['planningPeriodServiceNew', '$stateParams', function (planningPeriodServiceNew, $stateParams) {
					return planningPeriodServiceNew.getPlanningGroupById({planningGroupId: $stateParams.groupId}).$promise.then(function (data) {
						return data;
					});
				}]
			}
		}).state('resourceplanner.copyschedule', {
			url: '/copyschedule',
			templateUrl: 'app/resourceplanner/manageschedule/manageschedule.html',
			controller: 'ResourceplannerManageScheduleCtrl as vm'
		}).state('resourceplanner.importschedule', {
			url: '/importschedule',
			templateUrl: 'app/resourceplanner/manageschedule/manageschedule.html',
			controller: 'ResourceplannerManageScheduleCtrl as vm',
			params: {
				isImportSchedule: true
			}
		}).state('resourceplanner.tree', { // fortree
			url: '/tree',
			templateUrl: 'app/resourceplanner/tree_data/main.html',
			controller: 'TreeMainController as vm'
		}).state('resourceplanner.card', { // forcardpanel
			url: '/card',
			templateUrl: 'app/resourceplanner/tree_data/demo.html',
			controller: 'DemoMainController as vm'
		});
	}
})();
