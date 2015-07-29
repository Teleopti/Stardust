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
		.when('/import', {
			templateUrl: 'import.html',
			controller: 'importController'
		})
		.when('/empty', {
			templateUrl: 'empty.html',
			controller: 'emptyController'
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
		.when('/other', {
			templateUrl: 'other.html',
			controller: 'otherController'
		});
});