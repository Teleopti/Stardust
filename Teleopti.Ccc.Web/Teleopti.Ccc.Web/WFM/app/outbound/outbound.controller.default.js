(function() {
	'use strict';
	angular.module('wfm.outbound').controller('OutboundDefaultCtrl', [
		'$scope', '$state', '$location', function($scope, $state, $location) {

			$scope.$on('unauthorized.outbound', function() {
				$location.path("/#");
			});
		}
	]);
})();