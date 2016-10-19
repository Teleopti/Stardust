(function () {
	'use strict';

	angular
		.module('adminApp', ['ngRoute', 'ngMaterial', 'adminAppHttp', 'ngCookies'])
		.config(function ($routeProvider, $httpProvider) {

			$routeProvider
				.when('/', {
					templateUrl: 'list.html',
					controller: 'listController'
				})
				.when('/login', {
					templateUrl: 'login.html',
					controller: 'loginController'
				})
				.when('/details/:tenant', {
					templateUrl: 'details.html',
					controller: 'detailsController'
				})
				.when('/addSuperUser/:tenant', {
					templateUrl: 'addSuperUser.html',
					controller: 'addSuperUserController'
				})
				.when('/import', {
					templateUrl: 'import.html',
					controller: 'importController'
				})
				.when('/create', {
					templateUrl: 'create.html',
					controller: 'createController'
				})
				.when('/users', {
					templateUrl: 'users.html',
					controller: 'usersController'
				})
				.when('/userdetails/:id', {
					templateUrl: 'userdetails.html',
					controller: 'userdetailsController'
				})
				.when('/adduser', {
					templateUrl: 'adduser.html',
					controller: 'adduserController'
				})
				.when('/changepassword/:id', {
					templateUrl: 'changePassword.html',
					controller: 'changePasswordController'
				})
				.when('/other', {
					templateUrl: 'other.html',
					controller: 'otherController'
				})
				.when('/StardustDashboard', {
					templateUrl: 'StardustDashboard/stardust.html',
					controller: 'summaryController'
				})
				.when('/StardustDashboard/nodes', {
					templateUrl: 'StardustDashboard/nodes.html',
					controller: 'nodeController'
				})
				.when('/StardustDashboard/nodes/:nodeId', {
					templateUrl: 'StardustDashboard/nodedetails.html',
					controller: 'nodeDetailsController'
				})
				.when('/StardustDashboard/jobs_', {
					templateUrl: 'StardustDashboard/jobs.html',
					controller: 'jobController'
				})
				.when('/StardustDashboard/jobs_/:jobId', {
					templateUrl: 'StardustDashboard/jobdetails.html',
					controller: 'jobDetailsController'
				})
<<<<<<< dest
				.when('/jobqueuedetails/:jobId', {
					templateUrl: 'StardustDashboard/jobqueuedetails.html',
					controller: 'jobQueueDetailsController'
				})
					templateUrl: 'StardustDashboard/jobdetails.html',
					controller: 'jobDetailsController'
				})
				.when('/StardustDashboard/queue', {
					templateUrl: 'StardustDashboard/queue.html',
					controller: 'jobQueueController'
				})
=======
>>>>>>> source
				.when('/HangfireDashboard', {
					templateUrl: 'HangfireDashboard.html',
					controller: 'hangfireController'
				});
			$httpProvider.interceptors.push('httpInterceptor');
		});
})();
