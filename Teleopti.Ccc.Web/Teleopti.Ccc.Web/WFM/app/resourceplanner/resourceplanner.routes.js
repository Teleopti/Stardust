(function() {
	'use strict';

	angular.module('wfm.resourceplanner').config(stateConfig);

	function stateConfig($stateProvider, ToggleProvider) {
		$stateProvider
			.state('resourceplanner', {
				url: '/resourceplanner',
				templateUrl: 'app/resourceplanner/resourceplanner.html'
			})
			.state('resourceplanner.overview', {
				url: '',
				templateUrl: function() {
					return ToggleProvider.WFM_Plans_Redesign_81198
						? 'app/resourceplanner/plans-modern.html'
						: 'app/resourceplanner/planning_group/planninggroups.html';
				}, 
				controller: 'planningGroupsController as vm',
				resolve: {
					planningGroups: [
						'planningGroupService',
						function(planningGroupService) {
							return planningGroupService.getPlanningGroups().$promise.then(function(data) {
								return data;
							});
						}
					]
				}
			})
			.state('resourceplanner.createplanninggroup', {
				url: '/createplanninggroup',
				templateUrl: 'app/resourceplanner/planning_group/planninggroup.createform.html',
				controller: 'planningGroupFormController as vm',
				resolve: {
					editPlanningGroup: function() {
						return null;
					}
				}
			})
			.state('resourceplanner.editplanninggroup', {
				url: '/planninggroup/:groupId/edit/:planningPeriodId',
				templateUrl: 'app/resourceplanner/planning_group/planninggroup.createform.html',
				controller: 'planningGroupFormController as vm',
				params: {
					groupId: '',
					planningPeriodId: ''
				},
				resolve: {
					editPlanningGroup: [
						'planningGroupService',
						'$stateParams',
						function(planningGroupService, $stateParams) {
							return planningGroupService.getPlanningGroupById({ id: $stateParams.groupId }).$promise;
						}
					]
				}
			})
			.state('resourceplanner.selectplanningperiod', {
				url: '/planninggroup/:groupId/selectplanningperiod',
				templateUrl: 'app/resourceplanner/planning_period/planningperiod.select.html',
				controller: 'planningPeriodSelectController as vm',
				params: {
					groupId: ''
				},
				resolve: {
					planningPeriods: [
						'planningPeriodServiceNew',
						'$stateParams',
						function(planningPeriodServiceNew, $stateParams) {
							return planningPeriodServiceNew
								.getPlanningPeriodsForPlanningGroup({ planningGroupId: $stateParams.groupId })
								.$promise.then(function(data) {
									return data;
								});
						}
					],
					planningGroupInfo: [
						'planningPeriodServiceNew',
						'$stateParams',
						function(planningPeriodServiceNew, $stateParams) {
							return planningPeriodServiceNew
								.getPlanningGroupById({ planningGroupId: $stateParams.groupId })
								.$promise.then(function(data) {
									return data;
								});
						}
					]
				}
			})
			.state('resourceplanner.planningperiodoverview', {
				url: '/planninggroup/:groupId/detail/:ppId',
				templateUrl: function() {
					return ToggleProvider.WFM_Plans_HeatMap_79112
						? 'app/resourceplanner/plans-planningperiodoverview-modern.html'
						: 'app/resourceplanner/planning_period/planningperiod.overview.html';
				},
				controller: 'planningPeriodOverviewController as vm',
				params: {
					groupId: '',
					ppId: ''
				},
				resolve: {
					selectedPp: [
						'planningPeriodServiceNew',
						'$stateParams',
						function(planningPeriodServiceNew, $stateParams) {
							return planningPeriodServiceNew
								.getPlanningPeriod({ id: $stateParams.ppId })
								.$promise.then(function(data) {
									return data;
								});
						}
					],
					planningGroupInfo: [
						'planningPeriodServiceNew',
						'$stateParams',
						function(planningPeriodServiceNew, $stateParams) {
							return planningPeriodServiceNew
								.getPlanningGroupById({ planningGroupId: $stateParams.groupId })
								.$promise.then(function(data) {
									return data;
								});
						}
					]
				}
			})
			.state('resourceplanner.editsetting', {
				params: {
					filterId: '',
					groupId: ''
				},
				url: '/planninggroup/:groupId/setting/:filterId',
				templateUrl: 'app/resourceplanner/planning_group_setting/groupsetting.createform.html',
				controller: 'planningGroupSettingEditController as vm',
				resolve: {
					planningGroupInfo: [
						'planningPeriodServiceNew',
						'$stateParams',
						function(planningPeriodServiceNew, $stateParams) {
							return planningPeriodServiceNew
								.getPlanningGroupById({ planningGroupId: $stateParams.groupId })
								.$promise.then(function(data) {
									return data;
								});
						}
					]
				}
			})
			.state('resourceplanner.copyschedule', {
				url: '/copyschedule',
				templateUrl: 'app/resourceplanner/manageschedule/manageschedule.html',
				controller: 'ResourceplannerManageScheduleCtrl as vm'
			})
			.state('resourceplanner.importschedule', {
				url: '/importschedule',
				templateUrl: 'app/resourceplanner/manageschedule/manageschedule.html',
				controller: 'ResourceplannerManageScheduleCtrl as vm',
				params: {
					isImportSchedule: true
				}
			})
			.state('resourceplanner.tree', {
				// fortree
				url: '/tree',
				templateUrl: 'app/resourceplanner/tree_data/main.html',
				controller: 'TreeMainController as vm'
			})
			.state('resourceplanner.card', {
				// forcardpanel
				url: '/card',
				templateUrl: 'app/resourceplanner/tree_data/demo.html',
				controller: 'DemoMainController as vm'
			});
	}
})();
