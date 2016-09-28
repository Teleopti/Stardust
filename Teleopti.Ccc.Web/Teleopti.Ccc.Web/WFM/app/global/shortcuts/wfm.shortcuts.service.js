
(function() {
	'use strict';

	angular.module('wfm')
	.service('WfmShortcuts', ['$state', '$document', 'ShortCuts', 'keyCodes', function($state, $document, ShortCuts, keyCodes) {
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

		return service;
	}]);
})();
