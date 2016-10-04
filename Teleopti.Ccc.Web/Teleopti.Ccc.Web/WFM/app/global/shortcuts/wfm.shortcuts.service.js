(function() {
    'use strict';

    angular
        .module('wfm')
        .service('WfmShortcuts', WfmShortcuts);

    WfmShortcuts.$inject = ['$state', '$document', 'ShortCuts', 'keyCodes'];
    function WfmShortcuts($state, $document, ShortCuts, keyCodes) {

			var focusStyle = '1px solid #09F';
			var tabbedTargets = [];
			var isTabbedClicked;

			$document.on('keyup', function(event){
				if (event.keyCode == 9){
					isTabbedClicked = true;
					traverseWithTab(event);
				}
			});

			$document.on('mousedown', function (){
				if (isTabbedClicked)
					clearTabbedTarget();
			});

			function traverseWithTab(event) {
				tabbedTargets.push(event);
				tabbedTargets[0].target.style.outline = focusStyle;

				if (tabbedTargets.length > 1){
					tabbedTargets[0].target.style.outline = "";
					tabbedTargets[1].target.style.outline = focusStyle;
					tabbedTargets.splice(0, 1);
				}
			};

			function clearTabbedTarget() {
					tabbedTargets.forEach(function(e){
						e.target.style.outline = "";
					})
					tabbedTargets = [];
					isTabbedClicked = false;
			};
    }
})();

//
//
// (function() {
// 	'use strict';
//
// 	angular.module('wfm')
// 	.service('WfmShortcuts', ['$state', '$document', 'ShortCuts', 'keyCodes', function($state, $document, ShortCuts, keyCodes) {
// 		var service = {};
// 		var focusStyle = '1px solid #09F';
// 		service.tabbedTargets = [];
// 		service.isTabbedClicked;
//
// 		$document.on('keyup', function(event){
// 			if (event.keyCode == 9){
// 				service.isTabbedClicked = true;
// 				service.traverseWithTab(event);
// 			}
// 		});
//
// 		$document.on('mousedown', function (){
// 			if (service.isTabbedClicked)
// 				service.clearTabbedTarget();
// 		});
//
// 		service.traverseWithTab = function (event) {
// 			service.tabbedTargets.push(event);
// 			service.tabbedTargets[0].target.style.outline = focusStyle;
//
// 			if (service.tabbedTargets.length > 1){
// 				service.tabbedTargets[0].target.style.outline = "";
// 				service.tabbedTargets[1].target.style.outline = focusStyle;
// 				service.tabbedTargets.splice(0, 1);
// 			}
// 		};
//
// 		service.clearTabbedTarget = function () {
// 				service.tabbedTargets.forEach(function(e){
// 					e.target.style.outline = "";
// 				})
// 				service.tabbedTargets = [];
// 				service.isTabbedClicked = false;
// 		};
//
// 		return service;
// 	}]);
// })();
