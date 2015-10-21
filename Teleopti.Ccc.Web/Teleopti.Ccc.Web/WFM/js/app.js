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
				'cards@outbound.summary': {
					templateUrl: 'js/outbound/html/campaign-list-cards.html',
					controller: 'OutboundListCardsCtrl'
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
	'$rootScope', '$http', '$state', '$translate', 'i18nService', 'amMoment', 'HelpService', '$sessionStorage', '$timeout', 'CurrentUserInfo',
	function ($rootScope, $http, $state, $translate, i18nService, angularMoment, HelpService, $sessionStorage, $timeout, currentUserInfo) {
		var timeout = Date.now() + 10000;
		$rootScope.isAuthenticated = false;

		function checkCurrentUser() {
			return $http.get('../api/Global/User/CurrentUser');
		};

		function userNotAuthenticatedHandler(data, state) {
			var unAuthenticatedStateCode = 401;
			if (state === unAuthenticatedStateCode) {
				if (window.location.hash) {
					var d = new Date();
					d.setTime(d.getTime() + (5 * 60 * 1000));
					var expires = 'expires=' + d.toUTCString();
					document.cookie = 'returnHash=WFM' + window.location.hash + '; ' + expires + '; path=/';
				}
			}

			$sessionStorage.$reset();
			window.location = 'Authentication';
		};

		function increaseTimeout() {
			timeout = Date.now() + 10000;
		};

		function broadcastEventOnToggle() {
			$rootScope.$watchGroup(['toggleLeftSide', 'toggleRightSide'], function() {
				$timeout(function() {
					$rootScope.$broadcast('sidenav:toggle');
				}, 500);
			});
		}

		$rootScope.$on('$stateChangeStart', function (event, next, toParams) {

			if (Date.now() > timeout) { // TODO : extract it in a service
				event.preventDefault();
				var context = checkCurrentUser();
				context.error(function (data, state) {
					userNotAuthenticatedHandler(data, state);
				});
				context.success(function (data) {
					increaseTimeout();
					$state.go(next, toParams);
				});
			}
		});

		broadcastEventOnToggle();

		var startContext = checkCurrentUser();
		startContext.error(function (data,state) {
			userNotAuthenticatedHandler(data, state);
		});
		startContext.success(function (data) {
			$rootScope.isAuthenticated = true;
			wfm_cultureInfo_numberFormat = data.NumberFormat;
			$translate.fallbackLanguage('en');
			$translate.use(data.Language);
			angularMoment.changeLocale(data.DateFormatLocale);
			increaseTimeout();

			currentUserInfo.SetCurrentUserInfo(data);

			// i18nService is for UI Grid localization.
			// Languages supported by it is less than languages in server side (Refer to http://ui-grid.info/docs/#/tutorial/104_i18n).
			// Need do some primary language checking.
			var currentLang = "en";
			var serverSideLang = data.Language.toLowerCase();
			var dashIndex = serverSideLang.indexOf("-");
			var primaryLang = dashIndex > -1 ? serverSideLang.substring(0, dashIndex) : serverSideLang;
			var langs = i18nService.getAllLangs();
			if (langs.indexOf(serverSideLang) > -1) {
				currentLang = serverSideLang;
			} else if (langs.indexOf(primaryLang) > -1) {
				currentLang = primaryLang;
			}
			i18nService.setCurrentLang(currentLang);

			var ab1 = new ABmetrics();
			ab1.baseUrl = 'http://wfmta.azurewebsites.net/';
			$rootScope.$on('$stateChangeSuccess', function (event, next, toParams) {
				ab1.sendPageView();
				HelpService.updateState($state.current.name);
			});


			var buid = $sessionStorage.buid;
			if (buid) {
				$http.defaults.headers.common['X-Business-Unit-Filter'] = buid;
			}
		});
	}
]);




