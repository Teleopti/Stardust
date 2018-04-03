(function() {
	var intraday = angular
		.module('wfm.intraday', [
			'gridshore.c3js.chart',
			'ngResource',
			'ui.router',
			'wfm.notice',
			'pascalprecht.translate',
			'wfm.autofocus',
			'toggleService',
			'angularMoment',
			'wfm.dateOffset',
			'wfm.utilities',
			'wfm.skillGroup',
			'skillGroupService'
		])
		.run(['$rootScope', '$state', '$location', intradayModule]);

	function intradayModule($rootScope, $state, $location) {
		var rs = $rootScope;
		rs.$on('$stateChangeSuccess', function(event, toState) {
			if ($location.url() === $state.current.url && toState.name === 'intraday') $state.go('intraday.area');
		});
	}

	//Polyfill for Object.assign (for IE)
	if (typeof Object.assign != 'function') {
		// Must be writable: true, enumerable: false, configurable: true
		Object.defineProperty(Object, 'assign', {
			value: function assign(target, varArgs) {
				// .length of function is 2
				'use strict';
				if (target == null) {
					// TypeError if undefined or null
					throw new TypeError('Cannot convert undefined or null to object');
				}

				var to = Object(target);

				for (var index = 1; index < arguments.length; index++) {
					var nextSource = arguments[index];

					if (nextSource != null) {
						// Skip over if undefined or null
						for (var nextKey in nextSource) {
							// Avoid bugs when hasOwnProperty is shadowed
							if (Object.prototype.hasOwnProperty.call(nextSource, nextKey)) {
								to[nextKey] = nextSource[nextKey];
							}
						}
					}
				}
				return to;
			},
			writable: true,
			configurable: true
		});
	}
})();
