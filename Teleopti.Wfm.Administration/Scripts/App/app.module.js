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

		.when('/other', {
			templateUrl: 'other.html',
			controller: 'otherController'
		});
});