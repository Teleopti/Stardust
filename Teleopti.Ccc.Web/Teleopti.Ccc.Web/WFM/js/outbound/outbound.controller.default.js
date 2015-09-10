(function() {
	'use strict';
	angular.module('wfm.outbound').controller('OutboundDefaultCtrl', [
		'$state', '$location', function ($state, $location) {		
			if ($location.url() == $state.current.url )			
				$state.go('outbound.summary');
		}
	]);
})();