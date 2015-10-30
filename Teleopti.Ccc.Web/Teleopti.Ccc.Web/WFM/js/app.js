'use strict';

var wfm_cultureInfo_numberFormat;

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
	'fakeDateTimeService',
	'toggleService',
	'outboundServiceModule',
	'RtaService',
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
    'wfm.intraday'
]);

wfm.config([
	'$stateProvider', '$urlRouterProvider', '$translateProvider', '$httpProvider', function ($stateProvider, $urlRouterProvider, $translateProvider, $httpProvider) {
		$urlRouterProvider.otherwise("/#");
		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html'
		}).state('forecasting', {
			url: '/forecasting',
			templateUrl: 'js/forecasting/html/forecasting.html',
			controller: 'ForecastingDefaultCtrl'
		}).state('forecasting.start', {
			params: { workloadId: undefined },
			templateUrl: 'js/forecasting/html/forecasting-overview.html',
			controller: 'ForecastingStartCtrl'
		}).state('forecasting.advanced', {
			params: { workloadId: {}, workloadName: {} },
			templateUrl: 'js/forecasting/html/forecasting-advanced.html',
			controller: 'ForecastingAdvancedCtrl'
		}).state('forecasting.skillcreate', {
			url: '/skill/create',
			templateUrl: 'js/forecasting/html/skill-create.html',
			controller: 'ForecastingSkillCreateCtrl'
		}).state('forecasting-method', {
			params: { workloadId: {}, period: {} },
			templateUrl: 'js/forecasting/html/forecasting-method.html',
			controller: 'ForecastingMethodCtrl'
		}).state('forecasting-intraday', {
			params: { workloadId: {}, period: {} },
			templateUrl: 'js/forecasting/html/forecasting-intraday.html',
			controller: 'ForecastingIntradayCtrl'
		}).state('resourceplanner', {
			url: '/resourceplanner',
			templateUrl: 'js/resourceplanner/resourceplanner.html',
			controller: 'ResourcePlannerCtrl'
		}).state('resourceplanner.planningperiod', {
			url: '/planningperiod/:id',
			templateUrl: 'js/resourceplanner/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplanner.report', {
			params: { result: {},interResult: [],planningperiod:{} },
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
			params: { selectedPeopleIds: [], currentKeyword: '', paginationOptions: {} },
			templateUrl: 'js/people/html/people.html',
			controller: 'PeopleDefaultCtrl'
		}).state('people.start', {
			templateUrl: 'js/people/html/people-list.html',
			controller: 'PeopleStartCtrl'
		}).state('people.selection', {
			params: { selectedPeopleIds: [], commandTag: {}, currentKeyword: '', paginationOptions: {} },
			templateUrl: 'js/people/html/people-selection-cart.html',
			controller: 'PeopleCartCtrl as vm'
		}).state('seatPlan', {
			url: '/seatPlan/:viewDate',
			templateUrl: 'js/seatManagement/html/seatplan.html',
			controller: 'SeatPlanCtrl as seatplan'
		}).state('seatMap', {
			url: '/seatMap',
			templateUrl: 'js/seatManagement/html/seatmap.html'
		}).state('rta-sites', {
			url: '/rta/sites',
			templateUrl: 'js/rta/rta-sites.html',
			controller: 'RtaCtrl',
		}).state('rta-teams', {
			url: '/rta/site/:siteId',
			templateUrl: 'js/rta/rta-teams.html',
			controller: 'RtaTeamsCtrl'
		}).state('rta-agents', {
			url: '/rta/site/:siteId/team/:teamId',
			templateUrl: 'js/rta/rta-agents.html',
			controller: 'RtaAgentsCtrl'
		}).state('rta-agents-selected', {
			url: '/rta/agentes/?teamIds',
			templateUrl: 'js/rta/rta-agents.html',
			controller: 'RtaAgentsCtrl',
			params: {teamIds: {array:true}}
		}).state('rta-agents-sites-selected', {
			url: '/rta/agents/?siteIds',
			templateUrl: 'js/rta/rta-agents.html',
			controller: 'RtaAgentsCtrl',
			params: {siteIds: {array:true}}
		}).state('personSchedule', {
			url: '/teamSchedule',
			templateUrl: 'js/teamSchedule/schedule.html',
			controller: 'TeamScheduleCtrl as vm'
		});



		$translateProvider.useSanitizeValueStrategy('sanitizeParameters');
		$translateProvider.useUrlLoader('../api/Global/Language');
		$translateProvider.preferredLanguage('en');
		$httpProvider.interceptors.push('httpInterceptor');
	}
]).run([
	'$rootScope', '$state', '$translate', 'HelpService', '$timeout', 'CurrentUserInfo','FakeDateTime',
	function ($rootScope, $state, $translate, HelpService, $timeout, currentUserInfo, fakeDateTime) {
		$rootScope.isAuthenticated = false;
		
		function broadcastEventOnToggle() {
			$rootScope.$watchGroup(['toggleLeftSide', 'toggleRightSide'], function() {
				$timeout(function() {
					$rootScope.$broadcast('sidenav:toggle');
				}, 500);
			});
		}

		$rootScope.$on('$stateChangeStart', function (event, next, toParams) {
			if (!currentUserInfo.isConnected()){
				event.preventDefault();
				currentUserInfo.initContext().then(function () {
					$state.go(next, toParams);
				});
			}
		});
		
		broadcastEventOnToggle();

		var startContext = currentUserInfo.initContext();
		startContext.then(function (data) {
			$rootScope.isAuthenticated = true; // could it be somewhere else than in rootscope ?
			$translate.fallbackLanguage('en');
			
			wfm_cultureInfo_numberFormat = data.NumberFormat; // should be extracted in user service as well

			var ab1 = new ABmetrics();
			ab1.baseUrl = 'http://wfmta.azurewebsites.net/';
			$rootScope.$on('$stateChangeSuccess', function (event, next, toParams) {
				ab1.sendPageView();
				HelpService.updateState($state.current.name);
			});

		});
	}
]);
