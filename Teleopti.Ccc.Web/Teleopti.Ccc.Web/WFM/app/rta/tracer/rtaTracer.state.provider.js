'use strict';

angular.module('wfm.rtaTracer')
	.config(rtaTracer);

function rtaTracer($stateProvider) {
	$stateProvider.state('rtaTracer', {
		url: '/rtaTracer?userCode&trace',
		controller: 'RtaTracerController as vm',
		templateUrl: 'app/rta/tracer/rtaTracer.html'
	})
}
