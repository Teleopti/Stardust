
(function() {
	'use strict';

	angular.module('wfm')
	.service('WfmShortcuts', ['$state', '$document', 'ShortCuts', 'AreasSvrc', 'keyCodes', function($state, $document, ShortCuts, AreasSvrc, keyCodes) {
		var service = {};
		var focusStyle = '1px solid #09F';
		service.tabbedTargets = [];
		service.isTabbedClicked;

		$document.on('keyup', function(event){
			if (event.keyCode == 9){
				service.isTabbedClicked = true;
				service.traverseWithTab(event);
			}
		});

		$document.on('mousedown', function (){
			if (service.isTabbedClicked)
				service.clearTabbedTarget();
		});

		service.traverseWithTab = function (event) {
			service.tabbedTargets.push(event);
			service.tabbedTargets[0].target.style.outline = focusStyle;

			if (service.tabbedTargets.length > 1){
				service.tabbedTargets[0].target.style.outline = "";
				service.tabbedTargets[1].target.style.outline = focusStyle;
				service.tabbedTargets.splice(0, 1);
			}
		};

		service.clearTabbedTarget = function () {
				service.tabbedTargets.forEach(function(e){
					e.target.style.outline = "";
				})
				service.tabbedTargets = [];
				service.isTabbedClicked = false;
		};


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
