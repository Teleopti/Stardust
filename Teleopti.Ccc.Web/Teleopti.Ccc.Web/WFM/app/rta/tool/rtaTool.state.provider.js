'use strict';

angular.module('wfm.rtaTool')
	.config(rtaStates);

function rtaStates($stateProvider) {
	$stateProvider.state('rtaTool', {
		url: '/rtaTool',
		controller: 'RtaToolController as vm',
		templateUrl: 'app/rta/tool/rtaTool.html',
		resolve: {
			organization: function (rtaToolService) {
				return rtaToolService.getOrganization();
			}
		}
	})
};
