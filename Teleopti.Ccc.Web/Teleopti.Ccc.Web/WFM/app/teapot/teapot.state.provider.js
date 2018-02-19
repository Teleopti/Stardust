'use strict';

angular.module('wfm.teapot')
	.config(teapot);

function teapot($stateProvider) {
	$stateProvider.state('teapot', {
		url: '/teapot',
		controller: 'TeapotController as vm',
		templateUrl: 'app/teapot/view.html'
	})
}
