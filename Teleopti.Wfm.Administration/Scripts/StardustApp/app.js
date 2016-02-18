(function () {
	'use strict';

	angular.module('app', ['ngRoute'])
		.config(function ($routeProvider) {
			$routeProvider
				.when('/', {
					templateUrl: 'listJob.html',
					controller: 'listJobController'
				})
			.when('/details/:jobId', {
				templateUrl: 'jobdetails.html',
				controller: 'detailsController'
			})
			;
		});
}())