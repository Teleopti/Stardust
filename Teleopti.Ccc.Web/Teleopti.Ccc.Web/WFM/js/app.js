'use strict';

var externalModules = angular.module('externalModules', ['ui.router',
	'ui.bootstrap',
	'ui.tree',
	'ngMaterial',
	'angularMoment',
	'ngSanitize',
	'pascalprecht.translate',
	'ui.grid',
	'ui.grid.autoResize',
	'ui.grid.exporter',
	'ui.grid.selection',
	'ngStorage'
]);

var wfm = angular.module('wfm', [
	'externalModules',
	'currentUserInfoService',
	'toggleService',
	'wfm.http',
	'wfm.permissions',
	'wfm.people',
	'wfm.outbound',
	'wfm.forecasting',
	'wfm.resourceplanner',
	'wfm.searching',
	'wfm.seatMap',
	'wfm.seatPlan',
	'wfm.notifications',
	'wfm.notice',
	'wfm.areas',
	'wfm.help',
	'wfm.rta',
	'wfm.start',
	'wfm.businessunits',
	'wfm.teamSchedule',
	'wfm.intraday',
	'wfm.requests'
]);

var toggles = {
	Wfm_RTA_ProperAlarm_34975: false
};

wfm.config([
	'$stateProvider', '$urlRouterProvider', '$translateProvider', '$httpProvider',
	function($stateProvider, $urlRouterProvider, $translateProvider, $httpProvider) {

		$urlRouterProvider.otherwise("/#");

		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html'
		}).state('forecasting', {
			url: '/forecasting',
			templateUrl: 'js/forecasting/html/forecasting.html',
			controller: 'ForecastingDefaultCtrl'
		}).state('forecasting.start', {
			params: {
				workloadId: undefined
			},
			templateUrl: 'js/forecasting/html/forecasting-overview.html',
			controller: 'ForecastingStartCtrl'
		}).state('forecasting.advanced', {
			params: {
				workloadId: {},
				workloadName: {}
			},
			templateUrl: 'js/forecasting/html/forecasting-advanced.html',
			controller: 'ForecastingAdvancedCtrl'
		}).state('forecasting.skillcreate', {
			url: '/skill/create',
			templateUrl: 'js/forecasting/html/skill-create.html',
			controller: 'ForecastingSkillCreateCtrl'
		}).state('forecasting-method', {
			params: {
				workloadId: {},
				period: {}
			},
			templateUrl: 'js/forecasting/html/forecasting-method.html',
			controller: 'ForecastingMethodCtrl'
		}).state('forecasting-intraday', {
			params: {
				workloadId: {},
				period: {}
			},
			templateUrl: 'js/forecasting/html/forecasting-intraday.html',
			controller: 'ForecastingIntradayCtrl'
		}).state('resourceplanner', {
			url: '/resourceplanner',
			templateUrl: 'js/resourceplanner/resourceplanner.html',
			controller: 'ResourcePlannerCtrl'
		}).state('resourceplanner.filter', {
			params:{
				filterId:{},
				periodId:{},
				isDefault:{}
			},
			url: '/dayoffrules',
			templateUrl: 'js/resourceplanner/resourceplanner-filters.html',
			controller: 'ResourceplannerFilterCtrl'
		}).state('resourceplanner.planningperiod', {
			url: '/planningperiod/:id',
			templateUrl: 'js/resourceplanner/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplanner.report', {
			params: {
				result: {},
				interResult: [],
				planningperiod: {}
			},
			templateUrl: 'js/resourceplanner/resourceplanner-report.html',
			controller: 'ResourceplannerReportCtrl'
		}).state('permissions', {
			url: '/permissions',
			templateUrl: 'js/permissions/permissions.html',
			controller: 'PermissionsCtrl'
		}).state('intraday', {
			url: '/intraday',
			templateUrl: 'js/intraday/intraday.html',
			controller: 'IntradayCtrl'
		}).state('outbound', {
			url: '/outbound',
			templateUrl: 'js/outbound/html/outbound.html',
			controller: 'OutboundDefaultCtrl'
		}).state('outbound.summary', {
			url: '/summary',
			views: {
				'': {
					templateUrl: 'js/outbound/html/outbound-overview.html'
				},
				'gantt@outbound.summary': {
					templateUrl: 'js/outbound/html/campaign-list-gantt.html',
					controller: 'CampaignListGanttCtrl'
				}
			}
		}).state('outbound.create', {
			url: '/create',
			templateUrl: 'js/outbound/html/campaign-create.html',
			controller: 'OutboundCreateCtrl'
		}).state('outbound.edit', {
			url: '/campaign/:Id',
			templateUrl: 'js/outbound/html/campaign-edit.html',
			controller: 'OutboundEditCtrl'
		}).state('people', {
			url: '/people',
			params: {
				selectedPeopleIds: [],
				currentKeyword: '',
				paginationOptions: {}
			},
			templateUrl: 'js/people/html/people.html',
			controller: 'PeopleDefaultCtrl'
		}).state('people.start', {
			templateUrl: 'js/people/html/people-list.html',
			controller: 'PeopleStartCtrl'
		}).state('people.selection', {
			params: {
				selectedPeopleIds: [],
				commandTag: {},
				currentKeyword: '',
				paginationOptions: {}
			},
			templateUrl: 'js/people/html/people-selection-cart.html',
			controller: 'PeopleCartCtrl as vm'
		}).state('seatPlan', {
			url: '/seatPlan/:viewDate',
			templateUrl: 'js/seatManagement/html/seatplan.html',
			controller: 'SeatPlanCtrl as seatplan'
		}).state('seatMap', {
			url: '/seatMap',
			templateUrl: 'js/seatManagement/html/seatmap.html'
		}).state('personSchedule', {
			url: '/teamSchedule',
			templateUrl: 'js/teamSchedule/html/schedule.html',
			controller: 'TeamScheduleCtrl as vm'
		}).state('requests', {
			url: '/requests',
			templateUrl: 'js/requests/html/requests.html',
			controller: 'RequestsCtrl as requests'
		});

		var rtaAgentsTemplate = function(elem, attr) {
			if (toggles.Wfm_RTA_ProperAlarm_34975)
				return 'js/rta/rta-agents-ProperAlarm_34975.html';
			return 'js/rta/rta-agents.html';
		};
		$stateProvider.state('rta', {
			url: '/rta',
			templateUrl: 'js/rta/rta-sites.html',
			controller: 'RtaCtrl',
		}).state('rta-teams', {
			url: '/rta/teams/:siteId',
			templateUrl: 'js/rta/rta-teams.html',
			controller: 'RtaTeamsCtrl'
		}).state('rta-agents', {
			url: '/rta/agents/:siteId/:teamId',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl'
		}).state('rta-agents-teams', {
			url: '/rta/agents-teams/?teamIds',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl',
			params: {
				teamIds: {
					array: true
				}
			}
		}).state('rta-agents-sites', {
			url: '/rta/agents-sites/?siteIds',
			templateUrl: rtaAgentsTemplate,
			controller: 'RtaAgentsCtrl',
			params: {
				siteIds: {
					array: true
				}
			}
		}).state('rta-agent-details', {
			url: '/rta/agent-details/:personId',
			templateUrl: 'js/rta/rta-agent-details.html',
			controller: 'RtaAgentDetailsCtrl'
		});

		$translateProvider.useSanitizeValueStrategy('sanitizeParameters');
		$translateProvider.useUrlLoader('../api/Global/Language');
		$translateProvider.preferredLanguage('en');
		$httpProvider.interceptors.push('httpInterceptor');
	}
]).run([
	'$rootScope', '$state', '$translate', 'HelpService', '$timeout', 'CurrentUserInfo', 'Toggle', '$q',
	function($rootScope, $state, $translate, HelpService, $timeout, currentUserInfo, toggleService, $q) {
		$rootScope.isAuthenticated = false;

		(function broadcastEventOnToggle() {
			$rootScope.$watchGroup(['toggleLeftSide', 'toggleRightSide'], function() {
				$timeout(function() {
					$rootScope.$broadcast('sidenav:toggle');
				}, 500);
			});
		})();

		function refreshContext(event, next, toParams) {
			currentUserInfo.initContext().then(function() {
				$rootScope.isAuthenticated = true;
				$translate.fallbackLanguage('en');

				$rootScope.$on('$stateChangeSuccess', function(event, next, toParams) {
					HelpService.updateState($state);
				});
				$state.go(next, toParams);
			});
		};

		toggleService.togglesLoaded.then(function() {
			toggles.Wfm_RTA_ProperAlarm_34975 = toggleService.Wfm_RTA_ProperAlarm_34975
		});

		$rootScope.$on('$stateChangeStart', function(event, next, toParams) {
			if (!currentUserInfo.isConnected()) {
				event.preventDefault();
				refreshContext(event, next, toParams);
				return;
			}
			if (!toggleService.togglesLoaded.$$state.status) {
				event.preventDefault();
				toggleService.togglesLoaded.then(function() {
					$state.go(next, toParams);
				})
				return;
			}
		});
	}

]);
