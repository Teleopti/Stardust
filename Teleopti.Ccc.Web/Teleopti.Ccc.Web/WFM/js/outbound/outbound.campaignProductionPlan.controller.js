(function () {
	'use strict';

	angular.module('wfm.outbound')
        .controller('OutboundProductionPlanCtrl', [
            '$scope', '$state', 
            productionPlanCtrl
        ]);

	function productionPlanCtrl($scope, $state) {

		$scope.backToList = function() {
			$state.go('outbound');
		};

	}



})();