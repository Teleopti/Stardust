angular
	 .module('adminApp', ['ngRoute'])
.config(function ($routeProvider) {
	$routeProvider

		.when('/', {
			templateUrl: 'list.html',
			controller: 'listController'
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
			templateUrl: 'StardustDashboard/listJob.html',
			controller: 'listJobController'
		})
		.when('/jobdetails/:jobId', {
			templateUrl: 'StardustDashboard/jobdetails.html',
			controller: 'jobDetailsController'
		});
});