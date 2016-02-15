
(function() {
	'use strict';

	angular.module('wfm')
		.service('WfmShortcuts', ['$state', 'ShortCuts', 'AreasSvrc', 'keyCodes',function($state, ShortCuts, AreasSvrc, keyCodes) {
			var service = {};

			var goToState = function(state) {
				$state.go(state);
			};

			AreasSvrc.getAreas().then(function(result){
				registerGlobalShortcuts(result);
			});

			function registerGlobalShortcuts(result) {
				var keys = [keyCodes.SHIFT];
				var j = keyCodes.ONE;
				var state;
				for (var i = 0; i < result.length && j <= keyCodes.NINE; i++) {
					state = result[i].InternalName;
					ShortCuts.registerKeySequence(j, keys, goToState.bind(this, state));
					j++;
				}
			};

			return service;
		}]);
})();
