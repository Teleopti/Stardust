'use strict';

(function() {
	angular.module('wfm.seatPlan').directive('seatplanMonthChangeEmitter', seatplanMonthChangeEmitter);

	seatplanMonthChangeEmitter.$inject = ['$timeout'];
	function seatplanMonthChangeEmitter($timeout) {		
		return {
			restrict: 'A',
			priority: 1001,			
			link: postlink
		}

		function postlink(scope, elem) {
			$timeout(function () {
				
				var innerScope = angular.element(elem).isolateScope();				
				scope.$watch(function() {
					return innerScope.activeDt.date.getMonth();
				}, function(nv, ov) {
					if (nv === ov) return;
					scope.$emit('seatplan.month.changed', {
						activeDate: innerScope.activeDt.date
					});
				});
			}, 200);
		}

	}

})();