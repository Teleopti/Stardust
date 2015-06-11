angular
	 .module('adminApp', ['ngRoute'])
.config(function ($routeProvider) {
	$routeProvider

		.when('/', {
			templateUrl: 'list.html',
			controller: 'listController'
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