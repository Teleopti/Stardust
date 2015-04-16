'use strict';

(function () {

	var outbound = angular.isDefined(angular.module('wfm.outbound')) ?
		angular.module('wfm.outbound') : angular.module('wfm.outbound', []);

	outbound.directive('formLocator', function() {
		return {
			restrict: 'A',
			link: function(scope, elem, attr) {
				var identifier = attr['name'];
				scope.$emit('formLocator.' + identifier);
			}
		}
	});
})();