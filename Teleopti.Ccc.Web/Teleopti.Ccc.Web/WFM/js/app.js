'use strict';

var externalModules = angular.module('externalModules', ['ui.router',
	'ui.bootstrap',
	'ui.tree',
	'ngMaterial',
	'angularMoment',
	'pascalprecht.translate',
	'ui.grid',
	'ui.grid.autoResize',
	'ui.grid.exporter',
	'ui.grid.selection',
	'ngStorage'
]);

var wfm = angular.module('wfm', [
	'externalModules',
	'toggleService',
	'outboundServiceModule',
	'RtaService',
	'wfmCtrls',
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
	'wfm.businessunits'
]);

wfm.config([
	'$stateProvider', '$urlRouterProvider', '$translateProvider', function ($stateProvider, $urlRouterProvider, $translateProvider) {
		$urlRouterProvider.otherwise("forecasting");		
		$stateProvider.state('main', {
			url: '/',
			templateUrl: 'html/main.html',
			controller: 'MainCtrl'
		}).state('forecasting', {
			url: '/forecasting',
			templateUrl: 'html/forecasting/forecasting.html',
			controller: 'ForecastingStartCtrl'
		}).state('forecasting-method', {
			params: { workloadId: {}, period: {} },
			templateUrl: 'html/forecasting/forecasting-method.html',
			controller: 'ForecastingMethodCtrl'
		}).state('forecasting-intraday', {
			params: { workloadId: {}, period: {} },
			templateUrl: 'html/forecasting/forecasting-intraday.html',
			controller: 'ForecastingIntradayCtrl'
		}).state('forecasting-advanced', {
			params: { workloadId: {}, workloadName: {} },
			templateUrl: 'html/forecasting/forecasting-advanced.html',
			controller: 'ForecastingAdvancedCtrl'
		}).state('resourceplanner', {
			url: '/resourceplanner',
			templateUrl: 'js/resourceplanner/resourceplanner.html',
			controller: 'ResourcePlannerCtrl'
		}).state('resourceplanner.planningperiod', {
			url: '/planningperiod/:id',
			templateUrl: 'js/resourceplanner/planningperiods.html',
			controller: 'PlanningPeriodsCtrl'
		}).state('resourceplanner.report', {
			params: { result: {},interResult: [] },
			templateUrl: 'js/resourceplanner/resourceplanner-report.html',
			controller: 'ResourceplannerReportCtrl'
		}).state('permissions', {
			url: '/permissions',
			templateUrl: 'html/permissions/permissions.html',
			controller: 'PermissionsCtrl'
		}).state('outbound', {
			url: '/outbound',
			templateUrl: 'html/outbound/outbound.html',
			controller: 'OutboundDefaultCtrl'
		}).state('outbound.summary', {
			url: '/summary',			
			views: {
				'': {
					templateUrl: 'html/outbound/outbound-overview.html'
				},
				'cards@outbound.summary': {
					templateUrl: 'html/outbound/campaign-list-cards.html',
					controller: 'OutboundListCardsCtrl'
				},
				'gantt@outbound.summary': {
					templateUrl: 'html/outbound/campaign-list-gantt.html',
					controller: 'CampaignListGanttCtrl'
				}			
			}			
		}).state('outbound.create', {
			url: '/create',
			templateUrl: 'html/outbound/campaign-create.html',
			controller: 'OutboundCreateCtrl'
		}).state('outbound.edit', {
			url: '/campaign/:Id',
			templateUrl: 'html/outbound/campaign-edit.html',
			controller: 'OutboundEditCtrl'
		}).state('people', {
			url: '/people',
			params: { selectedPeopleIds: [], currentKeyword: '', paginationOptions: {} },
			templateUrl: 'html/people/people.html',
			controller: 'PeopleCtrl'
		}).state('people-selection-cart', {
			params: { selectedPeopleIds: [], commandTag: {}, currentKeyword: '', paginationOptions: {} },
			templateUrl: 'html/people/people-selection-cart.html',
			controller: 'PeopleCartCtrl as vm'
		}).state('seatPlan', {
			url: '/seatPlan/:viewDate',
			templateUrl: 'js/seatManagement/html/seatplan.html',
			controller: 'SeatPlanCtrl as seatplan'
		}).state('seatMap', {
			url: '/seatMap',
			templateUrl: 'js/seatManagement/html/seatmap.html'
		}).state('rta-sites', {
			url: '/rta/sites:id',
			templateUrl: 'js/rta/rta-sites.html',
			controller: 'RtaCtrl'
		}).state('rta-teams', {
			url: '/rta/teams',
			templateUrl: 'js/rta/rta-teams.html',
			controller: 'RtaCtrl'
		}).state('rta-agents', {
			url: '/rta/agents',
			templateUrl: 'js/rta/rta-agents.html',
			controller: 'RtaAgentsCtrl'
		});

		$translateProvider.useUrlLoader('../api/Global/Language');
		$translateProvider.preferredLanguage('en');
	}
]).run([
	'$rootScope', '$http', '$state', '$translate', 'i18nService', 'amMoment', 'HelpService', '$sessionStorage', '$timeout',
	function ($rootScope, $http, $state, $translate, i18nService, angularMoment, HelpService, $sessionStorage, $timeout) {
		var timeout = Date.now() + 10000;
		$rootScope.isAuthenticated = false;

		function checkCurrentUser() {
			return $http.get('../api/Global/User/CurrentUser');
		};

		function userNotAuthenticatedHandler() {
			if (window.location.hash) {
				var d = new Date();
				d.setTime(d.getTime() + (5 * 60 * 1000));
				var expires = 'expires=' + d.toUTCString();
				document.cookie = 'returnHash' + '=' + window.location.hash + '; ' + expires + '; path=/';
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
				context.error(userNotAuthenticatedHandler);
				context.success(function (data) {
					increaseTimeout();
					$state.go(next, toParams);
				});
			}
		});

		broadcastEventOnToggle();

		var startContext = checkCurrentUser();
		startContext.error(userNotAuthenticatedHandler);
		startContext.success(function (data) {
			$rootScope.isAuthenticated = true;
			$translate.fallbackLanguage('en');
			$translate.use(data.Language);
			angularMoment.changeLocale(data.DateFormat);
			increaseTimeout();

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