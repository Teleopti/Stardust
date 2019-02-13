(function () {
	'use strict';

	angular
		.module('adminApp', ['ngRoute', 'ngMaterial', 'adminAppHttp', 'ngCookies', 'ngAria', 'ngclipboard', 'createForecastReadModelModule'])
		.config(function ($locationProvider, $routeProvider, $httpProvider) {

			$locationProvider.hashPrefix('');

			$routeProvider
				.when('/', {
					templateUrl: 'list.html'
				})
				.when('/login', {
					templateUrl: 'login.html'
				})
				.when('/details/:tenant', {
					templateUrl: 'details.html'
				})
				.when('/addSuperUser/:tenant', {
					templateUrl: 'addSuperUser.html'
				})
				.when('/import', {
					templateUrl: 'import.html'
				})
				.when('/create', {
					templateUrl: 'create.html'
				})
				.when('/users', {
					templateUrl: 'users.html'
				})
				.when('/userdetails/:id', {
					templateUrl: 'userdetails.html'
				})
				.when('/adduser', {
					templateUrl: 'adduser.html'
				})
				.when('/changepassword/:id', {
					templateUrl: 'changePassword.html'
				})
				.when('/configurationdetails/:key', {
					templateUrl: 'configurationDetails.html'
				})
				.when('/other', {
					templateUrl: 'other.html'
				})
				.when('/StardustDashboard', {
					templateUrl: 'StardustDashboard/stardust.html'
				})
				.when('/StardustDashboard/nodes', {
					templateUrl: 'StardustDashboard/nodes.html'
				})
				.when('/StardustDashboard/nodes/:nodeId', {
					templateUrl: 'StardustDashboard/nodedetails.html'
				})
				.when('/StardustDashboard/jobs', {
					templateUrl: 'StardustDashboard/jobs.html'
				})
				.when('/StardustDashboard/jobs/:jobId', {
					templateUrl: 'StardustDashboard/jobdetails.html'
				})
				.when('/StardustDashboard/queue', {
					templateUrl: 'StardustDashboard/queue.html'
				})
				.when('/StardustDashboard/queue/:jobId', {
					templateUrl: 'StardustDashboard/jobqueuedetails.html'
				})
				.when('/StardustDashboard/failedJobs', {
					templateUrl: 'StardustDashboard/failedJobs.html'
				})
				.when('/ETL', {
					templateUrl: 'Etl/etl.html'
				})
				.when('/ETL/history', {
					templateUrl: 'Etl/history.html'
				})
				.when('/ETL/schedule', {
					templateUrl: 'Etl/schedule.html'
				})
				.when('/ETL/config', {
					templateUrl: 'Etl/config.html'
				})

				.when('/HangfireDashboard', {
					templateUrl: 'HangfireDashboard.html'
				})
				.when('/HangfireMonitoring', {
					templateUrl: 'HangfireMonitoring.html'
				})
				.when('/HangfireStatistics', {
					templateUrl: 'HangfireMonitoring_LevelUp_HangfireStatistics_76139_76373.html'
				});
				
			$httpProvider.interceptors.push('httpInterceptor');
		});
})();
