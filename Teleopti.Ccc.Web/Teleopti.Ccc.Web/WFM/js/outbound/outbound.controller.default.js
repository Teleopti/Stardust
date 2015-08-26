(function() {
	'use strict';
	angular.module('wfm.outbound').controller('OutboundDefaultCtrl', [
		'$state', function($state) {
			$state.go('outbound.summary');
		}
	]);
})();