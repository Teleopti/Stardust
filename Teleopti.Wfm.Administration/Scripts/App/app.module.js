(function () {
	'use strict';

	angular
		.module('adminApp', ['ngRoute', 'ngMaterial', 'adminAppHttp', 'ngCookies', 'ngAria'])
		.config(function ($routeProvider, $httpProvider) {

			$routeProvider
				.when('/', {
					templateUrl: 'list.html',
					//controller: 'listController'
				})
				.when('/login', {
					templateUrl: 'login.html',
					//controller: 'loginController'
				})
				.when('/details/:tenant', {
					templateUrl: 'details.html',
					//controller: 'detailsController'
				})
				.when('/addSuperUser/:tenant', {
					templateUrl: 'addSuperUser.html',
					//controller: 'addSuperUserController'
				})
				.when('/import', {
					templateUrl: 'import.html',
					//controller: 'importController'
				})
				.when('/create', {
					templateUrl: 'create.html',
					//controller: 'createController'
				})
				.when('/users', {
					templateUrl: 'users.html',
					//controller: 'usersController'
				})
				.when('/userdetails/:id', {
					templateUrl: 'userdetails.html',
					//controller: 'userdetailsController'
				})
				.when('/adduser', {
					templateUrl: 'adduser.html',
					//controller: 'adduserController'
				})
				.when('/changepassword/:id', {
					templateUrl: 'changePassword.html',
					//controller: 'changePasswordController'
				})
				.when('/configurationdetails/:key', {
					templateUrl: 'configurationDetails.html'
				})
				.when('/other', {
					templateUrl: 'other.html',
					//controller: 'otherController'
				})
				.when('/StardustDashboard', {
					templateUrl: 'StardustDashboard/stardust.html',
					//controller: 'summaryController'
				})
				.when('/StardustDashboard/nodes', {
					templateUrl: 'StardustDashboard/nodes.html',
					//controller: 'nodeController'
				})
				.when('/StardustDashboard/nodes/:nodeId', {
					templateUrl: 'StardustDashboard/nodedetails.html',
					//controller: 'nodeDetailsController'
				})
				.when('/StardustDashboard/jobs', {
					templateUrl: 'StardustDashboard/jobs.html',
				})
				.when('/StardustDashboard/jobs/:jobId', {
					templateUrl: 'StardustDashboard/jobdetails.html',
				})
				.when('/StardustDashboard/queue', {
					templateUrl: 'StardustDashboard/queue.html',
				})
				.when('/StardustDashboard/queue/:jobId', {
					templateUrl: 'StardustDashboard/jobqueuedetails.html',
				})
				.when('/StardustDashboard/failedJobs', {
					templateUrl: 'StardustDashboard/failedJobs.html',
				})

				.when('/HangfireDashboard', {
					templateUrl: 'HangfireDashboard.html',
				})
				.when('/HangfireMonitoring', {
					templateUrl: 'HangfireMonitoring.html'
				});

			$httpProvider.interceptors.push('httpInterceptor');
		});
})();
